using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxTest : MonoBehaviour {
    public AudioSource soundObjectMouse;
    public AudioSource soundObjectClic;


    private void OnMouseDown()
    {
        Debug.Log("Clicked on the dream fragment");
        soundObjectClic.Play();
    }

    private void OnMouseEnter()
    {
        Debug.Log("Passage Souris on dream fragment");
        soundObjectMouse.Play();
    }
    
}
