using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCamera : MonoBehaviour
{

    /*
     Requirements: Click the left button to rotate the camera (Method 1: Left and Right Method 2: Up and Down Method 3: Eight Directions)
           Idea: Get the button of the mouse by inputting the angle of calling the game object (its own Euler angle)
           Steps: 1 Determine whether to click the left button
                       2 determine which way to rotate
                       3 Rotate by obtaining the Euler angle of the game object itself!
    */

    //Enumeration: eight directions, left and right, up and down
    public enum Rotation { MouseXAndMouseY, MouseX, MouseY }
    //Enumerate variables
    //Rotation axis;
    //Move speed, sensitivity
    public float speed = 10f;
    //Rotation angle around Y axis (X Euler angle), rotation angle around X axis (Y Euler angle)
    public float yRotation = 0;
    public float xRotation = 0;
    //Limit the minimum and maximum angle values
    //Can't 90, prevent gimbal deadlock
    public float min = -80;
    public float max = 80;

    void Update()
    {
        NewMethod(Rotation.MouseXAndMouseY);
    }

    public void NewMethod(Rotation axis)
    {
        //1, determine whether the left button is clicked
        if (Input.GetMouseButton(0))
        {
            //2, determine which way
            if (axis == Rotation.MouseXAndMouseY)
            {
                //3. Rotate by obtaining the Euler angle of the game object (Camera)
                //The angle of the game object rotating around the x axis is equal to (the angle of the Euler angle Y axis) + the distance the mouse moves laterally
                xRotation += Input.GetAxis("Mouse X") * speed;
                yRotation += Input.GetAxis("Mouse Y") * speed;
                //Limit the rotation angle of Y axis
                yRotation = Mathf.Clamp(yRotation, min, max);
                //The angle of rotation along the X axis is the Euler angle of Y, and the angle of rotation along the Y axis is the negative number of the Euler angle of X
                transform.localEulerAngles = new Vector3(-yRotation, xRotation, 0);
                print(transform.localEulerAngles);
            }
            //Horizontal movement
            if (axis == Rotation.MouseX)
            {
                xRotation += Input.GetAxis("Mouse X") * speed;
                transform.localEulerAngles = new Vector3(0, xRotation, 0);
                //transform.Rotate(0,Input.GetAxis("Mouse X")*speed,0);
            }
            //Move up and down vertically
            else if (axis == Rotation.MouseY)
            {
                yRotation += Input.GetAxis("Mouse Y") * speed;
                yRotation = Mathf.Clamp(yRotation, min, max);
                transform.localEulerAngles = new Vector3(-yRotation, 0, 0);
            }
        }
    }
}
