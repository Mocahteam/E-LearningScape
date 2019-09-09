using UnityEngine;

public class DefaultValueSetting : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    [HideInInspector]
    public float defaultValue;
}