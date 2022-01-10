using Crest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpecs : MonoBehaviour
{

    [SerializeField]
    GameObject Boat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y > 0)
        {
            transform.position.Set(transform.position.x, transform.position.y - 0.01f, transform.position.z);
        }
    }
}
