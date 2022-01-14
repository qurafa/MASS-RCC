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
    Transform Drone;
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
    public bool DronePlayerControlled = true;
    public int camSet = 1;
    public int vesselSet = 1;
    public GameObject vessel;
    public bool ui = true;

    private int _camSetTemp = 0;
    private int _vesselSetTemp = 0;
    private bool _uiTemp = false;

    public  static int layerMask;//the layer for all the vessles, including boat and AUV controlled by the player

    private static RaycastHit _castHit;
    private bool _rCam = false; //Resetting cam rotation
    private readonly float _EarthRadius = 6371000;//meters
    private float _latitude;
    private float _longitude;
    private float _timer1 = 0;
    private float _timer2 = 0;
    private Vector3 _North = new Vector3(0, 0, 0);
    private RaycastHit _hit;
    private readonly float _FLSangle = 360;
    private float _camAngle = 0;
    private float _minProx = 12.5f;
    private float _maxProx = 25;
    private float _ColRange = 525;
    private GameObject _BoatCam;
    private GameObject _AUVCam;
    private GameObject _DroneCam;

    // Start is called before the first frame update
    void Start()
    {
        Sprite.GetComponent<Renderer>().material.SetColor("_SpriteColor", Color.green);
        layerMask = 1 << 8;
        layerMask = ~layerMask;

        //Setting the initial vessel to be the Boat
        vessel = Boat.gameObject;

        //Assigning the cameras
        _BoatCam = playerCam.transform.GetChild(0).gameObject;
        _AUVCam = playerCam.transform.GetChild(1).gameObject;
        _DroneCam = playerCam.transform.GetChild(2).gameObject;
    }

    void FixedUpdate()
    {
        CamSetup();
        VesselSetUp();

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
        ProximityAlert.SetActive(CastRayHorizontal(Boat, 0, 360, _minProx, _maxProx));

        _timer1 += Time.fixedDeltaTime;
        _timer2 += Time.fixedDeltaTime;
        if (_timer1 > 2)
        {
            SetHorizon();
            Info.text = string.Concat(GetLat(), " ", GetLong(), " \n", "Depth: ", decimal.Round(System.Convert.ToDecimal(Boat.position.y), 2), "\n", "Speed: ", decimal.Round(System.Convert.ToDecimal(GetComponent<Rigidbody>().velocity.magnitude), 1));
            DistanceFromDest.text = string.Concat("Distance From Destination:", "\n", decimal.Round(System.Convert.ToDecimal(Vector3.Distance(Boat.position, GetDestCoord())), 2)/1000, " km");

            if(Keyboard.current.rKey.isPressed) _rCam = !_rCam;

            if (Keyboard.current.uKey.isPressed) ui = (vesselSet != 3) ? !ui : false;

            if (Keyboard.current.spaceKey.isPressed)
            {
                if (vesselSet == 3) vesselSet = 1;
                else vesselSet++;
            }

            _timer1 = 0;
        }
        
        //Resetting the camera view to be the front of the boat or rotating the camera based on the horizontal mouse movement or the r-key 
        if (_vesselSetTemp != vesselSet || _rCam || camSet != 1)
        {
            _camAngle = 0;
            playerCam.transform.eulerAngles = new Vector3(vessel.transform.eulerAngles.x, vessel.transform.eulerAngles.y, 0);
        }
        else
        {
            _camAngle += Mathf.Round(Input.GetAxis("Mouse X"));
            playerCam.transform.eulerAngles =
                (vesselSet != 3) ?
                new Vector3(vessel.transform.eulerAngles.x, _camAngle + vessel.transform.eulerAngles.y, 0) :
                new Vector3(vessel.transform.eulerAngles.x, _camAngle + vessel.transform.eulerAngles.y, vessel.transform.eulerAngles.z);
        }

        //Rotating the AUV and Boat radars
        BoatRadar.transform.Rotate(0, 1, 0);
        AUVRadar.transform.Rotate(0, 1, 0);
        //To keep the rotation of the radars on a single axis, the y axis in this case.
        AUVRadar.transform.eulerAngles = new Vector3(0, AUVRadar.transform.eulerAngles.y, 0);
        BoatRadar.transform.eulerAngles = new Vector3(0, BoatRadar.transform.eulerAngles.y, 0);

        if (Physics.Raycast(BoatRadar.transform.position, BoatRadar.transform.TransformDirection(Vector3.forward), out _hit, _ColRange, layerMask))
        {
            DrawLine(BoatRadar.transform.position, _hit.point, Color.green, 0.5f);
            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(Sprite, _hit.point, Sprite.transform.rotation), 2);
            int t = 31;
            //Updating when the raday picks up on an object every 5 seconds
            if (_timer2 > 5 && _hit.transform.gameObject.layer != t)
            {
                DistanceSpeedInfo(_hit.collider.gameObject);
                _timer2 = 0;
            }

            Debug.DrawRay(BoatRadar.transform.position, BoatRadar.transform.TransformDirection(Vector3.forward)*_ColRange, Color.gray, 0.1f, false);
        }
        
        if (Keyboard.current.digit1Key.wasPressedThisFrame) camSet = 1;

        if (Keyboard.current.digit2Key.wasPressedThisFrame) camSet = 2;
    }

    private void VesselSetUp()
    {
        if (vesselSet == 1)
        {
            //Only changing these values when the vessel we are on changes
            if (_vesselSetTemp != vesselSet)
            {
                vessel = Boat.gameObject;
                BoatPlayerControlled = true;
                AUVPlayerControlled = false;
                DronePlayerControlled = false;
                Compass.GetComponent<CompassHandler>().Player = Boat.transform;
                _BoatCam.GetComponent<Camera>().targetTexture = null;
                _BoatCam.SetActive(true);
                _AUVCam.SetActive(false);
                _DroneCam.SetActive(false);

                _vesselSetTemp = vesselSet;
            }

            playerCam.transform.position = new Vector3(Boat.position.x, Boat.position.y + 7.36f, Boat.position.z);
            _BoatCam.transform.localPosition = new Vector3(_BoatCam.transform.localPosition.x, _BoatCam.transform.localPosition.y, 11.5f);
        }
        else if(vesselSet == 2)
        {
            //Only changing these values when the vessel we are on changes
            if (_vesselSetTemp != vesselSet)
            {
                vessel = AUV.gameObject;
                AUVPlayerControlled = true;
                BoatPlayerControlled = false;
                DronePlayerControlled = false;
                Compass.GetComponent<CompassHandler>().Player = AUV.transform;
                _BoatCam.GetComponent<Camera>().targetTexture = AUVCamTexture;
                _BoatCam.SetActive(true);
                _AUVCam.SetActive(true);
                _DroneCam.SetActive(false);

                _vesselSetTemp = vesselSet;
            }

            playerCam.transform.position = new Vector3(AUV.position.x, AUV.position.y + 2, AUV.position.z);
            _BoatCam.transform.localPosition = new Vector3(_BoatCam.transform.localPosition.x, _BoatCam.transform.localPosition.y, 3.5f);
        }
        else if(vesselSet == 3)
        {
            //Only changing these values when the vessel we are on changes
            if (_vesselSetTemp != vesselSet)
            {
                vessel = Drone.gameObject;
                DronePlayerControlled = true;
                BoatPlayerControlled = false;
                AUVPlayerControlled = false;
                Compass.GetComponent<CompassHandler>().Player = Drone.transform;
                _BoatCam.GetComponent<Camera>().targetTexture = null;
                _DroneCam.SetActive(true);
                _BoatCam.SetActive(false);
                _AUVCam.SetActive(false);

                _vesselSetTemp = vesselSet;
            }

            playerCam.transform.position = new Vector3(Drone.position.x, Drone.position.y + 0.15f, Drone.position.z);
            _DroneCam.transform.localPosition = new Vector3(_DroneCam.transform.localPosition.x, _DroneCam.transform.localPosition.y, 11.5f);
        }
    }

    private void CamSetup()
    {
        if (ui != _uiTemp || (camSet == 1 && _camSetTemp != camSet))
        {
            radarCam.SetActive(ui);
            PlayerCanvas.SetActive(ui);
            ECDISCam.SetActive(ui);

            _BoatCam.GetComponent<Camera>().depth = 1;
            _AUVCam.GetComponent<Camera>().depth = 1;
            _DroneCam.GetComponent<Camera>().depth = 1;
            ECDISCam.GetComponent<Camera>().depth = 2;
            radarCam.GetComponent<Camera>().depth = 2;

            _BoatCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            _AUVCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            _DroneCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            ECDISCam.GetComponent<Camera>().rect = new Rect(0.01f, 0.4f, 0.18f, 0.25f);
            radarCam.GetComponent<Camera>().rect = new Rect(0.815f, 0.01f, 0.18f, 0.53f);

            playerCam.GetComponent<AudioListener>().enabled = true;
            ECDISCam.GetComponent<AudioListener>().enabled = false;

            _camSetTemp = camSet;
            _uiTemp = ui;
        }
        else if (ui != _uiTemp || (camSet == 2 && _camSetTemp != camSet))
        {
            ECDISCam.SetActive(true);
            _BoatCam.SetActive(ui);
            radarCam.SetActive(ui);
            PlayerCanvas.SetActive(ui);

            ECDISCam.GetComponent<Camera>().depth = 1;
            _AUVCam.GetComponent<Camera>().depth = 2;
            _BoatCam.GetComponent<Camera>().depth = 2;
            _DroneCam.GetComponent<Camera>().depth = 2;
            radarCam.GetComponent<Camera>().depth = 2;

            ECDISCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
            _BoatCam.GetComponent<Camera>().rect = (vesselSet == 1) ? new Rect(0.01f, 0.4f, 0.18f, 0.25f) : new Rect(0, 0, 1, 1);
            _AUVCam.GetComponent<Camera>().rect = new Rect(0.01f, 0.4f, 0.18f, 0.25f);
            _DroneCam.GetComponent<Camera>().rect = new Rect(0.01f, 0.4f, 0.18f, 0.25f);
            radarCam.GetComponent<Camera>().rect = new Rect(0.815f, 0.01f, 0.18f, 0.53f);

            playerCam.GetComponent<AudioListener>().enabled = false;
            ECDISCam.GetComponent<AudioListener>().enabled = true;

            _camSetTemp = camSet;
            _uiTemp = ui;
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
            o.transform.GetChild(1).GetComponent<TextMesh>().text = Math.Round(_hit.distance) + " m";
        }
        else if(obj.transform.parent.TryGetComponent(out Rigidbody pr))
        {
            o.transform.parent = obj.transform.parent;
            o.transform.GetChild(0).GetComponent<TextMesh>().text = Math.Round(obj.transform.parent.GetComponent<Rigidbody>().velocity.magnitude).ToString() + " m/s";
            o.transform.GetChild(1).GetComponent<TextMesh>().text = Math.Round(_hit.distance) + " m";
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
            if (Physics.Raycast(origin.position, newDir, out _castHit, dist, layerMask)) return true;
        }
        return output;
    }

    private string GetLat() {
        string output = "";

        float deg = (180 * Boat.position.z) / (_EarthRadius * Mathf.PI);
        /*while (deg > 90)
            deg -= 90;*/

        if (deg < 0) output += decimal.Round(System.Convert.ToDecimal(Mathf.Abs(deg)), 5) + " S";
        else output += decimal.Round(System.Convert.ToDecimal(deg), 5) + " N";

        return output;
    }

    private string GetLong() {
        string output = "";

        float deg = (180 * Boat.position.x) / (_EarthRadius * Mathf.PI);
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
            x = (float.Parse(DestLong.text) * _EarthRadius * Mathf.PI) / 180;
            z = (float.Parse(DestLat.text) * _EarthRadius * Mathf.PI) / 180;
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
