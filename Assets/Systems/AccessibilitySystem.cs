using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;

public class AccessibilitySystem : FSystem {

    //creation de famille qui recupere tous les components type Accessibility_settings
    private Family accessibilitySettings_f = FamilyManager.getFamily(new AllOfComponents(typeof(Accessibility_settings)));
    private Family accessibilityButton_f = FamilyManager.getFamily(new AllOfComponents(typeof(AccessibilityButton)));

    //creation de famille qui recupere tous les components type Text; TextMeshPro et TextMeshProUGUI
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Text), typeof(TextMeshPro), typeof(TextMeshProUGUI)));

    private Accessibility_settings accessSettings;
    private AccessibilityButton accessButton;

    public AccessibilitySystem ()
    {
        if (Application.isPlaying)
        {
            //recuparation du tout premier component type Accessibility_settings
            accessSettings = accessibilitySettings_f.First().GetComponent<Accessibility_settings>();
            if (accessButton.buttonOn) //Si enableFont de Accessibility_Settings est selectionnee
            {
                foreach (GameObject go in text_f) //parcours de tous les GO de la famille text_f
                {
                    Text txt = go.GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.font = accessSettings.accessibleFont; //attribution de la police defini dans Accessibility_settings à txt
                        //GameObjectManager.addComponent<Contour>(go, new { m_size = 1f}); Pour parametrer directement la taille par défaut
                        GameObjectManager.addComponent<Contour>(go);
                    }
                    TextMeshPro tm = go.GetComponent<TextMeshPro>();
                    if (tm != null)
                        tm.font = accessSettings.accessibleFontTMPro;
                    TextMeshProUGUI tmGUI = go.GetComponent<TextMeshProUGUI>();
                    if (tmGUI != null)
                        tmGUI.font = accessSettings.accessibleFontTMPro;



                }
            }
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}
}