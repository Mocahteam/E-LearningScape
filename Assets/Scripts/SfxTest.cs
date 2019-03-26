using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxTest : MonoBehaviour {
    public AudioSource soundObject;
 

    private void OnMouseDown()
    {
        Debug.Log("Clicked on the dream fragment");
        soundObject.Play();
    }

    private void OnMouseEnter()
    {
        Debug.Log("Passage Souris on dream fragment");
        soundObject.Play();
    }
    
}
