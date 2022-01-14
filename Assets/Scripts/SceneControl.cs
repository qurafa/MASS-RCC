using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Text;
using c = TerrainAndTasks.ControlScript;

namespace TerrainAndTasks
{
    public class SceneControl : MonoBehaviour
    {
        //private int mode, pMode, rMode;
        public CanvasGroup trottleGroup, thrustGroup;
      
        public Slider sicknessSlider, taskSlider;
        public InputField sicknessField, diffField, userName;
      
        
        //private int[] runsArray;
        // Start is called before the first frame update
        void Start()
        {
            
            

            // Sprites


            // if practice
            if (c.eMode == (int)c.expMode.practice)
            {
                //trottleGroup.alpha = 1.0f;
                //thrustGroup.alpha = 1.0f;
                //if (c.pMode == (int)c.practiceMode.thrust)
                //{
                //    GetComponent<DroneMainScript>().pitchDown = KeyCode.None;
                //    GetComponent<DroneMainScript>().pitchUp = KeyCode.None;
                //    GetComponent<DroneMainScript>().rollLeft = KeyCode.None;
                //    GetComponent<DroneMainScript>().rollRight = KeyCode.None;
                //    GetComponent<DroneMainScript>().yawLeft = KeyCode.None;
                //    GetComponent<DroneMainScript>().yawRight = KeyCode.None;
                    
                //}
                //if (c.pMode == (int)c.practiceMode.rotate)
                //{
                //    GetComponent<DroneMainScript>().thrustForward = KeyCode.None;
                //    GetComponent<DroneMainScript>().thrustBackward = KeyCode.None;
                //    GetComponent<DroneMainScript>().thrustRight = KeyCode.None;
                //    GetComponent<DroneMainScript>().thrustLeft = KeyCode.None;
                //    GetComponent<DroneMainScript>().throttleUp = KeyCode.None;
                //    GetComponent<DroneMainScript>().throttleDown = KeyCode.None;
                    
                //}
            }                 
        }

        // Update is called once per frame
        void Update()
        {
            

        }

        
        public void startExperiment()
        {
            if (!c.experimentSet) 
            {
                // set experiment
                c.eMode = (int)c.expMode.init;    // initialization mode
                c.runsArray = new int[c.trials.Length];
                shuffleArray(c.runsArray);
                c.runCount = 0;
                c.numRuns = c.runsArray.Length;
                c.gsrDataWriter = new StringBuilder();
                c.pulseDataWriter = new StringBuilder();
                // create experiment directory
                if (!string.IsNullOrEmpty(userName.text))
                {
                    c.userName = userName.text + @"\";
                }
                c.mainPath = c.logPath + c.userName;
                DirectoryInfo logInfo = Directory.CreateDirectory(c.mainPath);   // create directory for participant
                c.experimentSet = true;
                checkMode();
            }
        }

        public void endExperiment()
        {
            if (c.experimentSet)
            {
                // record all data

                c.experimentSet = false;
                checkMode();
            }
        }

        private void checkMode()
        {
            switch (c.eMode)
            {
                case (int)c.expMode.init:     //Initialization
                    //createExperimentDir();          // create experiment directory
                    c.flyState = false;
                    SceneManager.LoadScene("Experiment Intro");
                    break;
                case (int)c.expMode.end:
                    if (c.experimentSet)
                    {
                        c.flyState = false;
                        SceneManager.LoadScene("Experiment End");
                    }
                    else
                    {
                        SceneManager.LoadScene("Control Scene");
                    }
                    break;
                case (int)c.expMode.practice:  //practice mode
                    if (c.pMode == (int)c.practiceMode.init)
                    {
                        //c.exp.practiceMode = (int)Experiment.pMode.demo;
                        c.flyState = false;
                        SceneManager.LoadScene("Practice Intro");
                    }
                    else if (c.pMode == (int)c.practiceMode.run)
                    {
                        c.flyState = true;
                        SceneManager.LoadScene("Terrain Key-Practice");
                    }
                    else if (c.pMode == (int)c.practiceMode.end)
                    {
                        c.flyState = false;
                        SceneManager.LoadScene("Practice End");
                    }
                    break;
                case (int)c.expMode.run:    //run mode
                    if(c.rMode == (int)c.runMode.init)
                    {
                        c.flyState = false;
                        SceneManager.LoadScene("Run Start");
                    }
                    else if(c.rMode == (int)c.runMode.run)
                    {
                        // Create trial subdirectory 
                        c.subPath = c.logPath + c.userName + c.trials[c.runsArray[c.runCount]] + @"\";
                        DirectoryInfo dirInfo = Directory.CreateDirectory(c.subPath);
                        c.dataWriter = new StringBuilder();
                        c.gsrDataWriter = new StringBuilder();
                        c.pulseDataWriter = new StringBuilder();
                        c.numCollisions = 0;
                        var line = string.Format("{0},{1},{2}", "Sickcess", "Difficulty", "Collisions");
                        c.dataWriter.AppendLine(line);

                        var line1 = string.Format("{0},{1}", "Time (s)", "PulseRate (bpm)");
                        c.pulseDataWriter.AppendLine(line1);

                        var line2 = string.Format("{0},{1}", "Time (s)", "Conductance (uS)");
                        c.pulseDataWriter.AppendLine(line2);

                        string[] taskCode = Regex.Split(c.trials[c.runsArray[c.runCount]], "_");  //split the code
                       
                        c.flyState = true;
                        if (taskCode[1].Equals("Turb1"))
                        {
                            c.turbulence = c.turbulences[0];
                        }
                        else if (taskCode[1].Equals("Turb2"))
                        {
                            c.turbulence = c.turbulences[1];
                        }
                        else if (taskCode[1].Equals("Turb3"))
                        {
                            c.turbulence = c.turbulences[2];
                        }
                        if (taskCode[0].Equals("Dens1"))
                        {
                            
                            SceneManager.LoadScene("Terrain_20");
                        }
                        else if (taskCode[0].Equals("Dens2"))
                        {
                            SceneManager.LoadScene("Terrain_40");
                        }
                        else if (taskCode[0].Equals("Dens3"))
                        {
                            SceneManager.LoadScene("Terrain_60");
                        }
                    }
                    else if(c.rMode == (int)c.runMode.end)
                    {
                        SceneManager.LoadScene("Run End-Feedback");
                    }
                    
                    break;

            }
        }

