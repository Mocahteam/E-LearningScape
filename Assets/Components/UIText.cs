using UnityEngine;

public class UIText : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	//the name of the variable in GameContent corersponding to this UI text
	public string gameContentVariable;

	//this bool is false if the text is in a Text Mesh Pro component, and true if it is in a Text Mesh Pro UGUI component
	public bool tmpUI = true;
}