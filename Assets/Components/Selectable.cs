using UnityEngine;

public class Selectable : MonoBehaviour {//objects that can be selected by the player (whit the left click)
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool selected = false; //true when a gameobject is selected

    //when true, the feedback for right/wrong answer is played
    //set to true when a system other than "SetAnswer" needs a feedback to be played
	public static bool askRight = false;
	public static bool askWrong = false;

    public bool isSelected = false; //true when this component's gameobject is selected
    public bool solved = false;     //true if the enigma associated to this gameobject is solved

    //audios played when the player gives an answer to the enigma linked to this object
    public AudioClip right;
    public AudioClip wrong;

    public Vector3 standingPosDelta; // define delta position to move the player near from the selected Game Object
    public Vector3 standingOrientation; // define which direction the camera as to look at
}