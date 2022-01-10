using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECDISSymbols : MonoBehaviour
{
    [SerializeField]
    Transform Boat;

    // Start is called before the first frame update
    void Start()
    {
        if (name.Equals("North Cardinal") || name.Equals("East Cardinal") || name.Equals("West Cardinal") || name.Equals("South Cardinal"))
        {
            for (int i = 0; i < transform.childCount-1; i++)
                transform.GetChild(i).GetComponent<Renderer>().material.color = Color.yellow;
            Color temp = Color.yellow;
            temp.a = 0.5f;
            transform.GetChild(2).GetComponent<Renderer>().material.color = temp;
        }
        if (name.Equals("Safe Water") || name.Equals("PortLateral (IALA A)") || name.Equals("Stbd. Lateral (IALA B)"))
        {
            transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
            transform.GetChild(1).GetComponent<Renderer>().material.color = Color.black;
            Color temp = Color.red;
            temp.a = 0.5f;
            transform.GetChild(2).GetComponent<Renderer>().material.color = temp;
        }
        if (name.Equals("Stbd. Lateral (IALA A)") || name.Equals("PortLateral (IALA B)"))
        {
            transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
            transform.GetChild(1).GetComponent<Renderer>().material.color = Color.black;
            Color temp = Color.green;
            temp.a = 0.5f;
            transform.GetChild(2).GetComponent<Renderer>().material.color = temp;
        }
        if (name.Equals("Isolated Danger"))
        {
            for (int i = 0; i < gameObject.transform.childCount-1; i++)
                transform.GetChild(i).GetComponent<Renderer>().material.color = Color.red;
            Color temp = Color.red;
            temp.a = 0.5f;
            transform.GetChild(2).GetComponent<Renderer>().material.color = temp;
        }
        if (name.Equals("Emergency Wreck Marking"))
        {
            transform.GetChild(0).GetComponent<Renderer>().material.color = Color.yellow;
            transform.GetChild(1).GetComponent<Renderer>().material.color = Color.black;
            Color temp = Color.yellow;
            temp.a = 0.5f;
            transform.GetChild(2).GetComponent<Renderer>().material.color = temp;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = new Vector3(0, 90+ Boat.eulerAngles.y, 0);
    }

}
