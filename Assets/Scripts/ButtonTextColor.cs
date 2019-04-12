using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public Color pointerOverColor;
    private TMP_Text buttonText;
    private Color initialColor;

    // Use this for initialization
    void Start () {
        buttonText = this.transform.GetChild(0).GetComponent<TMP_Text>();
        if (this.transform.GetChild(0).GetComponent<FadingMenu>())
            initialColor = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, this.transform.GetChild(0).GetComponent<FadingMenu>().finalAlpha);
        else
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
