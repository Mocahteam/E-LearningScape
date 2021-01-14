using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SettingsSave {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public List<float> slidersValues;
	public List<float> togglesValues;
}