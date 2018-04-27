using UnityEngine;

public class CollectableGO : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool onInventory = false;
    public static bool usingWire = false;
    public static bool usingKeyE03 = false;
    public static bool usingKeyE08 = false;
	public static bool usingGlasses = false;
	public static bool usingLamp = false;

    public GameObject goui; //the corresponding gameobject or ui element
}