using UnityEngine;

public class DroneMainScript : MonoBehaviour
{
    public static DroneMainScript current;

    public bool cursorStartLocked = false;

    [Space]
    [Header("Status")]
    public bool isActive = true;
    public bool isGrounded = true;
    public bool isBlocked = false;

    ////
    [Space]
    [Header("General Flight Settings")]
    public ControlType mode = ControlType.Arcade;
    public enum ControlType { Manual = 0, FlyByWire = 1, Arcade = 2 };

    [Space]
    [Tooltip("BiDirectional Throttle - Allows generating downward lift force (AKA 3D mode)")]
    public bool BiDirThrottle = false;
    [Tooltip("Automatically sets enough Throttle to hover and/or automatically trims it to 50% on the stick.")]
    public bool hoverThrottle = true;
    [Range(0, 1)] public float deadZoneHT = 0.1f;

    [Space]
    [Range(0, 1)] public float startBattery = 0.95f;
    [Tooltip("The Time (in Minutes) to consume 100% Battery when Engine running at Iddle RPM - Set to 0 for no fuel consumption")]
    public float batteryMaxTime = 8f;
    [Tooltip("The Time (in Minutes) taken to consume 100% Battery with Engine running at max RPM - Set to 0 for no fuel consumption")]
    public float batteryMinTime = 1.6666666f;

    [Space]
    public bool allowDamage = true;
    public float groundHeight = 0.26f;

    [Space]
    [Range(0, 2)] public float pitchFactor = 1f;
    [Range(0, 2)] public float rollFactor = 1f, yawFactor = 1f;

    [Space]
    [Range(0, 10)] public float throttleFactor = 2f;
    [Range(0, 10)] public float thrustFactor = 2f;

    [Space]
    [Tooltip("Gives an extra horizontal thrust on Manual Mode to make easier to move - For Realistic Simulations set it to 0")]
    public float thrustAssist = 1;


    //// FlyByWire
    [Space]
    [Header("Fly By Wire Settings")]
    public float fbwPitchAngle = 15;
    public float fbwRollAngle = 30;

    [Space]
    [Tooltip("Amount of yaw rotation added during a Roll to facilitate turns - Arcade Mode only!")]
    [Range(0f, 1f)]
    public float coordinatedTurn = 0.25f;

    [Space]
    [Range(0.01f, 0.1f)] public float fbwResponseFactor = 0.05f;
    [Range(0f, 0.1f)] public float dampResponseFactor = 0.05f;

    float fbwPitchInput, fbwRollInput;
    Vector3 currentAngle, lastAngle;
    ////


    [Space]
    [Header("Turbulence and Wind")]

    public bool useTurbulence = true;
    [Range(0, 1f)] public float turbIntensity = 0.05f;
    [Range(0, 1f)] public float turbVelFactor = 0.1f;

    [Space]
    public bool useWind = false;
    public float windVel = 0.125f;
    public Vector3 windDir = new Vector3(1f, 0f, 0f);
    ////


    ////
    bool clampInput = true;

    [Space]
    [Header("Input Settings")]
    public bool useKeyboard = true;
    public KeyCode toogleCursorKey = KeyCode.Tab, recoverKey = KeyCode.Space, modeKey = KeyCode.Backspace, cameraKey = KeyCode.C;


    public float pitchKeyFactor = 1f;
    public KeyCode pitchDown = KeyCode.W, pitchUp = KeyCode.S;

    public float rollKeyFactor = 1f;
    public KeyCode rollLeft = KeyCode.A, rollRight = KeyCode.D;

    public float yawKeyFactor = 1f;
    public KeyCode yawLeft = KeyCode.Q, yawRight = KeyCode.E;

    public float throttleKeyFactor = 1f;
    public KeyCode throttleUp =  KeyCode.R, throttleDown =  KeyCode.F;

    public float thrustForwardKeyFactor = 1f;
    public KeyCode thrustForward = KeyCode.None, thrustBackward = KeyCode.None; //KeyCode.Keypad8, KeyCode.Keypad2

    public float thrustLateralKeyFactor = 1f;
    public KeyCode thrustLeft = KeyCode.None, thrustRight = KeyCode.None; // KeyCode.Keypad4, KeyCode.Keypad6



    [Space]
    public bool useMouse = true;

    public float pitchMouseFactor = 1f;
    public string pitchMouse = "Mouse Y";

