using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAUV : MonoBehaviour
{
    [SerializeField]
    GameObject Boat;
    [SerializeField]
    GameObject Radar;

    public bool playerControlled;

    private Rigidbody rb;
    private RaycastHit hit;
    private float vesselSet;
    private bool stopRotate;
    private bool stopTranslateH;
    private float ColRange = 100;
    int layerMask;
    float speed = 0;
    float dir = 0;
    float hDir = 0;//horizontal Direction
    float vDir = 0;//vertical Direction

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        layerMask = 1 << 8;
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        vesselSet = Boat.GetComponent<VesselManager>().vesselSet;
        if (vesselSet == 2)
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
            TranslateH();
            Rotate();
            Stabilize();
        }

        if (StopCheck() && vesselSet != 2)
            Stop();

        gameObject.GetComponent<BoatAlignNormal>()._playerControlled = playerControlled;
    }

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
    }

    private void TranslateV()
    {}

    private void TranslateH()
    {
        float distance = Vector3.Distance(transform.position, Boat.transform.position);

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
            Debug.Log("A: " + a + " Object: " + hit.collider.gameObject.name);
            //float dir = (((ColRange - hit.distance) * (90 - a)) / (ColRange * 90)) / ((90 - a) / 90);
            //float dir = (ColRange - hit.distance) / ColRange;

            //This is so it turns less the less the object is in from of it
            float newMax = ((90 - a) / 90) * 1.5f;

            if (a < 7.5)
            {
                //prevent the boat from moving normal speeds if there is possibility of collision
                stopTranslateH = stopTranslateH || true;

                //Manage speed based on how far an object is to boat
                speed = (hit.distance / ColRange) * 1.5f;
            }
            else
            {
                stopTranslateH = stopTranslateH || false;
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

        //If there is nothing to worry about 90 deg to our left or right then continue with normal rotation to face destination
        if ((!VesselManager.CastRayHorizontal(transform, 0, 90, ColRange / 2, ColRange / 2) && Dir() == 1) || (!VesselManager.CastRayHorizontal(transform, 90, 180, ColRange / 2, ColRange / 2) && Dir() == -1))
        {
            stopRotate = false;
        }

        //If there is nothing in front of us, then continue with moving at normal speeds
        if (!VesselManager.CastRayHorizontal(transform, 82.5f, 97.5f, ColRange, ColRange))
        {
            stopTranslateH = false;
        }

    }
    

    private float Angle()
    {
        Vector3 currentDir = transform.forward;
        Vector3 desiredDir = Boat.transform.position - transform.position;

        float dot = Vector3.Dot(currentDir, desiredDir);
        float magProd = currentDir.magnitude * desiredDir.magnitude;

        float angle = Mathf.Acos(dot / magProd);

        return angle;
    }

    private int Dir()
    {
        Vector3 currentDir = transform.forward;
        Vector3 desiredDir = Boat.transform.position - transform.position;

        int dir = ((Vector3.Cross(currentDir, desiredDir).y < transform.position.y) ? -1 : 1);

        return dir;
    }

    private bool StopCheck()
    {
        if ((Angle() > -0.1 && Angle() < 0.1) && Vector3.Distance(transform.position, Boat.transform.position) <= 100)
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
