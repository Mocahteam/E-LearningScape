using UnityEngine;

public class ToggleableGO : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public bool focused = false; //true when the cursor is on this objectbject
	public bool toggled = false; //true when the object is being toggled
}