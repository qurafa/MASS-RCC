using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AudioController : MonoBehaviour
{

    [SerializeField]
    AudioSource BoatSound;
    [SerializeField]
    AudioSource Ocean;
    [SerializeField]
    AudioSource Underwater;
    [SerializeField]
    GameObject cam;
    [SerializeField]
    GameObject Boat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GetComponent<BoatAlignNormal>()._playerControlled == true)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed || Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed)
                BoatSound.mute = false;
            else
                BoatSound.mute = true;
        }
        else
        {
            if((GetComponent<BoatAlignNormal>()._throttleBias != 0) || (GetComponent<BoatAlignNormal>()._steerBias != 0))
                BoatSound.mute = false;
            else
                BoatSound.mute = true;
        }

        if ((gameObject == Boat) && (cam.transform.position.y >= 0))
        {
            Ocean.volume = 1;
            Underwater.volume = 0;
        }
        else if ((gameObject == Boat) && (cam.transform.position.y < 0))
        {
            Ocean.volume = 0;
            Underwater.volume = 1;
        }

    }
}
