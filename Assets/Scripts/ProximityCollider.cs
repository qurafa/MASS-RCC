using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityCollider : MonoBehaviour
{
    [SerializeField]
    GameObject proximityAlert;

    //private int closeObj = 0;
    private List<Collider> colliding = new List<Collider>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (colliding.Count > 0) {
            proximityAlert.SetActive(true);
        }
        else {
            proximityAlert.SetActive(false);
        }
        colliding.Clear();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!colliding.Contains(collision.collider)) {
            colliding.Add(collision.collider);
        }
    }
}
