using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VesselManager : MonoBehaviour
{
    [SerializeField]
    GameObject Sprite;
    [SerializeField]
    Transform Boat;
    [SerializeField]
    Transform AUV;
    [SerializeField]
    GameObject BoatPointer;
    [SerializeField]
    GameObject AUVPointer;
    [SerializeField]
    GameObject Compass;
    [SerializeField]
    GameObject playerCam;
    [SerializeField]
    RenderTexture AUVCamTexture;
    [SerializeField]
    GameObject radarCam;
    [SerializeField]
    GameObject ECDISCam;
    [SerializeField]
    GameObject PlayerCanvas;
    [SerializeField]
    GameObject AUVCanvas;
    [SerializeField]
    Text Info;
    [SerializeField]
    GameObject BoatRadar;
    [SerializeField]
    GameObject AUVRadar;
    [SerializeField]
    GameObject ProximityAlert;
    [SerializeField]
    InputField DestLat;
    [SerializeField]
    InputField DestLong;
    [SerializeField]
    Text DistanceFromDest;
    [SerializeField]
    GameObject DSInfo;

    public bool BoatPlayerControlled = true;
    public bool AUVPlayerControlled = true;
    public int camSet = 1;
    public int vesselSet = 1;
    public GameObject vessel;
    public bool uistuff = true;

    private static RaycastHit castHit;
    public  static int layerMask;//the layer for all the vessles, including boat and AUV controlled by the player

    private bool rCam = false; //Resetting cam rotation
    private readonly float EarthRadius = 6371000;//meters
    private float latitude;
    private float longitude;
    private float time = 0;
    private float t2 = 0;
    private Vector3 North = new Vector3(0, 0, 0);
    private RaycastHit hit;
    private readonly float FLSangle = 360;
    private float CamAngle = 0;
    private float minProx = 12.5f;
    private float maxProx = 25;
    private float ColRange = 525;

    // Start is called before the first frame update
    void Start()
    {
        Sprite.GetComponent<Renderer>().material.SetColor("_SpriteColor", Color.green);
        layerMask = 1 << 8;
        layerMask = ~layerMask;
    }

    void FixedUpdate()
    {
        CamSetup(camSet);
        VesselSetUp(vesselSet);

        //Keeping the radar camera and Radar oriented around the Boat alone
        radarCam.transform.position = new Vector3(Boat.position.x, radarCam.transform.position.y, Boat.position.z);
        radarCam.transform.eulerAngles = new Vector3(90, Boat.eulerAngles.y, 0);
        //BoatRadar.transform.position = new Vector3(Boat.position.x, Boat.position.y + 4, Boat.position.z);

        //Keeping the boat and auv pointers oriented around the boat and auv respectively
        BoatPointer.transform.position = new Vector3(Boat.position.x, 1, Boat.position.z);
        BoatPointer.transform.eulerAngles = new Vector3(0, Boat.eulerAngles.y, 0);
        AUVPointer.transform.position = new Vector3(AUV.position.x, 1, AUV.position.z);
        AUVPointer.transform.eulerAngles = new Vector3(0, AUV.eulerAngles.y, 0);

        //Turning on proximity alert if there is something close to the boat
        ProximityAlert.SetActive(CastRayHorizontal(Boat, 0, 360, minProx, maxProx));

        time += Time.fixedDeltaTime;
        t2 += Time.fixedDeltaTime;
        if (time > 2)
        {
            SetHorizon();
            Info.text = string.Concat(GetLat(), " ", GetLong(), " \n", "Depth: ", decimal.Round(System.Convert.ToDecimal(Boat.position.y), 2), "\n", "Speed: ", decimal.Round(System.Convert.ToDecimal(GetComponent<Rigidbody>().velocity.magnitude), 1));
            DistanceFromDest.text = string.Concat("Distance From Destination:", "\n", decimal.Round(System.Convert.ToDecimal(Vector3.Distance(Boat.position, GetDestCoord())), 2)/1000, " km");

            if(Keyboard.current.rKey.isPressed) rCam = !rCam;

            if (Keyboard.current.uKey.isPressed) uistuff = !uistuff;

            if (Keyboard.current.spaceKey.isPressed)
            {
                if (vesselSet == 1) vesselSet = 2;
                else if (vesselSet == 2) vesselSet = 1;
            }

            time = 0;
        }
        
        //Resetting the camera view to be the front of the boat or rotating the camera based on the horizontal mouse movement or the r-key 
        if (rCam || camSet != 1)
        {
            CamAngle = 0;
            playerCam.transform.eulerAngles = new Vector3(vessel.transform.eulerAngles.x, vessel.transform.eulerAngles.y, 0);
        }
        else
        {
            CamAngle += Mathf.Round(Input.GetAxis("Mouse X"));
            playerCam.transform.eulerAngles = new Vector3(vessel.transform.eulerAngles.x, CamAngle + vessel.transform.eulerAngles.y, 0);
        }

        //Rotating the AUV and Boat radars
        BoatRadar.transform.Rotate(0, 1, 0);
        AUVRadar.transform.Rotate(0, 1, 0);
        //To keep the rotation on a single axis, the y axis in this case.
        AUVRadar.transform.eulerAngles = new Vector3(0, AUVRadar.transform.eulerAngles.y, 0);
        BoatRadar.transform.eulerAngles = new Vector3(0, BoatRadar.transform.eulerAngles.y, 0);

        if (Physics.Raycast(BoatRadar.transform.position, BoatRadar.transform.TransformDirection(Vector3.forward), out hit, ColRange, layerMask))
        {
            DrawLine(BoatRadar.transform.position, hit.point, Color.green, 0.5f);
            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(Sprite, hit.point, Sprite.transform.rotation), 2);
            int t = 31;
            if (hit.transform.gameObject.layer != t && t2 > 5)
            {
                DistanceSpeedInfo(hit.collider.gameObject);
                t2 = 0;
            }

            Debug.DrawRay(BoatRadar.transform.position, BoatRadar.transform.TransformDirection(Vector3.forward)*ColRange, Color.gray, 0.1f, false);
        }
        
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            camSet = 1;

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            camSet = 2;
    }

    private void VesselSetUp(int i)
    {
        GameObject BoatCam = playerCam.transform.GetChild(0).gameObject;
        GameObject AUVCam = playerCam.transform.GetChild(1).gameObject;
        if (i == 1)
        {
            playerCam.transform.position = new Vector3(Boat.position.x, Boat.position.y + 7.36f, Boat.position.z);
            playerCam.transform.GetChild(0).transform.localPosition = new Vector3(playerCam.transform.GetChild(0).transform.localPosition.x, playerCam.transform.GetChild(0).transform.localPosition.y, 11.5f);
            vessel = Boat.gameObject;
            BoatPlayerControlled = true;
            AUVPlayerControlled = false;
            Compass.GetComponent<CompassHandler>().Player = Boat.transform;
            BoatCam.GetComponent<Camera>().targetTexture = null;
            AUVCam.SetActive(false);
        }
        else if(i == 2)
        {
            playerCam.transform.position = new Vector3(AUV.position.x, AUV.position.y + 2, AUV.position.z);
            playerCam.transform.GetChild(0).transform.localPosition = new Vector3(playerCam.transform.GetChild(0).transform.localPosition.x, playerCam.transform.GetChild(0).transform.localPosition.y, 3.5f);
            vessel = AUV.gameObject;
            AUVPlayerControlled = true;
            BoatPlayerControlled = false;
            Compass.GetComponent<CompassHandler>().Player = AUV.transform;
            BoatCam.GetComponent<Camera>().targetTexture = AUVCamTexture;
            AUVCam.SetActive(true);
        }
    }

    private void CamSetup(int i)
    {
        GameObject BoatCam = playerCam.transform.GetChild(0).gameObject;
        GameObject AUVCam = playerCam.transform.GetChild(1).gameObject;
        if (i == 1)
        {
            BoatCam.SetActive(true);
            radarCam.SetActive(uistuff);
            PlayerCanvas.SetActive(uistuff);
            ECDISCam.SetActive(uistuff);

            BoatCam.GetComponent<Camera>().depth = 1;
            AUVCam.GetComponent<Camera>().depth = 1;
            ECDISCam.GetComponent<Camera>().depth = 2;
            radarCam.GetComponent<Camera>().depth = 2;

            BoatCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            AUVCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            ECDISCam.GetComponent<Camera>().rect = new Rect(0.01f, 0.4f, 0.18f, 0.25f);
            radarCam.GetComponent<Camera>().rect = new Rect(0.815f, 0.01f, 0.18f, 0.53f);

            BoatCam.GetComponent<AudioListener>().enabled = true;
            ECDISCam.GetComponent<AudioListener>().enabled = false;
        }
        else if (i == 2)
        {
            ECDISCam.SetActive(true);
            BoatCam.SetActive(uistuff);
            radarCam.SetActive(uistuff);
            PlayerCanvas.SetActive(uistuff);

            ECDISCam.GetComponent<Camera>().depth = 1;
            AUVCam.GetComponent<Camera>().depth = 2;
            BoatCam.GetComponent<Camera>().depth = 2;
            radarCam.GetComponent<Camera>().depth = 2;

            ECDISCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            BoatCam.GetComponent<Camera>().rect = (vesselSet == 1) ? new Rect(0.01f, 0.4f, 0.18f, 0.25f) : new Rect(0, 0, 1, 1);
            AUVCam.GetComponent<Camera>().rect = new Rect(0.01f, 0.4f, 0.18f, 0.25f);
            radarCam.GetComponent<Camera>().rect = new Rect(0.815f, 0.01f, 0.18f, 0.53f);

            BoatCam.GetComponent<AudioListener>().enabled = false;
            ECDISCam.GetComponent<AudioListener>().enabled = true;
        }
    }

    private void SetHorizon()
    {
        float h = playerCam.transform.GetChild(0).position.y;

        if (h > 0)
            playerCam.transform.GetChild(0).GetComponent<Camera>().farClipPlane = (h * 5000) / 1.8288f;
        else
            playerCam.transform.GetChild(0).GetComponent<Camera>().farClipPlane = 10000;
    }

    private void DistanceSpeedInfo(GameObject obj)
    {
        GameObject o = Instantiate(DSInfo);
        o.transform.localScale = new Vector3(20, 20, 20);

        if (obj.TryGetComponent(out Rigidbody r))
        {
            o.transform.parent = obj.transform;
            o.transform.GetChild(0).GetComponent<TextMesh>().text = Math.Round(obj.GetComponent<Rigidbody>().velocity.magnitude).ToString() + " m/s";
            o.transform.GetChild(1).GetComponent<TextMesh>().text = Math.Round(hit.distance) + " m";
        }
        else if(obj.transform.parent.TryGetComponent(out Rigidbody pr))
        {
            o.transform.parent = obj.transform.parent;
            o.transform.GetChild(0).GetComponent<TextMesh>().text = Math.Round(obj.transform.parent.GetComponent<Rigidbody>().velocity.magnitude).ToString() + " m/s";
            o.transform.GetChild(1).GetComponent<TextMesh>().text = Math.Round(hit.distance) + " m";
        }
        o.transform.position = new Vector3(obj.transform.position.x, 5, obj.transform.position.z);
        o.SetActive(true);
        Destroy(o, 5);
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
    {
        GameObject line = new GameObject();
        line.transform.position = start;
        line.AddComponent<LineRenderer>();
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.gameObject.layer = 7;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 15;
        lr.endWidth = 15;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(line, duration);
    }

    public static bool CastRayHorizontal(Transform origin, float minAngle, float maxAngle, float maxX, float maxZ)
    {
        bool output = false;
        for (float i = minAngle; i <= maxAngle; i++)
        {
            float newX = origin.position.x + (maxX * Mathf.Cos(Mathf.Deg2Rad * i));
            float newZ = origin.position.z + (maxZ * Mathf.Sin(Mathf.Deg2Rad * i));
            Vector3 p = new Vector3(newX, origin.position.y, newZ);
            float dist = Vector3.Distance(p, origin.position);
            Vector3 newDir = origin.TransformDirection(p - origin.position);
            Debug.DrawRay(origin.position, newDir, Color.red, 0.05f, false);
            if (Physics.Raycast(origin.position, newDir, out castHit, dist, layerMask)) return true;
        }
        return output;
    }

    private string GetLat() {
        string output = "";

        float deg = (180 * Boat.position.z) / (EarthRadius * Mathf.PI);
        /*while (deg > 90)
            deg -= 90;*/

        if (deg < 0) output += decimal.Round(System.Convert.ToDecimal(Mathf.Abs(deg)), 5) + " S";
        else output += decimal.Round(System.Convert.ToDecimal(deg), 5) + " N";

        return output;
    }

    private string GetLong() {
        string output = "";

        float deg = (180 * Boat.position.x) / (EarthRadius * Mathf.PI);
        /*while (deg > 180)
            deg -= 180;*/

        if (deg < 0) output += decimal.Round(System.Convert.ToDecimal(Mathf.Abs(deg)), 5) + " W";
        else output += decimal.Round(System.Convert.ToDecimal(deg), 5) + " E";

        return output;
    }

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
            y = Boat.position.y;
            output = new Vector3(x, y, z);
        }
        catch (Exception e)
        {
            output = Boat.position;
        }

        return output;
    }

}
