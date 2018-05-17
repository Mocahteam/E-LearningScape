using UnityEngine;

public class ToggleableGO : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public bool focused = false;
	public bool toggled = false; //true when the object is being toggled
}