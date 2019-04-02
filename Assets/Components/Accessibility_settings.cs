using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FYFY;
using UnityEngine.UI;

public class Accessibility_settings : MonoBehaviour {

    public bool enableFont = false; // bouton que l'utilisateur coche ou non pour avoir les parametre defini ici
    public Font accessibleFont; //dans unity et accessibleFont est glisee la police souhaitee. La police ici est la police dys Accessible dfa
    public TMP_FontAsset accessibleFontTMPro; //dans unity et accessibleFontTMPro est glisee la police souhaitee. Police dys Accessible dfa
    public Font defaultFont; //dans unity et accessibleFont est glisee la police souhaitee. Police du jeu Flabby
    public TMP_FontAsset defaultFontTMPro; //dans unity et accessibleFontTMPro est glisee la police souhaitee. Flabby 
    public bool animate = true;
    //public Font standarFont; //Arial
    //public TMP_FontAsset standartFontTMPro; // Arial

    /*public Dropdown dropdown;
    public TextMeshProUGUI policeName;
    
    List<string> m_PoliceOptions = new List<string> { "Flabby Bums handwriting", "Dys Accessible DfA", "LibaerationSANS" };
        
    public void Dropdown_Index(int index)
    {
        policeName.text = m_PoliceOptions[index];
    }

    public void policeList()
    {
        dropdown.AddOptions(m_PoliceOptions);
    }*/
   
    
    public void toggleAccessibleSettings(bool newState)
    {
        enableFont = newState;
        GameObjectManager.addComponent<UpdateFont>(this.gameObject);
    }

    public void onSliderFontSizeUpdate(float size)
    {
        GameObjectManager.addComponent<UpdateFontSize>(this.gameObject, new { newFontSize = size });
    }

    public void onSliderFontOutlineUpdate(float size)
    {
        GameObjectManager.addComponent<UpdateFontOutline>(this.gameObject, new { newWidthContour = size });
    }

    public void optionAnimation(bool newState)
    {
        SpritesAnimator.instance.Pause = !newState;

    }

}
