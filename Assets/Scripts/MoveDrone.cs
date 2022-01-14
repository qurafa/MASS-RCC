using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDrone : MonoBehaviour
{
    // Drone camera should stand about 1 m off the ground
    // Initial position should be [750, 1, -9000]
    enum intention { moveUp, moveDown, moveRight, moveLeft, rotateLeft, rotateRight };
    Rigidbody m_Rigidbody;
    float m_Speed;
    Vector3 initialPosition = new Vector3(750.0f, 1.0f, - 8995.0f);
    // Start is called before the first frame update
    void Start()
    {
        //Fetch the Rigidbody component you attach from your GameObject
        transform.position = initialPosition;
        m_Rigidbody = GetComponent<Rigidbody>();
        
        //Set the speed of the GameObject
        m_Speed = 20.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.UpArrow))
        {
            // move up
            transform.Translate(new Vector3(0, 1, 0) * Time.deltaTime * m_Speed, Space.World);
        }
        else if(Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.DownArrow))
        {
            // move down
            transform.Translate(new Vector3(0, -1, 0) * Time.deltaTime * m_Speed, Space.World);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            //Move forward (z)
            transform.Translate(new Vector3(0, 0, 1) * Time.deltaTime * m_Speed, Space.World);
            //m_Rigidbody.velocity = transform.forward * m_Speed;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            // move back (-z)
            transform.Translate(new Vector3(0, 0, -1) * Time.deltaTime * m_Speed, Space.World);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            // move Right (+x)
            transform.Translate(new Vector3(1, 0, 0) * Time.deltaTime * m_Speed, Space.World);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            // move back (-x)
            transform.Translate(new Vector3(-1, 0, 0) * Time.deltaTime * m_Speed, Space.World);
        }
        else if ((Input.GetKeyDown(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKeyDown(KeyCode.LeftArrow) && Input.GetKey(KeyCode.UpArrow)))
        {
            // move forward and left
            transform.Translate(new Vector3(-1, 0, 1) * Time.deltaTime * m_Speed, Space.World);
            print("left and forward");
        }
        //else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow))
        //{
        //    // move back (-x)

        //}

        //if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    //Move the Rigidbody backwards constantly at the speed you define (the blue arrow axis in Scene view)
        //    m_Rigidbody.velocity = -transform.forward * m_Speed;
        //}

        //if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    //Rotate the sprite about the Y axis in the positive direction
        //    transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * m_Speed, Space.World);
        //}

        //if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    //Rotate the sprite about the Y axis in the negative direction
        //    transform.Rotate(new Vector3(0, -1, 0) * Time.deltaTime * m_Speed, Space.World);
        //}
    }
}

