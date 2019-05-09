using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ValueSlider : MonoBehaviour {

	TMP_Text percentageTextSlider;

	// Use this for initialization
	void Start () {
		percentageTextSlider = GetComponent<TMP_Text> ();
	}
	
	public void textUpdatePercent (float valueSlider){
		percentageTextSlider.text = Mathf.RoundToInt (valueSlider * 100) + "%";
	}

	public void textUpdate (float valueSlider){
		percentageTextSlider.text = Mathf.RoundToInt (valueSlider) + "%";
	}
}
