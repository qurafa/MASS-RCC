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
    [SerializeField]
    GameObject AUV;

    public bool playerControlled;

    public bool auto;
    private int layerMask;
    private int oldPointCount;
    private RaycastHit hit;
    private RaycastHit castHit;
    private bool stopRotate;
    private bool stopTranslate;
    private bool sM;
    private float vesselSet;
    private float ColRange = 525;
    //private int pastPointCount;
    float speed = 0;
    float dir = 0;
    private readonly float EarthRadius = 6371000;//meters
    private List<KeyValuePair<Vector3, bool>> points;
    private List<GameObject> lines;
    private Vector3 nextPos;

    //private Vector3 tempRotation;
    //float angle = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerControlled = gameObject.GetComponent<VesselManager>().BoatPlayerControlled;
        auto = false;
        sM = false;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        oldPointCount = 0;
        lines = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //Auto
        if(vesselSet == 1)
            Auto.isOn = auto;

        points = AUV.GetComponent<MoveAUV>().points;
        vesselSet = GetComponent<VesselManager>().vesselSet;

        if (Keyboard.current.tKey.wasPressedThisFrame && DestinationEntered() && vesselSet == 1)
            auto = !auto;

        //Dra
        if (!DestinationEntered())
            auto = false;
        else
            DrawLine(points, Color.black); //Drawing a line from the boats position to the next point on our way to the entered destination
        //DrawLine(scoutPoints, transform.position, GetDestCoord(), Color.black);

        if (auto && DestinationEntered())
        {
            //Move towards AUV is it stopped and we don't have any more points betwee, else move towards the next point 
            if (points.Count == 3)
                nextPos = points[points.Count - 1].Key;
            else
                nextPos = points[1].Key;
            String s = (nextPos.Equals(AUV.transform.position)) ? "nextAUV" : "nextNotAUV";

            //Debug.Log(s);

            //updating whether we should start moving or not
            if (!sM)
                sM = startMoving();

            playerControlled = false;
            //Rotate(); //Rotating to face our direction first

            //Start moving, while avoiding collision if we can start moving
            if (sM)
            {
                Rotate();
                //Col();
                AutoMove();
            }
            /*playerControlled = false;
            Col();
            Rotate();
            Translate();
            if (startMoving())
            {
                playerControlled = false;
                Col();
                Rotate();
                Translate();
                Debug.Log("Nextpoint scouted");
            }*/
        }
        else
        {
            playerControlled = GetComponent<VesselManager>().BoatPlayerControlled;
            GetComponent<BoatAlignNormal>()._steerBias = 0;
            GetComponent<BoatAlignNormal>()._throttleBias = 0;
        }

        if (StopCheck() && sM)
        {
            //Debug.Log("STOP, Angle: " + Angle() + ", " + "Distance: " + Vector3.Distance(transform.position, GetDestCoord()));
            Stop();
        }

        //Debug.Log("Points Count: " + points.Count);

        //UpdatePoints();
        //if(GetComponent<BoatAlignNormal>()._steerBias != 0)
            //Debug.Log(GetComponent<BoatAlignNormal>()._steerBias);
        SetDestBeacon();
        GetComponent<BoatAlignNormal>()._playerControlled = playerControlled;
        //Debug.Log(Angle(transform, nextPos) + "  " + nextPos.ToString());
    }

    //Moving the ship
    private void AutoMove()
    {
        float distance = Vector3.Distance(transform.position, GetDestCoord());

        if (distance > 50 && !stopTranslate)
        {
            //Adding the ratio so it slows down, the closer the boat gets to the destination, or next point
            float ratio = (distance >= 3000) ? 1 : distance / 3000; //ColRange didn't work, because distace in raycast scale and normal scale is different for whatever reason
            Debug.Log(distance);
            gameObject.GetComponent<BoatAlignNormal>()._throttleBias = ratio * 1;
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
        if ((Angle(transform, nextPos) > 0.1) && !stopRotate)
        {
            gameObject.GetComponent<BoatAlignNormal>()._steerBias = Dir(transform, nextPos);
            //Debug.Log("Angle: " + Angle(transform, nextPos));
        }
        else
        {
            //Stop();
            gameObject.GetComponent<BoatAlignNormal>()._steerBias = dir;
            //Debug.Log("StopRotate");
        }
    }

    //Getting the angle between the entered destination and current position
    private float Angle(Transform c, Vector3 n)
    {
        Vector3 currentDir = c.forward;
        Vector3 desiredDir = n - c.position;//GetDestCoord() - transform.position;

        //Vector3 currentDir = transform.forward;
        //Vector3 desiredDir = nextPos - transform.position;//GetDestCoord() - transform.position;

        float dot = Vector3.Dot(currentDir, desiredDir);
        float magProd = currentDir.magnitude * desiredDir.magnitude;

        float angle = (Mathf.Acos(dot / magProd)) * Mathf.Rad2Deg;

        return angle;
    }

    //Setting the direction of the entered destination, only called when the destination is entered
    private int Dir(Transform c, Vector3 n)
    {
        Vector3 currentDir = c.forward;
        Vector3 desiredDir = n - c.position;

        //Vector3 currentDir = transform.forward;
        //Vector3 desiredDir = GetDestCoord() - transform.position;

        int dir = ((Vector3.Cross(currentDir, desiredDir).y < transform.position.y) ? -1 : 1);

        return dir;
    }

    //Checking if we need to stop
    private bool StopCheck()
    {
        if (!auto)
            return true;
        if ((Angle(transform, GetDestCoord()) < 0.1) && Vector3.Distance(transform.position, GetDestCoord()) <= 100)
            return true;
        if (ProximityAlert.activeInHierarchy)
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
        if(lines.Count > 0)
        {
            foreach (var item in lines){Destroy(item);}
            Debug.Log("ClearLines");
            lines.Clear();
        }
        sM = false;
        AUV.GetComponent<MoveAUV>().ResetPoints();
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
            float dirOffSet = (180 - (Vector3.Angle(currentDir, nextPos - transform.position)) / 180);

            if (a < 7.5)
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
            //Debug.Log(Vector3.Angle(currentDir, nextPos - transform.position));
            //If there is something we may need to worry about 90 deg to our left or right
            if (a < 90)
            {
                //stop normal rotation to face destination because of the possibility of collision
                stopRotate = stopRotate || true;

                //set how much we need to turn using the newMax based on how close the object is
                dir = (((ColRange - hit.distance) * newMax) / ((ColRange * 2) + hit.distance));

                //Set whether we need to turn left or right
                dir *= ((Vector3.Cross(currentDir, contactDir).y < transform.position.y) ? 1 : -1);
            }
            else
            {
                stopRotate = stopRotate || false;
            }
        }

        //If there is nothing to worry about 90 deg to our left when we want to turn left or right when we want to turn right then continue with normal rotation to face destination
        if ((!VesselManager.CastRayHorizontal(transform, 0, 90, ColRange/2, ColRange/2) && Dir(transform, nextPos) == 1) || (!VesselManager.CastRayHorizontal(transform, 90, 180, ColRange/2, ColRange/2) && Dir(transform, nextPos) == -1))
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

    private void DrawLine(List<KeyValuePair<Vector3, bool>> points, Color color, float duration = 0.02f)
    {
        Debug.Log("pointsCount: " + points.Count);
        int[] index = new int[2] { 0, points.Count - 3};
        for (int i = 0; i < 2; i++)
        {
            GameObject line = new GameObject();
            line.name = "ECDISLine";
            line.transform.position = new Vector3(points[index[i]].Key.x, 1, points[index[i]].Key.z);
            line.AddComponent<LineRenderer>();
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.gameObject.layer = 10;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 2;
            lr.endWidth = 2;
            lr.SetPosition(0, new Vector3(points[index[i]].Key.x, 1, points[index[i]].Key.z));
            lr.SetPosition(1, new Vector3(points[index[i] + 1].Key.x, 1, points[index[i] + 1].Key.z));
            GameObject.Destroy(line, duration);
            if (points.Count-3 == 0)
                break;
            //Debug.Log(new Vector3(points[i].Key.x, 1, points[i].Key.z).ToString() + " " + new Vector3(points[i + 1].Key.x, 1, points[i + 1].Key.z).ToString());
        }
        if(points.Count > 4 && points.Count > oldPointCount)
        {
            GameObject line = new GameObject();
            line.name = "ECDISLine";
            line.transform.position = new Vector3(points[points.Count-3].Key.x, 1, points[points.Count - 3].Key.z);
            line.AddComponent<LineRenderer>();
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.gameObject.layer = 10;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 2;
            lr.endWidth = 2;
            lr.SetPosition(0, new Vector3(points[points.Count - 3].Key.x, 1, points[points.Count - 3].Key.z));
            lr.SetPosition(1, new Vector3(points[points.Count - 2].Key.x, 1, points[points.Count - 2].Key.z));
            lines.Add(line);
            Debug.Log("PointsCount" + points.Count + " AddLine: " + lines.Count);
            //Debug.Log("OldPointCount: " + oldPointCount + " pointCount: " + points.Count);
            oldPointCount = points.Count;
        }else if(points.Count < oldPointCount)
        {
            oldPointCount = points.Count;
        }
        if(points.Count >= 4)
        {
            if(points.Count == 4 && RemoveNext(nextPos))
            {
                points.RemoveAt(1);
                Debug.Log("NextPosClose" + points.Count);
            }
            if(points.Count > 4 && RemoveNext(nextPos))
            {
                GameObject lineTemp1 = lines[0];
                lines.RemoveAt(0);
                points.RemoveAt(1);
                Destroy(lineTemp1);
                Debug.Log("NextPosClose" + points.Count + " RemoveLine: " + lines.Count);
            }
        }
            /*
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
            */
    }

    private bool RemoveNext(Vector3 n)
    {
        float boatD = Vector3.Distance(transform.position, GetDestCoord());
        float nextPosD = Vector3.Distance(nextPos, GetDestCoord());

        if(Vector3.Distance(transform.position, nextPos) <= 50)
            return true;
        /*if((Angle(transform, nextPos) > 90) && (boatD > nextPosD))
        {
            return true;
        }*/
        return false;
    }

    private bool startMoving()
    {
        //Facing nextPoint

        if (points.Count >= 4)
        {
            Debug.Log("1......");
            return true;
        }

        if (!VesselManager.CastRayHorizontal(transform, 82.5f, 97.5f, ColRange, ColRange))
        {
            Debug.Log("2......");
            return true;
        }
        if (AUV.GetComponent<MoveAUV>().StopCheck())
        {
            Debug.Log("3......");
            return true;
        }

        return false;
    }

    //Checking if a destination has been entered
    public bool DestinationEntered()
    {
        return GetDestCoord() != transform.position;
    }

    //Getting the entered destination
    public Vector3 GetDestCoord()
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
