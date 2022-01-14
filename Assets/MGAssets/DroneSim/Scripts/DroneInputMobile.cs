using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DroneInputMobile : MonoBehaviour, IDragHandler,IPointerDownHandler, IPointerUpHandler//, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Input Settings")]

    public static int total = 0;

    public enum AxisInput { None = 0, Pitch = 1, Roll = 2, Yaw = 3, Throttle = 4, ThrustForward = 5, ThrustLateral = 6 }; //{ None = 0, Movement = 1, Rotation = 2 };
    public enum AxisReturnTo { None = 0, Center = 1, Bottom = 2, Top = 3 };
    public enum AxisStartPos { Center = 0, Bottom = 1, Top = 2 };


    [Space]
    public bool allowX = true;
    public float xFactor = 1;
    public AxisInput inputX_Axis = AxisInput.None;
    public AxisReturnTo returnX = AxisReturnTo.Center;
    [Tooltip("Set Start Position of Stick when Return mode is set no NONE.")]
    public AxisStartPos startX = AxisStartPos.Center;


    [Space]
    public bool allowY = true;
    public float yFactor = 1;
    public AxisInput inputY_Axis = AxisInput.None;
    public AxisReturnTo returnY = AxisReturnTo.Center;
    [Tooltip("Set Start Position of Stick when Return mode is set no NONE.")]
    public AxisStartPos startY = AxisStartPos.Center;



    [Space]
    public bool freeTouchMode = false;
    public bool hatMode = false, allowDrag = false;
    public float maxDelta = 15;
          
    [Space]
    public GameObject centerBackground;

    Vector2 inputTarget;
    public static float pitchInput, rollInput, yawInput, throttleInput, thrustForwardInput, thrustLateralInput;
    //public AxisInput inputAxis = AxisInput.None;
    //public static Vector2 inputMove, inputRotation;

    float deltaX, deltaY;
    Vector3 centerPos = Vector3.zero;
    Vector2 offSet;
    
    bool printInputTarget = false;

    bool returnAxis = true;
    bool firstTime = true;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////// Initialization
    IEnumerator Start()
    {
        if (returnX != AxisReturnTo.None || returnY != AxisReturnTo.None) returnAxis = true; else returnAxis = false;

        yield return new WaitForEndOfFrame();
        updateCenterPos(true);
        updateStartPos();

        firstTime = false;
    }
    void OnDisable()
    {
        inputTarget = Vector2.zero;
        //inputMove = Vector2.zero; inputRotation = Vector2.zero;
        pitchInput = 0; rollInput = 0; yawInput = 0; throttleInput = 0; thrustForwardInput = 0; thrustLateralInput = 0;
        return;
    }
    void OnEnable()
    {
        if (!firstTime)
        {
            if (returnX != AxisReturnTo.None || returnY != AxisReturnTo.None) returnAxis = true; else returnAxis = false;
            updateCenterPos(returnAxis);
            updateStartPos();
        }
    }
    //
    void updateCenterPos(bool goToCenter)
    {
        if (centerBackground != null) centerPos = centerBackground.transform.position; else centerPos = transform.position;
        if (goToCenter)
        {
            if (returnX != AxisReturnTo.None)
            {
                transform.position = new Vector2
                    (
                    centerPos.x + transform.lossyScale.magnitude * ( (returnX == AxisReturnTo.Bottom ? -maxDelta : 0) + (returnX == AxisReturnTo.Top ? maxDelta : 0)), 
                    transform.position.y
                    );
                inputTarget.x = 0 + ((returnX == AxisReturnTo.Bottom ? -xFactor : 0) + (returnX == AxisReturnTo.Top ? xFactor : 0));
            }

            if (returnY != AxisReturnTo.None)
            {
                transform.position = new Vector2
                    (
                    transform.position.x,
                    centerPos.y + transform.lossyScale.magnitude * ((returnY == AxisReturnTo.Bottom ? -maxDelta : 0) + (returnY == AxisReturnTo.Top ? maxDelta : 0))
                    );
                inputTarget.y = 0 + ((returnY == AxisReturnTo.Bottom ? -yFactor : 0) + (returnY == AxisReturnTo.Top ? yFactor : 0));
            }
        }
    }
    //
    void updateStartPos()
    {
        if (returnX == AxisReturnTo.None)
        {
            transform.position = new Vector2
                (
                centerPos.x + transform.lossyScale.magnitude * ((startX == AxisStartPos.Bottom ? -maxDelta : 0) + (startX == AxisStartPos.Top ? maxDelta : 0)),
                transform.position.y
                );
            inputTarget.x = 0 + ((startX == AxisStartPos.Bottom ? -xFactor : 0) + (startX == AxisStartPos.Top ? xFactor : 0));
        }

        if (returnY == AxisReturnTo.None)
        {
            transform.position = new Vector2
                (
                transform.position.x,
                centerPos.y + transform.lossyScale.magnitude * ((startY == AxisStartPos.Bottom ? -maxDelta : 0) + (startY == AxisStartPos.Top ? maxDelta : 0))
                );
            inputTarget.y = 0 + ((startY == AxisStartPos.Bottom ? -yFactor : 0) + (startY == AxisStartPos.Top ? yFactor : 0));
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////// Initialization




    ////////////////////////////////
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        total += 1; //print("total = " + total);

        updateCenterPos(false);
        offSet = new Vector2(eventData.position.x - transform.position.x, eventData.position.y - transform.position.y);

        if (hatMode) calculateInput(eventData);
    }
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (total > 0) total -= 1; //print("total = " + total);

        updateCenterPos(returnAxis);
    }
    ////////////////////////////////

    //
    ////void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) { }
    //

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    float delta;
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        calculateInput(eventData);
        if (printInputTarget) print("Android inputTarget  X = " + inputTarget.x + " Y = " + inputTarget.y);
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    //
    ////void IEndDragHandler.OnEndDrag(PointerEventData eventData) { }
    ////void IPointerClickHandler.OnPointerClick(PointerEventData eventData) { }
    //

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// INPUT
    void calculateInput(PointerEventData eventData)
    {
        delta = maxDelta * transform.lossyScale.magnitude;
        if (!hatMode)
        {
            //Drag Mode Input
            if (allowX) deltaX = Mathf.Clamp((eventData.position.x - offSet.x - centerPos.x), -delta, delta); else deltaX = 0;
            if (allowY) deltaY = Mathf.Clamp((eventData.position.y - offSet.y - centerPos.y), -delta, delta); else deltaY = 0;
        }
        else
        {
            //Hat Mode Input
            if (allowX) deltaX = Mathf.Clamp((eventData.position.x - centerPos.x), -delta, delta); else deltaX = 0;
            if (allowY) deltaY = Mathf.Clamp((eventData.position.y - centerPos.y), -delta, delta); else deltaY = 0;
        }

        if (allowDrag) transform.position = new Vector3(centerPos.x + deltaX, centerPos.y + deltaY, centerPos.z);

        if (maxDelta != 0)
        {
            if (allowX && allowY) inputTarget = new Vector2(xFactor * deltaX / delta, yFactor * deltaY / delta);
            else
            {
                if (allowX) inputTarget = new Vector2(xFactor * deltaX / delta, inputTarget.y);
                if (allowY) inputTarget = new Vector2(inputTarget.x, yFactor * deltaY / delta);
            }
        }
        //
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// INPUT




    ////////////////////////////////////////////////////////////////////////////////////////// Updates Input Reading
    int touchDragger = -1; //Id for free touch mode
    void Update()
    {
        ////////////////////////////////// Reads Input with FreeTouchMode (directly on sreen without an UI axis)
        //
        if (freeTouchMode)
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (Input.touchCount > total && touchDragger == -1) touchDragger = touch.fingerId;
                    }
                    else
                    if (touch.phase == TouchPhase.Moved)
                    {
                        //
                        if (touch.fingerId == touchDragger)
                        {
                            delta = maxDelta;
                            if (allowX) deltaX = Mathf.Clamp(xFactor * (touch.deltaPosition.x), -delta, delta); else deltaX = 0;
                            if (allowY) deltaY = Mathf.Clamp(yFactor * (touch.deltaPosition.y), -delta, delta); else deltaY = 0;
                            if (maxDelta != 0)
                            {
                                if (allowX && allowY) inputTarget = new Vector2(deltaX, deltaY); //Full Axis Mode
                                else
                                {
                                    if (allowX) inputTarget = new Vector2(deltaX, inputTarget.y); //Split Axis Mode
                                    if (allowY) inputTarget = new Vector2(inputTarget.x, deltaY);
                                }
                            }

                        }
                        //

                        if (printInputTarget) print("Android inputTarget  X = " + inputTarget.x + " Y = " + inputTarget.y);
                    }
                    else
                    if (touch.phase == TouchPhase.Stationary)
                    {
                        if (returnAxis && touch.fingerId == touchDragger) inputTarget = Vector2.zero;
                    }
                    else
                    if (touch.phase == TouchPhase.Ended)
                    {
                        if (touch.fingerId == touchDragger)
                        {
                            touchDragger = -1;
                            if (returnAxis) inputTarget = Vector2.zero;
                        }
                    }
                }
                //
            }
            else { touchDragger = -1; total = 0; } //Reset when no touchs left
        }
        //////////////////////////////////


        ////// Send current InputTarget to Axis Input
        if (inputX_Axis != AxisInput.None)
        {
            if (inputX_Axis == AxisInput.Pitch) pitchInput = inputTarget.x;
            if (inputX_Axis == AxisInput.Roll) rollInput = inputTarget.x;
            if (inputX_Axis == AxisInput.Yaw) yawInput = inputTarget.x;
            if (inputX_Axis == AxisInput.Throttle) throttleInput = inputTarget.x;
            if (inputX_Axis == AxisInput.ThrustForward) thrustForwardInput = inputTarget.x;
            if (inputX_Axis == AxisInput.ThrustLateral) thrustLateralInput = inputTarget.x;
        }
        //
        if (inputY_Axis != AxisInput.None)
        {
            if (inputY_Axis == AxisInput.Pitch) pitchInput = inputTarget.y;
            if (inputY_Axis == AxisInput.Roll) rollInput = inputTarget.y;
            if (inputY_Axis == AxisInput.Yaw) yawInput = inputTarget.y;
            if (inputY_Axis == AxisInput.Throttle) throttleInput = inputTarget.y;
            if (inputY_Axis == AxisInput.ThrustForward) thrustForwardInput = inputTarget.y;
            if (inputY_Axis == AxisInput.ThrustLateral) thrustLateralInput = inputTarget.y;
        }
        //
        ////////bak
        ////if (inputAxis != AxisInput.None)
        ////{
        ////    if (inputAxis == AxisInput.Movement) inputMove = inputTarget;
        ////    if (inputAxis == AxisInput.Rotation) inputRotation = inputTarget;
        ////}
        ////////
    }
    ////////////////////////////////////////////////////////////////////////////////////////// Updates Input Reading
}