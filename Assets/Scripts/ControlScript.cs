using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace TerrainAndTasks
{
    public class ControlScript
    {
        public static string logPath = @"C:\Users\zckog\Google Drive (drzckog@gmail.com)\DRDC Project\Path-Creator-master - Copy\Path-Creator-master\terrain\ExperimentLogs\";
        public static string userName = @"user\";
        public static string neuLogBatchURL = @"C:\Users\zckog\Google Drive (drzckog@gmail.com)\DRDC Project\Path-Creator-master - Copy\Path-Creator-master\terrain\startNeulog.cmd";
        public static string mainPath, subPath;

        public static string neuLogData;
        public static string NeuLogGetSample = "http://localhost:22002/NeuLogAPI?GetSensorValue:[GSR],[1]";
        public static string neuLogPort = "22002";
        public static int[,] apiSampling = new int[,] { { 5, 100 }, { 6, 50 }, { 7, 20 }, { 8, 10 }, { 9, 5 }, { 10, 2 } };
        public static int sampleIndex = 3;
        public static string http_header = "http://localhost:";
        public static string comm_header = "NeuLogAPI?";
        public static StringBuilder dataWriter;
        public static StringBuilder gsrDataWriter, pulseDataWriter;
        public static float time;
        public static float ExpDuration = 60.0f; //300.0f;
        public static bool flyState = false;
        public static bool gotData = false;
        public static bool experimentSet = false;
        public static string[] trials = {"Dens1_Turb1", "Dens1_Turb2", "Dens1_Turb3", "Dens2_Turb1", "Dens2_Turb2", "Dens2_Turb3",
                                         "Dens3_Turb1", "Dens3_Turb2", "Dens3_Turb3"};
        public static int[] runsArray;
        public static string[] densities = { "20", "40", "60" };
        public static float[] turbulences = { 0f, 0.05f, 0.1f };
        public static float turbulence = 0;
        public enum expMode { init, practice, run, end };
        public enum practiceMode {init, thrust, rotate, run, end };  // practice modes
        public enum runMode {init, run, end };
        public enum neulogState { starting, getting, stopping};
        public static int sensorState = (int)neulogState.stopping;
        public static int eMode, pMode, rMode;
        public static int numCalls = 0;
        public static int runCount, numRuns;
        public static string taskDiffScore, sicknessScore, vectionScore;
        public static int numCollisions = 0, numCrashes = 0;
    }
}

