using UnityEngine;

public class DreamFragmentToggle : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	//the content gameobject assiociated to this toggle, and activated/deactivated on click
	public GameObject dreamFragmentContent;

	//colors used to change toggle color when clicked
	public Color onColor;
	public Color offColor;
}