using UnityEngine;

public class ActionPerformedForLRS : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    public string verb;
    public string objectType;
    public string objectName;

    public bool result = false;
    public bool completed;
    public bool success;
    public string response = null;
    public int? score = null;
    public float duration = 0;
}