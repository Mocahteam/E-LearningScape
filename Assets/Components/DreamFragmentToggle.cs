using UnityEngine;

public class DreamFragmentToggle : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	//the content gameobject assiociated to this toggle, and activated/deactivated on click
	public GameObject dreamFragmentContent;

	//sprites used to change toggle state
	public Sprite onState;
	public Sprite offState;
	public Sprite focusedState;
    public Sprite selectedState;
    public Sprite currentState;
}