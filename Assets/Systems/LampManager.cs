﻿using UnityEngine;
using FYFY;

public class LampManager : FSystem {

    // this system manage the lamp and symbols

    private Family f_symbols = FamilyManager.getFamily(new AnyOfTags("E12_Symbol"));

    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));

    public GameObject fpsLight;

    private GameObject lampSelected;
    private GameObject tmpGo;

    public static LampManager instance;

    public LampManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_itemSelected.addEntryCallback(onItemSelectedInInventory);
        f_itemSelected.addExitCallback(onItemUnselectedInInventory);
    }

    private void onItemSelectedInInventory(GameObject go)
    {
        // check if it is the lamp
        if (go.name == "Lamp")
        {
            // turn on lamp
            lampSelected = go;
            GameObjectManager.setGameObjectState(fpsLight, true);
            this.Pause = false;
        }
    }

    private void onItemUnselectedInInventory(int instanceId)
    {
        // check if it is the lamp
        if (lampSelected && lampSelected.GetInstanceID() == instanceId)
        {
            // turn off lamp
            GameObjectManager.setGameObjectState(fpsLight, false);
            lampSelected = null;

            // hide all symbols in case one of them is viewed
            int nbSymbols = f_symbols.Count;
            for (int i = 0; i < nbSymbols; i++)
                GameObjectManager.setGameObjectState(f_symbols.getAt(i), false);
            this.Pause = true;
        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        if (lampSelected)
        {
            int nbSymbols = f_symbols.Count;
            for (int i = 0; i < nbSymbols; i++)
            {
                tmpGo = f_symbols.getAt(i);
                Vector3 position = tmpGo.GetComponentInChildren<Lamp_Symbol>().position;
                //if the symbol is illuminated by the lamp
                if (Vector3.Angle(position - Camera.main.transform.position, Camera.main.transform.forward) < 22 && Vector3.Angle(Camera.main.transform.forward, tmpGo.transform.forward) < 87)
                {
                    if (!tmpGo.activeSelf)
                    {
                        GameObjectManager.addComponent<ActionPerformed>(tmpGo, new { name = "activate", performedBy = "player" });
                        GameObjectManager.setGameObjectState(tmpGo, true);
                    }
                    //calculate the intersection between player direction and the wall
                    float d = Vector3.Dot((position - Camera.main.transform.position), tmpGo.transform.forward) / Vector3.Dot(Camera.main.transform.forward, tmpGo.transform.forward);
                    //move the mask to the calculated position
                    tmpGo.transform.position = Camera.main.transform.position + Camera.main.transform.forward * d;
                    //set the symbol position to its initial position (it shouldn't move but it is moved because of it being the mask's child)
                    tmpGo.GetComponentInChildren<Lamp_Symbol>().gameObject.transform.position = position;
                    //calculate the new scale of the mask (depending on the distance with the player)
                    float a = (0.026f - 0.015f) / (5.49f - 3.29f);
                    float b = 0.026f - a * 5.49f;
                    float scale = a * (tmpGo.transform.position - Camera.main.transform.position).magnitude + b;
                    //change the scale of the mask and set the symbol scale to its initial scale
                    tmpGo.GetComponentInChildren<Lamp_Symbol>().gameObject.transform.localScale *= tmpGo.transform.localScale.x / scale * tmpGo.transform.parent.localScale.x * 100;
                    tmpGo.transform.localScale = new Vector3(scale, scale, scale) / tmpGo.transform.parent.localScale.x / 100;
                }
                else if (tmpGo.activeSelf)
                {
                    //disable the mask and the symbol
                    tmpGo.transform.position = position;
                    tmpGo.GetComponentInChildren<Lamp_Symbol>().gameObject.transform.position = position; 
                    GameObjectManager.setGameObjectState(tmpGo, false);
                }
            }
        }
    }
}