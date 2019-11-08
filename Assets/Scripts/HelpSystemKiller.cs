using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpSystemKiller : MonoBehaviour {

    void OnDestroy()
    {
        HelpSystem.killThread = true;
    }
}
