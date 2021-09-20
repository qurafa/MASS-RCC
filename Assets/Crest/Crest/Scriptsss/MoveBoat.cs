using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveBoat : MonoBehaviour
{
    [SerializeField]
    InputField DestLat;
    [SerializeField]
    InputField DestLong;
    [SerializeField]
    GameObject Radar;
    [SerializeField]
    GameObject ProximityAlert;
    [SerializeField]
    Toggle Auto;
    [SerializeField]
    GameObject DestBeacon;

    public bool playerControlled;

    private bool auto;
    private int layerMask;
    private RaycastHit hit;
    private RaycastHit castHit;
    private bool stopRotate;
    private bool stopTranslate;
    private float ColRange = 525;
    float speed = 0;
    float dir = 0;
    private readonly float EarthRadius = 6371000;//meters
    //private Vector3 tempRotation;
    //float angle = 0;

    // Start is called before the first frame update
    void Start()
    {
        auto = false;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        Auto.isOn = auto;
        if (Keyboard.current.tKey.wasPressedThisFrame && DestinationEntered())
            auto = !auto;

        if (!DestinationEntered())
            auto = false;
        else
            DrawLine(transform.position, GetDestCoord(), Color.black);

        if (auto && DestinationEntered())
        {
            playerControlled = false;
            Col();
            Rotate();
            Translate();
        }

        if (StopCheck())
        {
            //Debug.Log("STOP, Angle: " + Angle() + ", " + "Distance: " + Vector3.Distance(transform.position, GetDestCoord()));
            Stop();
        }
        SetDestBeacon();
        gameObject.GetComponent<BoatAlignNormal>()._playerControlled = playerControlled;
    }

    //Moving the ship
    private void Translate()
    {
        float distance = Vector3.Distance(transform.position, GetDestCoord());

        if (distance > 50 && !stopTranslate)
        {
            gameObject.GetComponent<BoatAlignNormal>()._throttleBias = 1;
        }
        else
        {
            //Stop();
            gameObject.GetComponent<BoatAlignNormal>()._throttleBias = speed;
        }

        //Debug.Log("Distance: " + distance);

    }

    //Rotating the ship towards the desired destination
    private void Rotate()
    {
        if ((Angle() < -0.1 || Angle() > 0.1) && !stopRotate)
        {
            
            gameObject.GetComponent<BoatAlignNormal>()._steerBias = Dir();
            //Debug.Log("Angle: " + Angle());
        }
        else
        {
            //Stop();
            gameObject.GetComponent<BoatAlignNormal>()._steerBias = dir;
            //Debug.Log("StopRotate");
        }
    }

    //Getting the angle between the entered destination and current position
    private float Angle()
    {
        Vector3 currentDir = transform.forward;
        Vector3 desiredDir = GetDestCoord() - transform.position;

        float dot = Vector3.Dot(currentDir, desiredDir);
        float magProd = currentDir.magnitude * desiredDir.magnitude;

        float angle = Mathf.Acos(dot / magProd);

        return angle;
    }

    //Setting the direction of the entered destination, only called when the destination is entered
    private int Dir()
    {
        Vector3 currentDir = transform.forward;
        Vector3 desiredDir = GetDestCoord() - transform.position;

        int dir = 1;
        if (Vector3.Cross(currentDir, desiredDir).y < transform.position.y)
            dir = -1;

        return dir;
    }

    //Checking if we need to stop
    private bool StopCheck()
    {
        if (!auto)
            return true;
        if ((Angle() > -0.1 && Angle() < 0.1) && Vector3.Distance(transform.position, GetDestCoord()) < 100)
            return true;
        if (ProximityAlert.active)
            return true;

        return false;
    }

    //To stop the boat
    private void Stop()
    {
        playerControlled = gameObject.GetComponent<VesselManager>().BoatPlayerControlled;
        gameObject.GetComponent<BoatAlignNormal>()._steerBias = 0;
        gameObject.GetComponent<BoatAlignNormal>()._throttleBias = 0;
        stopRotate = false;
        stopTranslate = false;
        speed = 0;
        auto = false;
        //Debug.Log("Stop");
    }

    //For avoiding collision with different objects
    private void Col()
    {
        if (Physics.Raycast(Radar.transform.position, Radar.transform.TransformDirection(Vector3.forward), out hit, ColRange, layerMask))
        {
            Vector3 currentDir = transform.forward;
            Vector3 contactDir = hit.point - transform.position;

            float a = Vector3.Angle(currentDir, contactDir);
            //Debug.Log("A: " + a);
            //Debug.DrawRay(Radar.transform.position, Radar.transform.TransformDirection(Vector3.forward) * ColRange, Color.red, 0.1f);
            //float dir = (((ColRange - hit.distance) * (90 - a)) / (ColRange * 90)) / ((90 - a) / 90);
            //float dir = (ColRange - hit.distance) / ColRange;

            //This is so it turns less the less the object is in from of it
            float newMax = ((90 - a) / 90) * 1.5f;

            if(a < 7.5)
            {
                //prevent the boat from moving normal speeds if there is possibility of collision
                stopTranslate = stopTranslate || true;

                //Manage speed based on how far an object is to boat
                speed = (hit.distance / ColRange) * 1.5f;
            }
            else
            {
                stopTranslate = stopTranslate || false;
            }

            //If there is something we may need to worry about 90 deg to our left or right
            if (a < 90)
            {
                //stop normal rotation to face destination because of the possibility of collision
                stopRotate = stopRotate || true;

                //set how much we need to turn using the newMax based on how close th eobject is
                dir = ((ColRange - hit.distance) * newMax) / ((ColRange * 2) + hit.distance);

                //Set whether we need to turn left or right
                dir *= ((Vector3.Cross(currentDir, contactDir).y < transform.position.y) ? 1 : -1);
            }
            else
            {
                stopRotate = stopRotate || false;
            }
        }

        //If there is nothing to worry about 90 deg to our left when we want to turn left or right when we want to turn right then continue with normal rotation to face destination
        if ((!VesselManager.CastRayHorizontal(transform, 0, 90, ColRange/2, ColRange/2) && Dir() == 1) || (!VesselManager.CastRayHorizontal(transform, 90, 180, ColRange/2, ColRange/2) && Dir() == -1))
        {
            stopRotate = false;
        }

        //If there is nothing in front of us, then continue with moving at normal speeds
        if(!VesselManager.CastRayHorizontal(transform, 82.5f, 97.5f, ColRange, ColRange))
        {
            stopTranslate = false;
        }

    }

    //Adding a beacon to the set destination
    private void SetDestBeacon()
    {
        if(DestinationEntered())
        {
            DestBeacon.SetActive(true);
            DestBeacon.transform.position = new Vector3(GetDestCoord().x, 1, GetDestCoord().z);
            DestBeacon.transform.localScale = new Vector3(7.5f, 1, 7.5f);
        }
        else
        {
            DestBeacon.SetActive(false);
        }
    }

    /*private bool CastRayHorizontal(Vector3 origin, float minAngle, float maxAngle, float maxX, float maxZ)
    {
        bool output = false;
        for (float i = minAngle; i <= maxAngle; i++)
        {
            float newX = transform.position.x + (maxX * Mathf.Cos(Mathf.Deg2Rad * i));
            float newZ = transform.position.z + (maxZ * Mathf.Sin(Mathf.Deg2Rad * i));
            Vector3 p = new Vector3(newX, transform.position.y, newZ);
            float dist = Vector3.Distance(p, origin);
            Vector3 newDir = transform.TransformDirection(p - origin);
            if (Physics.Raycast(origin, newDir, out castHit, dist, layerMask))
            {
                //Debug.DrawRay(origin, newDir, Color.blue, 0.05f, false);
                return true;
            }
        }
        return output;
    }*/

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
    {
        GameObject line = new GameObject();
        line.transform.position = new Vector3(start.x, 1, start.z);
        line.AddComponent<LineRenderer>();
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.gameObject.layer = 10;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 2;
        lr.endWidth = 2;
        lr.SetPosition(0, new Vector3(start.x, 1, start.z));
        lr.SetPosition(1, new Vector3(end.x, 1, end.z));
        GameObject.Destroy(line, duration);
    }

    //Checking if a destination has been entered
    private bool DestinationEntered()
   {
        return GetDestCoord() != transform.position;
    }

    //Getting the entered destination
    private Vector3 GetDestCoord()
    {
        float x;
        float y;
        float z;
        Vector3 output;
        try
        {
            x = (float.Parse(DestLong.text) * EarthRadius * Mathf.PI) / 180;
            z = (float.Parse(DestLat.text) * EarthRadius * Mathf.PI) / 180;
            y = transform.position.y;
            output = new Vector3(x, y, z);
        }
        catch (Exception e)
        {
            output = transform.position;
        }

        return output;
    }

}
