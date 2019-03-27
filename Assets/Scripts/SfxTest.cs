using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SfxTest : MonoBehaviour 
    //IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public AudioSource soundObjectMouse;
    public AudioSource soundObjectClic;

    private void OnMouseDown()
    {
        Debug.Log("Clicked on the dream fragment");
        soundObjectClic.Play();
        //if (Input.GetMouseButtonDown(0)) si on clique sur la souris
    }

    private void OnMouseEnter()
    {
        Debug.Log("Passage Souris on dream fragment");
        soundObjectMouse.Play();
    }

    /*public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Passage Souris on dream fragment");
        soundObjectMouse.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        soundObjectMouse.Stop();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked on the dream fragment");
        soundObjectClic.Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        soundObjectMouse.Stop();
    }*/

}
