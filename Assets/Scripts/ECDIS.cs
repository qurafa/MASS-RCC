using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ECDIS : MonoBehaviour
{
    [SerializeField]
    Transform Boat;
    [SerializeField]
    Transform AUV;
    [SerializeField]
    GameObject ECDIScam;
    [SerializeField]
    GameObject ECDISCamName;
    [SerializeField]
    GameObject FLS;
    [SerializeField]
    List<GameObject> symbols = new List<GameObject>();
    [SerializeField]
    GameObject DLight;
    [SerializeField]
    Dropdown d;
    [SerializeField]
    Camera cam;
    [SerializeField]
    GameObject Pointer;

    private float time = 0;
    private int layerMask;
    private Vector3 worldPos;
    private bool dPosSet = false;
    private RaycastHit hit;
    private readonly float FLSangle = 360;
    private List<string> symbolNames = new List<string>();
    private bool camControl = false;
    private int camSet;
    private bool ui = true;
    private bool dState = false;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        for (int i = 0; i < symbols.Count; i++)
        {
            symbolNames.Add(symbols[i].name);
        }
        d.AddOptions(symbolNames);
        d.onValueChanged.AddListener(delegate { OptionClick(); });
    }

    // Update is called once per frame
    void Update()
    {
        d.gameObject.SetActive(dState);
        ui = Boat.GetComponent<VesselManager>().ui;
        Pointer.transform.position = new Vector3(Boat.position.x, 1, Boat.position.z);
        camSet = Boat.gameObject.GetComponent<VesselManager>().camSet;
        time += Time.deltaTime;

        MoveCam();

        if (time > 1)
        {
            for (float i = ((FLSangle / 2) * -1); i <= (FLSangle / 2); i++)
            {
                float newY = (FLS.transform.TransformDirection(Vector3.forward).y * Mathf.Cos(i)) + ((FLS.transform.TransformDirection(Vector3.forward).z) * Mathf.Sin(i));
                float newZ = (-1 * FLS.transform.TransformDirection(Vector3.forward).y * Mathf.Sin(i)) + ((FLS.transform.TransformDirection(Vector3.forward).z) * Mathf.Cos(i));
                //Debug.DrawRay(FLS.transform.position, new Vector3(FLS.transform.TransformDirection(Vector3.forward).x, newY, newZ) * 272.5f, Color.red, 1);
                if (newY < 0 && Physics.Raycast(FLS.transform.position, new Vector3(FLS.transform.TransformDirection(Vector3.forward).x, newY, newZ), out hit, 272.5f, layerMask))
                {
                    GameObject d = new GameObject();
                    d.AddComponent<MeshRenderer>();
                    d.AddComponent<TextMesh>();
                    d.layer = 10;
                    d.transform.localScale = new Vector3(10, 10, 10);
                    d.transform.Rotate(90, 0, -Boat.eulerAngles.y);
                    d.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
                    d.GetComponent<TextMesh>().text = Mathf.Round(Vector3.Distance(hit.point, new Vector3(hit.point.x, 0, hit.point.z))).ToString();
                    Destroy(d, 5);

                    break;
                }
            }
            time = 0;
        }


        if (camSet == 2)
        {
            ECDIScam.transform.GetChild(0).gameObject.SetActive(ui);//Setting the ECDIS canvas active or not depending on the Cam that we're using

            DLight.GetComponent<Light>().shadows = LightShadows.None;

            if (Keyboard.current.ctrlKey.isPressed && Input.GetMouseButtonDown(1))
            {
                d.transform.position = Input.mousePosition;
                if (!dPosSet)
                {
                    worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    dPosSet = !dPosSet;
                }
                dState = true;
            }

            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                camControl = !camControl;
            }

            if (Keyboard.current.ctrlKey.isPressed && Input.GetMouseButtonDown(0) && d.IsActive())
            {
                dState = false;
                d.value = 0;
                dPosSet = false;
            }
            if (Boat.GetComponent<VesselManager>().vesselSet == 1)
                ECDISCamName.GetComponent<Text>().text = "BoatCam";
            else if (Boat.GetComponent<VesselManager>().vesselSet == 2)
                ECDISCamName.GetComponent<Text>().text = "AUVCam";
            else if (Boat.GetComponent<VesselManager>().vesselSet == 3)
                ECDISCamName.GetComponent<Text>().text = "DroneCam";
        }
        else
        {
            ECDIScam.transform.GetChild(0).gameObject.SetActive(false);//Setting the ECDIS canvas active or not depending on the Cam that we're using
            camControl = false;
            DLight.GetComponent<Light>().shadows = LightShadows.Hard;
        }

        FLS.transform.position = Boat.position;
        FLS.transform.Rotate(0, 1, 0);
    }

    private void MoveCam()
    {
        float v = 5;
        if(camSet == 2)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                ECDIScam.transform.position += new Vector3(v * Mathf.Sin(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y), 0, v * Mathf.Cos(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y));
            }
            if (Keyboard.current.sKey.isPressed)
            {
                ECDIScam.transform.position += new Vector3(-v * Mathf.Sin(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y), 0, -v * Mathf.Cos(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y));
            }
            if (Keyboard.current.dKey.isPressed)
            {
                ECDIScam.transform.position += new Vector3(v * Mathf.Cos(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y), 0, -v * Mathf.Sin(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y));
            }
            if (Keyboard.current.aKey.isPressed)
            {
                ECDIScam.transform.position += new Vector3(-v * Mathf.Cos(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y), 0, v * Mathf.Sin(Mathf.Deg2Rad * ECDIScam.transform.eulerAngles.y));
            }

            if (ECDIScam.GetComponent<Camera>().orthographicSize < 10)
                ECDIScam.GetComponent<Camera>().orthographicSize = 10;
            else
                ECDIScam.GetComponent<Camera>().orthographicSize -= Input.mouseScrollDelta.y * 10;
        }

        if (!camControl)
        {
            ECDIScam.transform.position = new Vector3(Boat.position.x, 82, Boat.position.z);
            ECDIScam.transform.eulerAngles = new Vector3(90, Boat.eulerAngles.y, 0);
            Boat.GetComponent<BoatAlignNormal>()._playerControlled = Boat.GetComponent<MoveBoat>().playerControlled;
            AUV.GetComponent<BoatAlignNormal>()._playerControlled = AUV.GetComponent<MoveAUV>().playerControlled;
        }
        else
        {
            Boat.GetComponent<BoatAlignNormal>()._playerControlled = false;
            AUV.GetComponent<BoatAlignNormal>()._playerControlled = false;
        }
    }

    private void OptionClick()
    {
        if (d.value != 0)
        {
            GameObject g = Instantiate(symbols[d.value - 1]);
            g.SetActive(true);
            g.transform.position = new Vector3(worldPos.x, 6, worldPos.z);
            g.transform.localScale = new Vector3(7.5f, 1, 7.5f);
            d.value = 0;
            dState = false;
            dPosSet = false;
        }
    }

}
