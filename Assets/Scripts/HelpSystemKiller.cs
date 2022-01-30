using UnityEngine;

public class HelpSystemKiller : MonoBehaviour {

    void OnDestroy()
    {
        HelpSystem.killThread = true;
    }
}
