using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using c = TerrainAndTasks.ControlScript;

namespace TerrainAndTasks
{
    public class NeuLogCom : MonoBehaviour
    {
        private Process mStarterProcess;
        private Process mStopperProcess;
        private string neuLogData;
        public string[] samples;
        public bool expStarted = false, expEnded = false, samplesPresent = false;
        string time;
        float tps;
        bool gsrFind = false, pulseFind = false, sHeader = false;




        public void SendHttpRequest(string request)
        {
            // A correct website page.
            tps = 1 / (float)c.apiSampling[c.sampleIndex, 1];
            print(tps);
            StartCoroutine(GetRequest(request));
        }

        IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                //string[] pages = uri.Split('/');
                //int page = pages.Length - 1;

                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {

                    print("Error");
                }
                else
                {
                    if (c.sensorState == (int)c.neulogState.getting)
                    {
                        neuLogData = webRequest.downloadHandler.text;
                        GetResponse();
                    }
                    // get data if the request is good


                    //string[] s = neuLogData.Split('[')[1].Split(']');
                    //if (!c.dataWriter.Equals(null))
                    //{
                    //    c.neuLogData = s[0];
                    //}
                    // if the command is to start data collection:
                    // try until the result is true"
                    //GetResponse();
                }
            }
        }

        public void StartNeuLogAPI()
        {
            mStarterProcess = new Process();
            mStarterProcess.StartInfo.FileName = c.neuLogBatchURL;
            mStarterProcess.StartInfo.RedirectStandardError = true;
            mStarterProcess.StartInfo.RedirectStandardOutput = true;
            mStarterProcess.StartInfo.UseShellExecute = false;
            mStarterProcess.Start();
        }

        private void GetResponse()
        {
            int startPos = neuLogData.IndexOf("{");
            int endPos = neuLogData.IndexOf("}");
            string subStr = neuLogData.Substring(startPos + 1, endPos - startPos - 1);
            string[] headers = Regex.Split(subStr, "\"");
            string[] gsrSamples, pulseSamples;
            int i = 0;
            while (i < headers.Length)
            {
                //print("I got here");
                if (headers[i].Contains("GetExperimentSamples"))
                {
                    //print("GetSamplesNow");
                    i += 1;
                    sHeader = true;
                    while (i < headers.Length && !gsrFind)
                    {
                        if (headers[i].Contains("GSR"))
                        {
                            i += 1;
                            gsrSamples = Regex.Split(headers[i], ",");
                            gsrFind = true;
                            if (gsrSamples.Length > 2)
                            {
                                float dt = 0;
                                for (int j = 2; j < gsrSamples.Length - 1; j++)
                                {
                                    var line = string.Format("{0},{1}", dt.ToString(), gsrSamples[j].ToString());
                                    //print(line);
                                    c.gsrDataWriter.AppendLine(line);
                                    dt += tps;
                                }
                            }
                            //print("GSR :" + gsrSamples.Length);
                        }
                        i += 1;
                    }
                    while (i < headers.Length && !pulseFind)
                    {
                        if (headers[i].Contains("Pulse"))
                        {
                            i += 1;
                            pulseSamples = Regex.Split(headers[i], ",");
                            pulseFind = true;
                            if (pulseSamples.Length > 2)
                            {
                                float dt = 0;
                                for (int j = 2; j < pulseSamples.Length - 1; j++)
                                {
                                    var line = string.Format("{0},{1}", dt.ToString(), pulseSamples[j].ToString());
                                    c.pulseDataWriter.AppendLine(line);

                                    dt += tps;
                                }
                            }
                        }
                        i += 1;
                    }


                }
                i += 1;
            }
        }
    }
}
