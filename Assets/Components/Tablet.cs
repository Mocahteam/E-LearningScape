using UnityEngine;

public class Tablet : MonoBehaviour { // A tablet question
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Door[] opens;
    public Question[] questions;
    //information displayed on the ball
    [HideInInspector]
    public bool isAnswered;
}
