using UnityEngine;

public class Selectable : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool selected = false;
    public bool isSelected = false;
    public bool focused = false;
    public string[] words;
    public string answer;
    public bool solved = false;
    public AudioClip right;
    public AudioClip wrong;
}