using UnityEngine;

public class Selectable : MonoBehaviour {//objects that can be selected by the player (whit the left click)
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool selected = false; //true when a gameobject is selected

	public static bool askRight = false;
	public static bool askWrong = false;

    public bool isSelected = false; //true when this component's gameobject is selected
    public bool solved = false;     //true if the enigma associated to this gameobject is solved

    //audios played when the player gives an answer
    public AudioClip right;
    public AudioClip wrong;
}