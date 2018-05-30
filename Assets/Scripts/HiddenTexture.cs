using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenTexture : MonoBehaviour {
	public GameObject light;
	private Transform tfLight;

	// Use this for initialization
	void Start () {
		if(light) tfLight = light.transform;
	}

	// Update is called once per frame
	void Update () {
		if(tfLight) {
			GetComponent<Renderer>().material.SetVector("_LightPos", tfLight.position);
    	GetComponent<Renderer>().material.SetVector("_LightDir", tfLight.forward);
		}
	}
}