    public float rollMouseFactor = 1f;
    public string rollMouse = "Mouse X";

    public float yawMouseFactor = 1f;
    public string yawMouse = "";    //"Mouse X"

    public float throttleMouseFactor = 1f;
    public string throttleMouse = ""; //"Mouse ScrollWheel"

    public float thrustForwardMouseFactor = 1f;
    public string thrustForwardMouse = ""; //"Mouse Y"

    public float thrustLateralMouseFactor = 1f;
    public string thrustLateralMouse = ""; //Mouse X"


    [Space]
    public bool useMobile = true;
    [Tooltip("Show Mobile controls even in other platforms")]public bool forceMobile = false;
    public float pitchMobileFactor = 1f, rollMobileFactor = 1f, yawMobileFactor = 1f, throttleMobileFactor = 1f, thrustForwardMobileFactor = 1f, thrustLateralMobileFactor = 1f;


    [Space]
    public bool useJoystick = true;

    public KeyCode recoverJoystick = KeyCode.Joystick1Button1, modeJoystick = KeyCode.Joystick1Button2, cameraJoystick = KeyCode.Joystick1Button3;

    public float pitchAxisFactor = 1f;
    public string pitchAxis = "Vertical";

    public float rollAxisFactor = 1f;
    public string rollAxis = "Horizontal";

    public float yawAxisFactor = 1f;
    public string yawAxis = ""; //"Yaw"

    public float throttleAxisFactor = 1f;
    public string throttleAxis = ""; //"Throttle"

    public float thrustForwardAxisFactor = 1f;
    public string thrustForwardAxis = ""; //"Vertical"

    public float thrustLateralAxisFactor = 1f;
    public string thrustLateralAxis = ""; //"Horizontal"
    ////


    //
    [Header("References")]

    [Space]
    public Rigidbody rigidBody;

    [Space]
    public DroneHUD droneHUD;
    public RectTransform pitchArea, mobileInput;
    public GameObject[] cameras;
    int cameraIndex = 0, camNull = 0;

    [Space]
    public AudioClip touchSND;
    public AudioClip hitSND, damageSND;
    public string touchMSG = "Touch", hitMSG = "Hit!", damageMSG = "Propeller Damage!";

    [Space]
    public FlashImg recoverFlashImgBut;
    public Spinning[] propellers;


    [Space]
    [Header("Read Only!")]
    public Vector3 inputTorque;
    public Vector3 inputForce;
    //

    float linearDrag = 0, angDrag = 0;





