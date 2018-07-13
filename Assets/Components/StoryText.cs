using UnityEngine;

public class StoryText : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    [TextArea]
    public string[] intro;
    [TextArea]
    public string[] transition;
    [TextArea]
    public string[] end;
}