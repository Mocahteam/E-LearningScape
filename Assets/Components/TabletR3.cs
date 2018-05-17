using UnityEngine;

public class TabletR3 : MonoBehaviour { // The tablet in room 3
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Door opens;
    public Question[] questions;
    public string[] answers;
    //information displayed on the ball
    [HideInInspector]
    public bool isAnswered;
}
