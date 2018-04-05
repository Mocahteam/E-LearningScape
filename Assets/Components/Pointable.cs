using UnityEngine;

public class Pointable : MonoBehaviour {//objects that can be taken by the player (with the key "e")
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public GameObject mouseOver; // Object to show when focused
    [HideInInspector]
    public bool focused = false; //true when this gameobject is focused
    [HideInInspector]
    public bool selected = false; //true when this gameobject is focused
}