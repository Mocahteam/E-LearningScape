using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityButton : MonoBehaviour {
    public GameObject buttonOn, buttonOff;

    public void SetAccessibilitySettings()
    {
        bool onoffButton = gameObject.GetComponent<Toggle>().isOn;
        if (onoffButton)
        {
            buttonOn.SetActive(true);
            buttonOff.SetActive(false);
        }
        if (!onoffButton)
        {
            buttonOn.SetActive(false);
            buttonOff.SetActive(true);
        }

        //Debug.Log("Value is changing"); //Afficher ce message dans la console 

    }
}
