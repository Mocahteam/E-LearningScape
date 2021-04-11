using UnityEngine;
using FYFY;
using System.IO;

public class CheckMacOSLauncher : FSystem
{
    // Check if Java is installed

    private bool launcherUsed = true;

    public CheckMacOSLauncher()
    {
        if (Application.isPlaying)
        {
            // Check if PlayGame file exists, means MacOS context
            if (File.Exists("./PlayGame"))
            {
                // check if player use this file to launch the game
                if (!File.Exists("./usingLauncher"))
                    launcherUsed = false;
                else
                    File.Delete("./usingLauncher");
            }
        }
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (Time.frameCount > 10 && !launcherUsed)
        { // Do not load scene at the first frame => Unity Crash !!! Something wrong with GPU...
            GameObjectManager.loadScene("MacOSLauncher");
        }
        else if (Time.frameCount > 10)
            Pause = true;
    }
}