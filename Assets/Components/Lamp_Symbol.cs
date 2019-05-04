using UnityEngine;

public class Lamp_Symbol : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Vector3 position; //used to fix the position of the symbol when its parent is moved (the parent is a mask used with the lamp)
}