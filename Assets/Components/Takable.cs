using UnityEngine;

public class Takable : MonoBehaviour {//objects that can be taken by the player (with the key "e")
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool objectTaken = false; //true when a gameobject with this component is taken
    public static bool mirrorOnPlank = false;

    public bool taken = false;              //true when this component's gameobject is taken
}