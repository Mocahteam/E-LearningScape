using UnityEngine;

public class StoryText : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public int storyProgression = 0;
    [TextArea]
    public string[] intro;
    [TextArea]
    public string[] transition;
    [TextArea]
    public string[] end;
    [TextArea]
    public string[] credit;
    public string endLink;
}