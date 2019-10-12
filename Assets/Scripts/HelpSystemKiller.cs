using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HelpSystemKiller : MonoBehaviour {

    void OnDestroy()
    {
        HelpSystem.killThread = true;
    }
}
