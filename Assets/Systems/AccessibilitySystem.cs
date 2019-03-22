using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;

public class AccessibilitySystem : FSystem {

    //creation de famille qui recupere tous les components type Accessibility_settings
    private Family needUpdate_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFont), typeof(Accessibility_settings)));

    //creation de famille qui recupere tous les components type Text; TextMeshPro et TextMeshProUGUI
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Text), typeof(TextMeshPro), typeof(TextMeshProUGUI)));

    public AccessibilitySystem ()
    {
        if (Application.isPlaying)
        {
            //recuparation du tout premier component type Accessibility_settings
            
            needUpdate_f.addEntryCallback(onNeedUpdate); //Ecouteur qui regarde quand un nouvel element rentre dans la famille et dans ce cas appel la méthode onNeedUpdate
        }
    }

    private void onNeedUpdate(GameObject go)
    {
        Accessibility_settings accessSettings = go.GetComponent<Accessibility_settings>();

        Font textFont;
        TMP_FontAsset TM_Font;
        if (accessSettings.enableFont) //Si la case est cochée alors changement de toutes les polices
        {
            textFont = accessSettings.accessibleFont;//attribution de la police defini dans Accessibility_settings à txt
            TM_Font = accessSettings.accessibleFontTMPro;
        } else //sinon mettre les polices par défaut
        {
            textFont = accessSettings.defaultFont;
            TM_Font = accessSettings.defaultFontTMPro;
        }

        foreach (GameObject textGo in text_f) //parcours de tous les GO de la famille text_f
        {
            Text txt = textGo.GetComponent<Text>();
            if (txt != null)
            {
                txt.font = textFont;
                if (accessSettings.enableFont)
                {
                    //GameObjectManager.addComponent<Contour>(go, new { m_size = 1f}); Pour parametrer directement la taille par défaut
                    GameObjectManager.addComponent<Contour>(textGo);
                } else
                {
                    GameObjectManager.removeComponent<Contour>(textGo);
                }
            }
            TextMeshPro tm = textGo.GetComponent<TextMeshPro>();
            if (tm != null)
                tm.font = TM_Font;
            TextMeshProUGUI tmGUI = textGo.GetComponent<TextMeshProUGUI>();
            if (tmGUI != null)
                tmGUI.font = accessSettings.accessibleFontTMPro;
        }

        GameObjectManager.removeComponent<UpdateFont>(go); //tuer updatefont sinon pas de possibilité de changer de police car tous les éléments seront déjà dans la famille

    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        
    }
}