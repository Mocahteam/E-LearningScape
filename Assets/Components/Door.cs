using UnityEngine;

public class Door : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public AudioClip openAudio;
    public bool isOpened;
    public GameObject loadsOnOpen;
}