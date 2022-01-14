using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveAUV : MonoBehaviour
{
    [SerializeField]
    GameObject Boat;
    [SerializeField]
    GameObject Radar;
    [SerializeField]
    Toggle Auto;

    public bool playerControlled;
    public List<KeyValuePair<Vector3, bool>> points = new List<KeyValuePair<Vector3, bool>>(2);

    private Rigidbody rb;
    private RaycastHit hit;
    private bool auto;
    private bool addPoint1;
    private bool addPoint2;
    private float vesselSet;
    private bool stopRotate;
    private bool stopTranslateH;
    private bool justCollided;
    private bool destinationAddedToPoints = false;
    private bool destinationEntered = false;
    private Vector3 newDest;
    private float ColRange = 60;
    private int maxBoatDist = 150; //maximum distance from the boat
    private int time;
    int layerMask;
    float speed = 0;
    float dir = 0;
    float hDir = 0;//horizontal Direction
    float vDir = 0;//vertical Direction

    // Start is called before the first frame update
    void Start()
    {
        //Adding the first and second point to add between
        points.Add(new KeyValuePair<Vector3, bool>(Boat.transform.position, false));
        points.Add(new KeyValuePair<Vector3, bool>(Boat.transform.position, false));
        points.Add(new KeyValuePair<Vector3, bool>(transform.position, false));
        auto = true;
        addPoint1 = true;
        addPoint2 = true;
        rb = GetComponent<Rigidbody>();
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        justCollided = false;
        //Debug.Log(points.Count);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(points[points.Count - 2]);
        //Add the timer so it updates slower than the boat, so it's more realistic(MAYBE)
        destinationEntered = Boat.GetComponent<MoveBoat>().DestinationEntered();
        if (vesselSet == 2) Auto.isOn = auto;

        //updating the boat and AUV's position in the points list
        points[0] = new KeyValuePair<Vector3, bool>(Boat.transform.position, false);
        points[points.Count - 1] = (new KeyValuePair<Vector3, bool>(transform.position, false));

        //Setting the number based on whether the camera is on the Boat or AUV......
        vesselSet = Boat.GetComponent<VesselManager>().vesselSet;

        //Toggling the autopilot for the AUV
        if (Keyboard.current.tKey.wasPressedThisFrame && vesselSet == 2)
            auto = !auto;

        //If a destination is entered, then start adding points as you move towards that destination, else keep facing the boat
        if (destinationEntered && auto)
        {
            newDest = Boat.GetComponent<MoveBoat>().GetDestCoord();

            if (!destinationAddedToPoints)
            {
                points[points.Count - 2] = (new KeyValuePair<Vector3, bool>(newDest, false));
                destinationAddedToPoints = true;
            }
            if (!newDest.Equals(points[points.Count - 2].Key))
                destinationAddedToPoints = false;
        }
        else
        {
            points[points.Count - 2] = (new KeyValuePair<Vector3, bool>(Boat.transform.position, false));
        }

        //if the autopilot is off, while were on the AUV we are able to comtrol it, else it moves automatically
        if (vesselSet == 2 && !auto)
        {
            playerControlled = true;
            gameObject.GetComponent<BoatAlignNormal>()._throttleBias = 0;
            gameObject.GetComponent<BoatAlignNormal>()._steerBias = 0;
            MoveV();
        }
        else
        {
            playerControlled = false;
            Col();
            AutoMoveH();
            Rotate();
            Stabilize();
        }

        //If we see the conditon to stop, and we're not controlling the AUV, then stop
        if (StopCheck()) {
            Stop();
            //Debug.Log("Stopped");    
        }
        
        //updating whether we're controlling the AUV or not
        gameObject.GetComponent<BoatAlignNormal>()._playerControlled = playerControlled;
    }

    //used to move the AUV up or down, depending on if the q or e button is pressed
    private void MoveV()
    {
        if (Keyboard.current.qKey.isPressed && (GetComponent<BoatAlignNormal>()._bottomH > 1.5f))
            GetComponent<BoatAlignNormal>()._bottomH -= 0.05f;

        if (Keyboard.current.eKey.isPressed)
            GetComponent<BoatAlignNormal>()._bottomH += 0.05f;
    }

    private void Stabilize()
    {
        if (GetComponent<BoatAlignNormal>()._bottomH > 1.5f)
            GetComponent<BoatAlignNormal>()._bottomH -= 0.05f;
        transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }

    private void AutoMoveV()
    {}

    private void AutoMoveH()
    {
        float distance = Vector3.Distance(transform.position, points[points.Count-2].Key/*Boat.transform.position*/);

        gameObject.GetComponent<BoatAlignNormal>()._throttleBias = ((distance > 50 && !stopTranslateH) ? 1.5f : speed);
    }

    private void Rotate()
    {
        gameObject.GetComponent<BoatAlignNormal>()._steerBias = (((Angle() < -0.1 || Angle() > 0.1) && !stopRotate) ? Dir() : dir);
    }

    //For avoiding collision with different objects
    private void Col()
    {
        Debug.DrawRay(Radar.transform.position, Radar.transform.TransformDirection(Vector3.forward) * ColRange, Color.red, 0.1f);
        if (Physics.Raycast(Radar.transform.position, Radar.transform.TransformDirection(Vector3.forward), out hit, ColRange, layerMask))
        {
            Vector3 currentDir = transform.forward;
            Vector3 contactDir = hit.point - transform.position;
            float a = Vector3.Angle(currentDir, contactDir);

            //float dir = (((ColRange - hit.distance) * (90 - a)) / (ColRange * 90)) / ((90 - a) / 90);
            //float dir = (ColRange - hit.distance) / ColRange;

            //This is so it turns less the less the object is in from of it
            float newMax = ((90 - a) / 90) * 1.5f;

            if (a < 7.5)
            {
                //prevent the boat from moving normal speeds if there is possibility of collision
                stopTranslateH = true;

                //Manage speed based on how far an object is to boat
                speed = (hit.distance / ColRange) * 1.5f;
            }
            else
            {
                stopTranslateH = false;
            }

            //If there is something we may need to worry about 90 deg to our left or right
            if (a < 90)
            {
                //stop normal rotation to face destination because of the possibility of collision
                stopRotate = true;

                //set how much we need to turn using the newMax based on how close th eobject is
                dir = ((ColRange - hit.distance) * newMax) / ((ColRange * 2) + hit.distance);

                //Set whether we need to turn left or right
                dir *= ((Vector3.Cross(currentDir, contactDir).y < transform.position.y) ? 1 : -1);
            }
            else
            {
                stopRotate = false;
            }

            justCollided = true;
        }

        //If there is nothing to worry about 90 deg to our left or right then continue with normal rotation to face destination
        if (((!VesselManager.CastRayHorizontal(transform, 0, 90, ColRange / 2, ColRange / 2) && !VesselManager.CastRayHorizontal(transform, 180, 270, ColRange / 2, ColRange / 2)) && Dir() == 1) || ((!VesselManager.CastRayHorizontal(transform, 90, 180, ColRange / 2, ColRange / 2) && !VesselManager.CastRayHorizontal(transform, 270, 360, ColRange / 2, ColRange / 2)) && Dir() == -1))
        {
            //Debug.Log(11);
            stopRotate = false;
            if (justCollided == true)
            {
                addPoint1 = true;
                /*Debug.Log("AddNewRotate");
                AddNewPoint(transform.position);
                col = false;*/
            }
            else
            {
                addPoint1 = false;
            }
        }
        else
        {
            addPoint1 = false;
        }
        //Debug.Log("AddPoint1: " + addPoint1);
        //If there is nothing in front of us, then continue with moving at normal speeds
        if (!VesselManager.CastRayHorizontal(transform, 82.5f, 97.5f, ColRange, ColRange))
        {
            //Debug.Log(22);
            stopTranslateH = false;
            if (justCollided == true)
            {
                addPoint2 = true;
                /*Debug.Log("AddNewTraslate");
                AddNewPoint(transform.position);
                col = false;*/
            }
            else
            {
                addPoint2 = false;
            }
        }
        else
        {
            addPoint2 = false;
        }

        //Stop Adding points if the boat is not set to autopilot
        if (addPoint1 && addPoint2 && Boat.GetComponent<MoveBoat>().auto)
        {
            AddNewPoint(transform.position);
            Debug.Log("AddedPoint");
            justCollided = false;
        }
    }

    private float Angle()
    {
        Vector3 currentDir = transform.forward;
        Vector3 desiredDir = points[points.Count - 2].Key - transform.position;//Boat.transform.position - transform.position;

        float dot = Vector3.Dot(currentDir, desiredDir);
        float magProd = currentDir.magnitude * desiredDir.magnitude;

        float angle = Mathf.Rad2Deg * Mathf.Acos(dot / magProd);

        return angle;
    }

    private int Dir()
    {
        Vector3 currentDir = transform.forward;
        Vector3 desiredDir = points[points.Count - 2].Key - transform.position;//Boat.transform.position - transform.position;

        int dir = ((Vector3.Cross(currentDir, desiredDir).y < transform.position.y) ? -1 : 1);

        return dir;
    }

    private void AddNewPoint(Vector3 p)
    {
        KeyValuePair<Vector3, bool> temp1 = points[points.Count - 2];
        KeyValuePair<Vector3, bool> temp2 = points[points.Count - 1];
        points.RemoveAt(points.Count - 1);
        points.RemoveAt(points.Count - 2);
        points.Add(new KeyValuePair<Vector3, bool>(p, false));
        points.Add(temp1);
        points.Add(temp2);
        //Debug.Log(points.Count);
    }

    public void ResetPoints()
    {
        points.Clear();
        points.Add(new KeyValuePair<Vector3, bool>(Boat.transform.position, false));
        points.Add(new KeyValuePair<Vector3, bool>(Boat.transform.position, false));
        points.Add(new KeyValuePair<Vector3, bool>(transform.position, false));
    }

    public bool StopCheck()
    {
        bool vCheck = vesselSet != 2 || (vesselSet == 2 && auto);

        if ((Angle() > -0.1 && Angle() < 0.1) && Vector3.Distance(transform.position, points[points.Count - 2].Key) <= 50 && vCheck)
            return true;
        if (Vector3.Distance(transform.position, Boat.transform.position) > maxBoatDist && vCheck)
            return true;

        return false;
    }

    private void Stop()
    {
        playerControlled = false;//gameObject.GetComponent<VesselManager>().AUVPlayerControlled;
        gameObject.GetComponent<BoatAlignNormal>()._steerBias = 0;
        gameObject.GetComponent<BoatAlignNormal>()._throttleBias = 0;
        //stopRotate = false;
        //stopTranslate = false;
        //speed = 0;
        //Debug.Log("Stop");
    }

}
