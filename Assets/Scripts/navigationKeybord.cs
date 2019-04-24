using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class navigationKeybord : MonoBehaviour {
    public EventSystem eventSystem;
    public GameObject selectedObject;
    private bool buttonSelected;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxisRaw ("Vertical") != 0 && buttonSelected == false)
        {
            eventSystem.SetSelectedGameObject(selectedObject);
            buttonSelected = true;
        }

	}

    private void OnDisable()
    {
        buttonSelected = false;
    }
}
