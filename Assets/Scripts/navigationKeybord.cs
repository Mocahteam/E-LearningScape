using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class navigationKeybord : MonoBehaviour {

    public EventSystem es;
    public GameObject bouton1;
    public GameObject panelPopup;

    /*public void navigMenu()
    {
        GameObjectManager.setGameObjectState(panelPopup, true);
        es.SetSelectedGameObject(bouton1);
        es.UpdateModules();
    }*/



    /*public EventSystem eventSystem;
    public GameObject selectedObject;
    private bool buttonSelected;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Navigation simple, pas de sélection du premier bouton visible 
		if (Input.GetAxisRaw ("Vertical") != 0 && buttonSelected == false)
        {
            eventSystem.SetSelectedGameObject(selectedObject);
            buttonSelected = true;
        }
        
        //Selction visible du premier bouton avant de naviguer 
		if (Input.GetAxisRaw ("Vertical") == 0 && buttonSelected == false)
		{
			eventSystem.SetSelectedGameObject(selectedObject);
			buttonSelected = true;
		}

	}

    private void OnDisable()
    {
        buttonSelected = false;
    }*/

}
