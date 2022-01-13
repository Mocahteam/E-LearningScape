using UnityEngine;
using FYFY;
using System;
using System.Diagnostics;
using System.IO;

public class CheckJava : FSystem
{
    // Check if Java is installed

    private bool javaOK = false;

    public static CheckJava instance;

    public CheckJava()
    {
        if (Application.isPlaying)
        {
            if (!HelpSystem.shouldPause)
                checkJava();
            else
                javaOK = true; // java is not required
        }
        instance = this;
    }

    public void checkJava()
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
            checkJava.WaitForExit();
            string output = checkJava.StandardError.ReadLine();
            UnityEngine.Debug.Log("Java cmd output: " + output);
            javaOK = output.Contains("java version") || output.Contains("openjdk version");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Java cmd error: " + e.Message);
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