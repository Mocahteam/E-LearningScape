using UnityEngine;

public class BagImage : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Sprite image0; //displayed when no glasses used
    public Sprite image1; //displayed when yellow glasses used
    public Sprite image2; //displayed when red glasses used
    public Sprite image3; //displayed when both glasses are used
}