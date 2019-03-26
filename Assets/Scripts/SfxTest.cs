using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxTest : MonoBehaviour {
    public AudioSource fragmentReve;

    private void OnMouseDown()
    {
        Debug.Log("Clicked on the dream fragment");
        fragmentReve.Play();
    }

    private void OnMouseEnter()
    {
        Debug.Log("Passage Souris on dream fragment");
        fragmentReve.Play();
    }
    
}
