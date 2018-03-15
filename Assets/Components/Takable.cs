using UnityEngine;

public class Takable : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool objectTaken = false;

    public bool taken = false;
}