    ////////////////// Inicialization
    void Awake()
    {
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();
        //if (rigidBody != null) rigidBody.maxAngularVelocity = 7f; //raise default value here if necessary

        if (droneHUD != null && droneHUD.autoFuel)
        {
            droneHUD.fuelTarget = startBattery; droneHUD.fuel = startBattery * droneHUD.fuelAmplitude;
            droneHUD.fuelMaxTime = batteryMaxTime;
            droneHUD.fuelMinTime = batteryMinTime;
        }

        linearDrag = rigidBody.drag;
        angDrag = rigidBody.angularDrag;
    }
    void Start() { if (cursorStartLocked) Cursor.lockState = CursorLockMode.Locked; else Cursor.lockState = CursorLockMode.None; }
    void OnEnable()
    {
        if (current != null && current != this) current.transform.parent.gameObject.SetActive(false);
        current = this;

        detectControls();
    }
    //
    public void detectControls()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld || forceMobile) useMobile = true; else useMobile = false;
        if (mobileInput != null) mobileInput.gameObject.SetActive(useMobile); else useMobile = false;

        if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames().GetValue(0).ToString() != "" ) useJoystick = true;
        else useJoystick = false;

        //print(Input.GetJoystickNames().Length);
        //foreach (string i in Input.GetJoystickNames()) print(i);
    }
    //////////////////



    ////////////////////////////////////// Aircraft Physics
    void FixedUpdate()
    {
        //Return if not activated
        if (!isActive) return;


        //////////////// Current and Last frame Angles
        float currentX = 0, currentY = 0, currentZ = 0;

        currentX = rigidBody.rotation.eulerAngles.x % 360;
        if (currentX > 180) currentX -= 360; else if (currentX < -180) currentX += 360;

        currentY = rigidBody.rotation.eulerAngles.y % 360;
        if (currentX > 180) currentX -= 360; else if (currentX < -180) currentX += 360;

        currentZ = rigidBody.rotation.eulerAngles.z % 360;
        if (currentZ > 180) currentZ -= 360; else if (currentZ < -180) currentZ += 360;

        currentAngle = new Vector3(currentX, currentY, currentZ);
        ////////////////




        //////////////////////////////////////////////////// Manual Control    (TROCAR MODE POR ENUM***)
        if (mode == ControlType.Manual && !isBlocked)
        {
            //Modulates input value to make 50% throttle correspond to hover state on non BiDirectionalThrottle
            if (hoverThrottle)
            {
                if (!BiDirThrottle && throttleFactor != 0)
                {
                    if (inputForce.y < 0.5f + deadZoneHT) inputForce.y = Mathf.Lerp(0, (1f / throttleFactor), inputForce.y / 0.5f);
                    else if (inputForce.y >= 0.5f - deadZoneHT) inputForce.y = Mathf.Lerp((1f / throttleFactor), throttleFactor, (inputForce.y - 0.5f) / 0.5f);
                    else inputForce.y = (1f / throttleFactor);
                }
            }
            //


            ////////  Movement

            // Translation & Lift Input
            rigidBody.AddRelativeForce(
                inputForce.x * thrustFactor * rigidBody.mass * Physics.gravity.magnitude,
                inputForce.y * throttleFactor * rigidBody.mass * Physics.gravity.magnitude,
                inputForce.z * thrustFactor * rigidBody.mass * Physics.gravity.magnitude,
                ForceMode.Force);

            // Extra Horizontal thrust help
            if(thrustAssist != 0)
            {
                rigidBody.AddForce(
                    thrustAssist * (Mathf.Abs(currentAngle.x) > 15 ?   Mathf.Sin(currentAngle.x * Mathf.Deg2Rad) : 0) * thrustFactor * rigidBody.mass * Physics.gravity.magnitude * Vector3.ProjectOnPlane(rigidBody.transform.forward, Vector3.up).normalized +
                    thrustAssist * (Mathf.Abs(currentAngle.z) > 15 ? - Mathf.Sin(currentAngle.z * Mathf.Deg2Rad) : 0) * thrustFactor * rigidBody.mass * Physics.gravity.magnitude * Vector3.ProjectOnPlane(rigidBody.transform.right, Vector3.up).normalized
                    , ForceMode.Force);
            }
            //


            // Torque Input
            rigidBody.AddRelativeTorque(
                inputTorque.x * rigidBody.mass * pitchFactor,
                inputTorque.y * rigidBody.mass * yawFactor,
                inputTorque.z * rigidBody.mass * rollFactor,
                ForceMode.Force);
            //
            ////////
        }
        //////////////////////////////////////////////////// Manual Control



        //////////////////////////////////////////////////// FlyByWire Controls
        if ( (mode == ControlType.FlyByWire || mode == ControlType.Arcade) && !isBlocked)
        {
            //Modulates input value to make 50% throttle correspond to hover state on non BiDirectionalThrottle
            if (hoverThrottle)
            {
                if (!BiDirThrottle && throttleFactor != 0)
                {
                    if (inputForce.y < 0.5f + deadZoneHT) inputForce.y = Mathf.Lerp(0, (1f / throttleFactor), inputForce.y / 0.5f);
                    else if (inputForce.y >= 0.5f - deadZoneHT) inputForce.y = Mathf.Lerp((1f / throttleFactor), throttleFactor, (inputForce.y - 0.5f) / 0.5f);
                    else inputForce.y = (1f / throttleFactor);
                }
            }
            //


            ////////  Movement
            //
            rigidBody.AddForce
                (
                (isGrounded ? 0 : 1) *  inputTorque.x * thrustFactor * rigidBody.mass * Physics.gravity.magnitude * Vector3.ProjectOnPlane(rigidBody.transform.forward, Vector3.up).normalized +
                (isGrounded ? 0 : 1) * -inputTorque.z * thrustFactor * rigidBody.mass * Physics.gravity.magnitude * Vector3.ProjectOnPlane(  rigidBody.transform.right, Vector3.up).normalized +
                (inputForce.y * throttleFactor * rigidBody.mass * Physics.gravity.magnitude * Vector3.up) //(mode == ControlType.Arcade ? 1 : 0) *
                , ForceMode.Force);
            //
            // Translation & Lift Input
            rigidBody.AddRelativeForce(
                inputForce.x * thrustFactor * rigidBody.mass * Physics.gravity.magnitude,
                0,//(mode == ControlType.FlyByWire ? 1 : 0) * inputForce.y * throttleFactor * rigidBody.mass * Physics.gravity.magnitude,
                inputForce.z * thrustFactor * rigidBody.mass * Physics.gravity.magnitude,
                ForceMode.Force);
            //
            rigidBody.AddRelativeTorque
                (
                (inputTorque.x * fbwPitchAngle - currentAngle.x) * rigidBody.mass * pitchFactor * fbwResponseFactor * Vector3.right +
                 ((currentAngle.x - lastAngle.x) / Time.fixedDeltaTime) * (-dampResponseFactor / 10f) * rigidBody.mass * pitchFactor * Vector3.right +

                 inputTorque.y * rigidBody.mass * yawFactor * Vector3.up +

                (inputTorque.z * fbwRollAngle - currentAngle.z) * rigidBody.mass * rollFactor * fbwResponseFactor * Vector3.forward +
                ((currentAngle.z - lastAngle.z) / Time.fixedDeltaTime) * (-dampResponseFactor / 10f) * rigidBody.mass * rollFactor * Vector3.forward +

                (mode == ControlType.Arcade /*&& Mathf.Abs(inputTorque.x) > 0.25f*/ ? 1 : 0) * ( -coordinatedTurn / 10f * inputTorque.z * fbwRollAngle) * rigidBody.mass * yawFactor * Vector3.up
                , ForceMode.Force);
            //
            ////////  Movement

        }
        //////////////////////////////////////////////////// FlyByWire Controls


        ////////////////// Turbulence Force
        if (useTurbulence && !isGrounded && (!isBlocked || (isBlocked && rigidBody.velocity.magnitude > 0.1f)) )
        {
            rigidBody.AddRelativeTorque(
                rigidBody.mass * (isBlocked ? 5f : 1f) *
                new Vector3 
                ( 
                    Random.Range(-turbIntensity, turbIntensity) * (1 + rigidBody.velocity.magnitude * turbVelFactor)
                    , 
                    Random.Range(-turbIntensity / 2f, turbIntensity / 2f) * (1 + rigidBody.velocity.magnitude * turbVelFactor / 2f)
                    , 
                    Random.Range(-turbIntensity, turbIntensity) * (1 + rigidBody.velocity.magnitude * turbVelFactor)
                )
                , ForceMode.Force);
        }
        ////////////////// Turbulence Force



        ////////////////// Wind Force
        if (useWind && !isGrounded)
        {
            rigidBody.AddForce( rigidBody.mass * Physics.gravity.magnitude * windVel * windDir.normalized, ForceMode.Acceleration);
        }
        ////////////////// Wind Force



        lastAngle = currentAngle;
    }
    //////////////////////////////////////






    ////////////////////////////////////// Aircraft Input Control
    void Update()
    {
        //Return if control is not activated
        if (!isActive) return;


        //Cursor lock-unlock with Tab key
        if (Input.GetKeyDown(toogleCursorKey))
        {
            if (Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

            SndPlayer.playClick();
        }
        //

        //Recover Aircraft Attitude
        if (useKeyboard && Input.GetKeyDown(recoverKey)) recoverAttitude();
        if (useJoystick && Input.GetKeyDown(recoverJoystick)) recoverAttitude();
        //

        //Switch from Manual/FlyByWire Mode
        if (useKeyboard && Input.GetKeyDown(modeKey)) toogleArcade();
        if (useJoystick && Input.GetKeyDown(modeJoystick)) toogleArcade();
        //

        //Switch from FPV Camera and External
        if (useKeyboard && Input.GetKeyDown(cameraKey)) changeCamera();
        if (useJoystick && Input.GetKeyDown(cameraJoystick)) changeCamera();
        //



        // Verify if Drone is Grounded using a RayCast below
        RaycastHit hit;
        Physics.Raycast(transform.position, -transform.up, out hit, groundHeight);
        if (hit.collider != null && hit.collider.gameObject != this.gameObject && hit.collider.transform.parent != this.transform) isGrounded = true; else isGrounded = false;

        //if (hit.collider != null) Debug.DrawLine(transform.position, hit.point, Color.yellow, 2.5f);
        //else Debug.DrawLine(transform.position, transform.position + (groundHeight /*0.26f*/) * -transform.up, Color.yellow, 2.5f);
        //


        // Fuel & RPM + Propellers
        if (droneHUD != null)
        {
            if (droneHUD.autoFuel)
            {
                droneHUD.fuelMaxTime = batteryMaxTime;
                droneHUD.fuelMinTime = batteryMinTime;
            }


            if (droneHUD.fuelTarget <= 0.001f || droneHUD.fuel == 0)
            {
                if (!isBlocked)
                {
                    DisplayMsg.showAll("Out of Batteries!");
                    if (recoverFlashImgBut != null) recoverFlashImgBut.flash();
                }
                isBlocked = true;
            }

            if (droneHUD.autoRPM)
            {
                if (!isBlocked) droneHUD.engineTarget = (droneHUD.idleEngine / droneHUD.engineAmplitude + ((Mathf.Abs(inputForce.magnitude) + Mathf.Abs(inputTorque.magnitude) + (droneHUD.absSpeed /(13f * thrustFactor * droneHUD.speedAmplitude) /*droneHUD.maxSpeed*/)) / 3f) * (1 - droneHUD.idleEngine / droneHUD.engineAmplitude));
                else droneHUD.engineTarget = 0;
            }


            for (int i = 0; i <= propellers.Length - 1; i++) { if(propellers[i] != null) propellers[i].factor = droneHUD.engineTarget * Mathf.Sign(inputForce.y); }
        }
        //



        // Skip Input if Propeller is Blocked
        if (isBlocked) { inputTorque = Vector3.zero; inputForce = Vector3.zero; return; }
        //


        //////////////////////// Read all INPUT
        inputTorque = new Vector3
            (
            (useKeyboard ? pitchKeyFactor * ((Input.GetKey(pitchDown) ? 1 : 0) - (Input.GetKey(pitchUp) ? 1 : 0)) : 0 ) +
            (useJoystick && pitchAxis != "" ? pitchAxisFactor * Input.GetAxis(pitchAxis)  : 0) +
            ((useMouse && Cursor.lockState == CursorLockMode.Locked && pitchMouse != "") ? pitchMouseFactor * Input.GetAxis(pitchMouse) : 0) +
            (useMobile ? pitchMobileFactor * DroneInputMobile.pitchInput : 0)
            ,
            (useKeyboard ? yawKeyFactor * ((Input.GetKey(yawRight) ? 1 : 0) - (Input.GetKey(yawLeft) ? 1 : 0)) : 0) +
            (useJoystick && yawAxis != "" ?  yawAxisFactor * Input.GetAxis(yawAxis) : 0) +
            ((useMouse && Cursor.lockState == CursorLockMode.Locked && yawMouse != "") ? yawMouseFactor * Input.GetAxis(yawMouse) : 0) +
            (useMobile ? yawMobileFactor * DroneInputMobile.yawInput : 0)
            ,
            (useKeyboard ? rollKeyFactor * ((Input.GetKey(rollLeft) ? 1 : 0) - (Input.GetKey(rollRight) ? 1 : 0)) : 0) +
            (useJoystick && rollAxis != "" ? rollAxisFactor  * -Input.GetAxis(rollAxis) : 0) +
            ((useMouse && Cursor.lockState == CursorLockMode.Locked && rollMouse != "") ? rollMouseFactor * -Input.GetAxis(rollMouse) : 0) +
            (useMobile ? rollMobileFactor * -DroneInputMobile.rollInput : 0)
            );
        //
        if (BiDirThrottle)
        {
            inputForce = new Vector3
                (
                (useKeyboard ? thrustLateralKeyFactor * ((Input.GetKey(thrustRight) ? 1 : 0) - (Input.GetKey(thrustLeft) ? 1 : 0)) : 0) +
                (useJoystick && thrustLateralAxis != "" ? thrustLateralAxisFactor * Input.GetAxis(thrustLateralAxis) : 0) +
                ((useMouse && Cursor.lockState == CursorLockMode.Locked && thrustLateralMouse != "") ? thrustLateralMouseFactor * Input.GetAxis(thrustLateralMouse) : 0) +
                (useMobile ? thrustLateralMobileFactor * DroneInputMobile.thrustLateralInput : 0)
                ,
                (useKeyboard ? throttleKeyFactor * ((Input.GetKey(throttleUp) ? 1 : 0) - (Input.GetKey(throttleDown) ? 1 : 0)) : 0) +
                (useJoystick && throttleAxis != "" ? throttleAxisFactor * -Input.GetAxis(throttleAxis) : 0) +
                ((useMouse && Cursor.lockState == CursorLockMode.Locked && throttleMouse != "") ? throttleMouseFactor * -Input.GetAxis(throttleMouse) : 0) +
                (useMobile ? throttleMobileFactor * DroneInputMobile.throttleInput : 0)
                ,
                (useKeyboard ? thrustForwardKeyFactor * ((Input.GetKey(thrustForward) ? 1 : 0) - (Input.GetKey(thrustBackward) ? 1 : 0)) : 0) +
                (useJoystick && thrustForwardAxis != "" ? thrustForwardAxisFactor * Input.GetAxis(thrustForwardAxis) : 0) +
                ((useMouse && Cursor.lockState == CursorLockMode.Locked && thrustForwardMouse != "") ? thrustForwardMouseFactor * Input.GetAxis(thrustForwardMouse) : 0) +
                (useMobile ? thrustForwardMobileFactor * DroneInputMobile.thrustForwardInput : 0)
            );
        }
        else
        {
            inputForce = new Vector3
                (
                (useKeyboard ? thrustLateralKeyFactor * ((Input.GetKey(thrustRight) ? 1 : 0) - (Input.GetKey(thrustLeft) ? 1 : 0)) : 0) +
                (useJoystick && thrustLateralAxis != "" ? thrustLateralAxisFactor * Input.GetAxis(thrustLateralAxis) : 0) +
                ((useMouse && Cursor.lockState == CursorLockMode.Locked && thrustLateralMouse != "") ? thrustLateralMouseFactor * Input.GetAxis(thrustLateralMouse) : 0) +
                (useMobile ? thrustLateralMobileFactor * DroneInputMobile.thrustLateralInput : 0)
                ,
                (useKeyboard ? throttleKeyFactor * ((Input.GetKey(throttleUp) ? 1 : 0)) : 0) +
                (useJoystick && throttleAxis != "" ? throttleAxisFactor * (-Input.GetAxis(throttleAxis) + 1f) / 2f : 0) +
                ((useMouse && Cursor.lockState == CursorLockMode.Locked && throttleMouse != "") ? throttleMouseFactor * (-Input.GetAxis(throttleMouse) + 1f) / 2f : 0) +
                (useMobile ? throttleMobileFactor * (DroneInputMobile.throttleInput + 1f) / 2f : 0)
                ,
                (useKeyboard ? thrustForwardKeyFactor * ((Input.GetKey(thrustForward) ? 1 : 0) - (Input.GetKey(thrustBackward) ? 1 : 0)) : 0) +
                (useJoystick && thrustForwardAxis != "" ? thrustForwardAxisFactor * Input.GetAxis(thrustForwardAxis) : 0) +
                ((useMouse && Cursor.lockState == CursorLockMode.Locked && thrustForwardMouse != "") ? thrustForwardMouseFactor * Input.GetAxis(thrustForwardMouse) : 0) +
                (useMobile ? thrustForwardMobileFactor * DroneInputMobile.thrustForwardInput : 0)
            );
            //
            if (useKeyboard && Input.GetKey(throttleDown)) inputForce = new Vector3(inputForce.x, 0f, inputForce.z);
        }
        //
        ////////////////////////


        //// Add enough input Force to balance weight and Hover
        if (hoverThrottle)
        {
            if (BiDirThrottle && throttleFactor != 0)
            {
                if (inputForce.y < 0.5f && inputForce.y > -0.5f)
                {
                    if (inputForce.y < deadZoneHT && inputForce.y > -deadZoneHT)
                        inputForce = new Vector3(inputForce.x, (1f / throttleFactor) * Mathf.Sign(Vector3.Project(rigidBody.transform.up, Vector3.up).y), inputForce.z);
                    else inputForce += Vector3.up * (1f / throttleFactor) * Mathf.Sign(Vector3.Project(rigidBody.transform.up, Vector3.up).y);
                }
            }
        }
        ////


        //////// Clamp Input to -1x1 at each direction
        if (clampInput)
        {
            inputTorque = new Vector3(Mathf.Clamp(inputTorque.x, -1f, 1f), Mathf.Clamp(inputTorque.y, -1f, 1f), Mathf.Clamp(inputTorque.z, -1f, 1f));
            inputForce = new Vector3(Mathf.Clamp(inputForce.x, -1f, 1f), Mathf.Clamp(inputForce.y, -1f, 1f), Mathf.Clamp(inputForce.z, -1f, 1f));
        }
        ////////
    }
    ////////////////////////////////////// Aircraft Input Control



    ////////////////////////////////////// Other Methods
    //
    public void setBattery(float value) { setBattery(value, true); }
    public void setBattery(float value, bool useFilter = true)
    {
        if (droneHUD != null)
        {
            value = Mathf.Clamp01(value);
            droneHUD.fuelTarget = value;
            if(!useFilter) droneHUD.fuel = value * droneHUD.fuelAmplitude;
        }
    }
    //
    public void toogleArcade()
    {
        mode += 1;
        if ((int)mode > System.Enum.GetValues(typeof(ControlType)).Length - 1) mode = 0;

        SndPlayer.playClick();
        DisplayMsg.showAll(mode.ToString() + " Mode", 5f);
    }
    //

    // Recover Aircraft Attitude
    public void recoverAttitude()
    {
        rigidBody.velocity = Vector3.zero;
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        //transform.localRotation = Quaternion.identity;

        if (linearDrag != 0) rigidBody.drag = linearDrag;
        if (angDrag != 0) rigidBody.angularDrag = angDrag;

        if (droneHUD != null && droneHUD.fuelTarget <= 0.01f) droneHUD.fuelTarget = 0.05f;
        if (droneHUD != null) droneHUD.ResetHud();


        isBlocked = false;
        if (recoverFlashImgBut != null) recoverFlashImgBut.stopFlash();

        SndPlayer.playClick();
        DisplayMsg.showAll("Drone Recovered", 5f);
    }
    //

    //
    public void changeCamera()
    {
        if (cameras.Length == 0) return;

        cameraIndex++;
        if (cameraIndex > cameras.Length - 1) cameraIndex = 0;
        if (cameras[cameraIndex].gameObject == null)
        {
            if(camNull >= cameras.Length) { camNull = 0; return; } else { camNull++; changeCamera(); return; }
        }

        for (int i = 0; i <= cameras.Length - 1; i++)
        {
            if (cameras[i] != null)
            {
                if(i == cameraIndex) cameras[i].SetActive(true); else cameras[i].SetActive(false);
            }
        }

        if (pitchArea != null)
        {
            if (cameraIndex == 0) pitchArea.gameObject.SetActive(true); else pitchArea.gameObject.SetActive(false);
        }

        SndPlayer.playClick();
    }
    //

    //Verify if Propeller hit something
    void OnTriggerEnter(Collider other)
    {
        if (!allowDamage) return;

        if (!isBlocked && other != null && other.gameObject != this.gameObject && other.transform.parent != this.transform)
        {
            isBlocked = true;
            if (recoverFlashImgBut != null) recoverFlashImgBut.flash();

            DisplayMsg.showAll(damageMSG);//, 5f);
            SndPlayer.play(damageSND);

            linearDrag = rigidBody.drag; rigidBody.drag = 0.25f;
            angDrag = rigidBody.angularDrag; rigidBody.angularDrag = 0.25f;

            rigidBody.AddForceAtPosition( rigidBody.mass * Physics.gravity.magnitude * -rigidBody.velocity/5f, transform.position, ForceMode.Impulse);
            rigidBody.AddRelativeTorque(rigidBody.mass * Physics.gravity.magnitude * -rigidBody.velocity/2f, ForceMode.Impulse);
        }
    }
    //

    //Collision Sounds and Msgs
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 0.1f) return;
        else if ( collision.relativeVelocity.magnitude < 2f)
        {
            if (touchMSG != "" && !isBlocked) DisplayMsg.showAll(touchMSG, 0.75f);
            if (touchSND != null) SndPlayer.play(touchSND);
        }
        else if (collision.relativeVelocity.magnitude >= 2f)
        {
            if (hitMSG != "" && !isBlocked) DisplayMsg.showAll(hitMSG, 0.75f);
            if (hitSND != null) SndPlayer.play(hitSND);
        }
    }
    //

    ////////////////////////////////////// Other Methods


}