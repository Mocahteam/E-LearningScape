using UnityEngine;

public class CollectableGO : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool onInventory = false;
    public static bool usingWire = false;
    public static bool usingKeyE03 = false;
    public static bool usingKeyE08 = false;
	public static bool usingGlasses1 = false;
    public static bool usingGlasses2 = false;
    public static bool usingLamp = false;

    //when true the method "closeInventory" of the system "Inventory" is called
    //set to true when a system other than "Inventory" needs the inventory to be opened/closed
    public static bool askCloseInventory = false;
    public static bool askOpenInventory = false;

    public GameObject goui; //the corresponding gameobject or ui element
}