using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAUV : MonoBehaviour
{
    [SerializeField]
    GameObject Boat;

    public bool playerControlled;

    private Rigidbody rb;
    private float vesselSet;
    private bool stopRotate;
    private bool stopTranslate;
    float speed = 0;
    float hDir = 0;//horizontal Direction
    float vDir = 0;//vertical Direction

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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

        gameObject.GetComponent<BoatAlignNormal>()._throttleBias = ((distance > 50) ? 1 : 0);
    }

    private void Rotate()
    {
        gameObject.GetComponent<BoatAlignNormal>()._steerBias = ((Angle() < -0.1 || Angle() > 0.1) ? Dir() : 0);
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
