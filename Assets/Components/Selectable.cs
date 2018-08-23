using UnityEngine;

public class Selectable : MonoBehaviour {//objects that can be selected by the player (whit the left click)
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Vector3 standingPosDelta; // define delta position to move the player near from the selected Game Object
    public Vector3 standingOrientation; // define which direction the camera as to look at
}