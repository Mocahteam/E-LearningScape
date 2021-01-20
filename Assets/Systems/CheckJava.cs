using UnityEngine;
using FYFY;
using System;
using System.Diagnostics;

public class CheckJava : FSystem {
    // Check if Java is installed

    private bool javaOK = false;

    public CheckJava()
    {
        if (Application.isPlaying)
        {
            if (!HelpSystem.shouldPause)
            {
                // Check if Java is installed
                Process checkJava = new Process();
                checkJava.StartInfo.FileName = "java";
                checkJava.StartInfo.Arguments = "-version";
                checkJava.StartInfo.UseShellExecute = false;
                checkJava.StartInfo.RedirectStandardError = true;
                try
                {
                    checkJava.Start();
                    string output = checkJava.StandardError.ReadLine();
                    string javaVersion = output.Split(' ')[2].Replace("\"", "");
                    javaOK = output.StartsWith("java version") && javaVersion != "";
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.Message);
                }
            }
        }
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (Time.frameCount > 10 && !javaOK)
        { // Do not load scene at the first frame => Unity Crash !!! Something wrong with GPU...
            GameObjectManager.loadScene("JavaRequired");
        }
        else if (Time.frameCount > 10)
            Pause = true;
    }
}