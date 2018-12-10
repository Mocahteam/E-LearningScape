using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public Color pointerOverColor;
    private Text buttonText;
    private Color initialColor;

    // Use this for initialization
    void Start () {
        buttonText = this.GetComponentInChildren<Text>();
        initialColor = buttonText.color;
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = pointerOverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = initialColor;
    }
}