        private void shuffleArray(int[] inArray)
        {
            int n = inArray.Length;
            int[] array = new int[n];
            int[] tmp = new int[n];
            int i = 0;
            List<int> randomList = new List<int>();
            System.Random random = new System.Random();
            while (i < n)
            {
                int num = random.Next(0, inArray.Length);
                if (!randomList.Contains(num))
                {
                    randomList.Add(num);
                    i += 1;
                }
            }
            randomList.CopyTo(inArray);
        }

        public void Continue()
        {
            if (c.experimentSet)
            {
                switch (c.eMode)
                {
                    case (int)c.expMode.init:     //switch to practice mode
                        c.eMode = (int)c.expMode.practice;
                        c.pMode = (int)c.practiceMode.init;
                        checkMode();
                        break;
                    case (int)c.expMode.end:
                        if (c.experimentSet)
                        {
                            c.experimentSet = false;
                            checkMode();
                            //SceneManager.LoadScene("controlScene");
                        }
                        break;
                    case (int)c.expMode.practice:  //practice mode
                        if (c.pMode == (int)c.practiceMode.init)
                        {
                            c.pMode = (int)c.practiceMode.run;
                            checkMode();
                        }
                        else if (c.pMode == (int)c.practiceMode.run)
                        {
                            c.pMode = (int)c.practiceMode.end;
                            checkMode();
                        }
                        else if (c.pMode == (int)c.practiceMode.end)
                        {
                            c.eMode = (int)c.expMode.run;
                            c.rMode = (int)c.runMode.init;
                            checkMode();
                            //string[] pCode = Regex.Split(c.exp.practiceArray[c.exp.pCount], "_");  //split practice code
                            //c.exp.speed = Convert.ToSingle(pCode[0]);
                            //c.exp.task = pCode[1];
                            //logHeading();
                        }
                        break;
                    case (int)c.expMode.run:    //run mode
                        if(c.rMode == (int)c.runMode.init)
                        {
                            c.rMode = (int)c.runMode.run;
                        }
                        else if (c.rMode == (int)c.runMode.end)
                        {
                            string sickScore = sicknessSlider.value.ToString();
                            string diffScore = taskSlider.value.ToString();
                            var line = string.Format("{0},{1},{2}", sickScore, diffScore, c.numCollisions);
                            c.dataWriter.AppendLine(line);
                            saveAllData();

                            c.runCount += 1;
                            if (c.runCount < c.numRuns)
                            {
                                c.rMode = (int)c.runMode.init;
                            }
                            else
                            {
                                c.eMode = (int)c.expMode.end;
                            }
                        }
                        c.time = 0;
                        checkMode();
                        break;

                }
            }
        }

        private void saveAllData()
        {
            string path = c.subPath + "subjective.csv";
            File.WriteAllText(path, c.dataWriter.ToString());
            path = c.subPath + "gsr.csv";
            File.WriteAllText(path, c.gsrDataWriter.ToString());
            path = c.subPath + "pulse.csv";
            File.WriteAllText(path, c.pulseDataWriter.ToString());
        }

        public void Back()
        {
            if (c.eMode == (int)c.expMode.practice && c.pMode == (int)c.practiceMode.end)
            {
                c.pMode = (int)c.practiceMode.run;
                checkMode();
            }
        }

        public void Exit()
        {

            c.eMode = (int)c.expMode.end;
            checkMode();
        }


        public void ConcludeRun()
        {
            c.flyState = false;
            if(c.eMode == (int)c.expMode.practice)
            {
                c.pMode = (int)c.practiceMode.end;
            }
            if(c.eMode == (int)c.expMode.run)
            {
                c.rMode = (int)c.runMode.end;
            }
            checkMode();
            
        }

        public void updateTaskDiffField()
        {
            diffField.text = taskSlider.value.ToString();
            c.taskDiffScore = taskSlider.value.ToString();
        }

        public void updateSicknessField()
        {
            sicknessField.text = sicknessSlider.value.ToString();
            c.sicknessScore = sicknessSlider.value.ToString();
        }

        public void OnCollisionEnter(Collision col)
        {
            //print("collided");
            if(col.GetType() == typeof(CapsuleCollider))
            {
                c.numCollisions += 1;
                //print(c.numCollisions);
            }
        }
        



    }
}

