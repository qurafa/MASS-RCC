using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    [SerializeField]
    Transform toRotateAbout = null;
    [SerializeField]
    public float speed = 1;

    float angle = 0;
    void Update()
    {
        angle += Mathf.Round(Input.GetAxis("Mouse X") * speed);
        transform.eulerAngles = new Vector3(0, angle, 0);
        /*var dir = transform.TransformDirection(Vector3.forward);
        float newX = (dir.x * Mathf.Cos(angle)) + ((dir.z) * Mathf.Sin(angle));
        float newZ = (-1 * dir.x * Mathf.Sin(angle)) + ((dir.z) * Mathf.Cos(angle));
        transform.position = new Vector3(newX*Radius, transform.position.y, newZ*Radius);
        Debug.Log("Position: " + transform.position.ToString());
        Debug.Log("Angle: " + angle);*/
    }
}
