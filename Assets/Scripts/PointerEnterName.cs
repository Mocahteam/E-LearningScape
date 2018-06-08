using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FYFY_plugins.PointerManager;

public class PointerEnterName : MonoBehaviour, IPointerEnterHandler {

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(this.name);
    }
}
