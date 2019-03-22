using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FYFY;

public class Accessibility_settings : MonoBehaviour {

    public bool enableFont = false; // bouton que l'utilisateur coche ou non pour avoir les parametre defini ici
    public Font accessibleFont; //dans unity et accessibleFont est glisee la police souhaitee
    public TMP_FontAsset accessibleFontTMPro; //dans unity et accessibleFontTMPro est glisee la police souhaitee
    public Font defaultFont; //dans unity et accessibleFont est glisee la police souhaitee
    public TMP_FontAsset defaultFontTMPro; //dans unity et accessibleFontTMPro est glisee la police souhaitee


    public void toggleAccessibleSettings(bool newState)
    {
        enableFont = newState;
        GameObjectManager.addComponent<UpdateFont>(this.gameObject);
    }

}
