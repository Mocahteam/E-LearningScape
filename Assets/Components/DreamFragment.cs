using UnityEngine;

public class DreamFragment : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public string itemName;
    public int id;
    public int type; //0 - blue, 1 - green, 2 - endFragments
    public bool viewed = false;
    public string urlLink = ""; // if not empty display a button to access this web link
    public string linkButtonText;
}