using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using c = TerrainAndTasks.ControlScript;

namespace TerrainAndTasks
{
    public class DroneBehaviour : MonoBehaviour
    {
        public Image Q, W, E, R, A, S, D, F, UpA, DnA, RiA, LeA;
        private string[] actResources = { "QKeyAct", "WKeyAct", "EKeyAct", "AKeyAct", "SKeyAct", "DKeyAct", "RKeyAct", "FKeyAct",
                                           "UpArrowAct","DownArrowAct","RightArrowAct","LeftArrowAct"};
        private string[] normResources = { "QKey", "WKey", "EKey", "AKey", "SKey", "DKey", "RKey", "FKey",
                                           "UpArrow","DownArrow","RightArrow","LeftArrow"};
        private Image[] images;
        private string keyResource;
        private int keyCode;
        string uri;
        // Start is called before the first frame update
        void Start()
        {
            c.time = 0;
            //c.eMode = (int)c.expMode.run;
            if (c.experimentSet)
            {
                if (c.eMode == (int)c.expMode.practice || c.eMode == (int)c.expMode.run)
                {
                    //c.eMode = (int)c.expMode.run;
                    //c.pMode = (int)c.practiceMode.run;
                    GetComponent<DroneMainScript>().useTurbulence = true;
                    GetComponent<DroneMainScript>().turbIntensity = c.turbulence;
                    images = new Image[12] { Q, W, E, A, S, D, R, F, UpA, DnA, RiA, LeA };
                    // for all modes
                    // Forward and backward, up and down, left and right
                    //trottleGroup.alpha = 0.0f;
                    //thrustGroup.alpha = 0.0f;
                    GetComponent<DroneMainScript>().thrustForward = KeyCode.UpArrow;
                    GetComponent<DroneMainScript>().thrustBackward = KeyCode.DownArrow;
                    GetComponent<DroneMainScript>().thrustRight = KeyCode.RightArrow;
                    GetComponent<DroneMainScript>().thrustLeft = KeyCode.LeftArrow;
                    GetComponent<DroneMainScript>().throttleUp = KeyCode.R;
                    GetComponent<DroneMainScript>().throttleDown = KeyCode.F;

                    //Pitch forward and back, Yaw right and left, Roll right and left

                    GetComponent<DroneMainScript>().pitchDown = KeyCode.W;
                    GetComponent<DroneMainScript>().pitchUp = KeyCode.S;
                    GetComponent<DroneMainScript>().rollLeft = KeyCode.A;
                    GetComponent<DroneMainScript>().rollRight = KeyCode.D;
                    GetComponent<DroneMainScript>().yawLeft = KeyCode.Q;
                    GetComponent<DroneMainScript>().yawRight = KeyCode.E;

                    //Start the sensors
                    //start experiment
                    if(c.eMode == (int)c.expMode.run && c.rMode == (int)c.runMode.run)
                    {
                        float nSamples = c.apiSampling[c.sampleIndex, 1] * c.ExpDuration * 60 + 1;
                        uri = c.http_header + c.neuLogPort + "/" + c.comm_header + "StartExperiment:[GSR],[1],[Pulse],[1],[" + c.apiSampling[c.sampleIndex, 0].ToString() + "],[" + nSamples.ToString() + "]";
                        c.sensorState = (int)c.neulogState.starting;
                        c.numCalls = 0;
                        GetComponent<NeuLogCom>().SendHttpRequest(uri);
                    }
                    

                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (c.eMode == (int)c.expMode.practice)
            {
                if (Input.anyKeyDown)
                {

                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        keyCode = (int)KeyCode.R;
                        keyResource = "RKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        keyCode = (int)KeyCode.F;
                        keyResource = "FKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        keyCode = (int)KeyCode.UpArrow;
                        keyResource = "UpArrowAct";
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        keyCode = (int)KeyCode.DownArrow;
                        keyResource = "DownArrowAct";
                    }
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        keyCode = (int)KeyCode.LeftArrow;
                        keyResource = "LeftArrowAct";
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        keyCode = (int)KeyCode.RightArrow;
                        keyResource = "RightArrowAct";
                    }
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        keyCode = (int)KeyCode.Q;
                        keyResource = "QKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        keyCode = (int)KeyCode.W;
                        keyResource = "WKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        keyCode = (int)KeyCode.E;
                        keyResource = "EKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        keyCode = (int)KeyCode.A;
                        keyResource = "AKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        keyCode = (int)KeyCode.S;
                        keyResource = "SKeyAct";
                    }
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        keyCode = (int)KeyCode.D;
                        keyResource = "DKeyAct";
                    }
                    changeKeyImage();
                }
            }
            //Q.sprite = Resources.Load<Sprite>("QKeyAct");
            //W.sprite = Resources.Load<Sprite>("WKey");
            //E.sprite = Resources.Load<Sprite>("EKey");
            //R.sprite = Resources.Load<Sprite>("RKey");
            //A.sprite = Resources.Load<Sprite>("AKey");
            //S.sprite = Resources.Load<Sprite>("SKey");
            //D.sprite = Resources.Load<Sprite>("DKey");
            //F.sprite = Resources.Load<Sprite>("FKey");
            //UpA.sprite = Resources.Load<Sprite>("UpArrow");
            //DnA.sprite = Resources.Load<Sprite>("DownArrow");
            //RiA.sprite = Resources.Load<Sprite>("RightArrow");
            //LeA.sprite = Resources.Load<Sprite>("LeftArrow");


            if (c.eMode == (int)c.expMode.run)
            {
                if (c.flyState)
                {
                    c.time += Time.deltaTime;
                    if (c.time >= c.ExpDuration)
                    {
                        if(c.time > (c.ExpDuration + 1.0f) && c.numCalls == 0)
                        {
                            //get experiment data
                            uri = c.http_header + c.neuLogPort + "/" + c.comm_header + "GetExperimentSamples:[GSR],[1],[Pulse],[1]";
                            c.sensorState = (int)c.neulogState.getting;
                            GetComponent<NeuLogCom>().SendHttpRequest(uri);
                            c.numCalls += 1; 
                        }
                        if(c.time>=(c.ExpDuration + 2.0f) && c.numCalls == 1)
                        {
                            //stop experiment                             
                            string uri = c.http_header + c.neuLogPort + "/" + c.comm_header + "StopExperiment";
                            c.sensorState = (int)c.neulogState.stopping;
                            GetComponent<NeuLogCom>().SendHttpRequest(uri);
                            c.numCalls += 1; 
                        }
                        if (c.time >= (c.ExpDuration + 2.0f) && c.numCalls >= 2)
                        {
                            GetComponent<SceneControl>().ConcludeRun();  
                        }
                        
                    }
                    //else
                    //{
                    //    print(GetComponent<DroneMainScript>().turbIntensity);
                    //}
                }
            }

            if (c.eMode == (int)c.expMode.practice)
            {
                if (c.flyState)
                {
                    c.time += Time.deltaTime;
                    if (c.time >= c.ExpDuration)
                    {
                        GetComponent<SceneControl>().ConcludeRun();
                    }
                }
            }
        }

        private void changeKeyImage()
        {
            for (int i = 0; i < images.Length; i++)
            {
               
                if (keyResource.Equals(actResources[i]))
                {
                    images[i].sprite = Resources.Load<Sprite>(actResources[i]);
                }
                else
                {
                    images[i].sprite = Resources.Load<Sprite>(normResources[i]);
                }
            }

        }
    }
}

