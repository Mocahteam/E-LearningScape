using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JavaRequiredEvents : MonoBehaviour {
    public void close()
    {
        Application.Quit();
    }

    public void installJava()
    {
        Application.OpenURL("https://www.oracle.com/java/technologies/javase-downloads.html");
    }
}
