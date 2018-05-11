using UnityEngine;

public class Question : MonoBehaviour { // A tablet question
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public string[] answers;

    public GameObject answer;

    public AudioSource source;
    public AudioClip right;
    public AudioClip wrong;
    //information displayed on the ball
    [HideInInspector]
    public bool isAnswered;
}
