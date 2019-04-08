using UnityEngine;

public class MaxFontSize : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public int maxSize;
    [HideInInspector]
    public float defaultSize;
}