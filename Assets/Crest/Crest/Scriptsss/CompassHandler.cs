using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class CompassHandler : MonoBehaviour
{
    public bool horizontal;

    //Normal Compass
    public Vector3 NorthDirection;
    public Transform Player;
    public RectTransform Northlayer;

    //Horozontal Compass
    public float numberOfPixelsNorthToNorth;
    public GameObject target;
    Vector3 startPosition;
    float rationAngleToPixel;

    private void Start()
    {
        if (horizontal)
        {
            startPosition = transform.position;
            rationAngleToPixel = numberOfPixelsNorthToNorth / 360f;
        }
    }

    private void Update()
    {
        if (!horizontal)
        {
            NorthDirection.z = Player.eulerAngles.y;
            Northlayer.localEulerAngles = NorthDirection;
        }
        if (horizontal)
        {
            Vector3 perp = Vector3.Cross(NorthDirection, target.transform.forward);
            float dir = Vector3.Dot(perp, Vector3.up);
            transform.position = startPosition + (new Vector3(Vector3.Angle(target.transform.forward, NorthDirection) * Mathf.Sign(dir) * rationAngleToPixel, 0, 0));
        }
    }

}
