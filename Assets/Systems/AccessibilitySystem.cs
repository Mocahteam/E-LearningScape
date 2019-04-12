using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AccessibilitySystem : FSystem {

    //creation de famille qui recupere tous les components type Accessibility_settings
    private Family needUpdateFont_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFont), typeof(Accessibility_settings)));

    private Family needUpdateFontColor_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontColor), typeof(Accessibility_settings)));
    private Family needUpdateFontSize_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontSize)));
    private Family needUpdateFontOutlineWidth_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontOutline)));
    private Family countourSlider_f = FamilyManager.getFamily(new AllOfComponents(typeof(Slider)), new AnyOfTags("TMP_Contour"));

    private Family UIColorAlpha_f = FamilyManager.getFamily(new AnyOfTags("UI_colorAlpha", "DreamFragmentUI"), new AllOfComponents(typeof(Image)));
    private Family needUpdateColorAlpha_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateOpacity)));

    //creation de famille qui recupere tous les components type Text; TextMeshPro et TextMeshProUGUI
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents (typeof(TextMeshPro), typeof(TextMeshProUGUI)));
    private Family textWithMax_f = FamilyManager.getFamily(new AnyOfComponents(typeof(TextMeshPro), typeof(TextMeshProUGUI)), new AllOfComponents(typeof(MaxFontSize)));
    private Family textContour_f = FamilyManager.getFamily(new AnyOfComponents(typeof(TextMeshPro), typeof(TextMeshProUGUI)), new
        AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family needUpdateAnimation_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateAnimation), typeof(Accessibility_settings)));
    private Family AnimatedObject_f = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)), new NoneOfTags("PlankE09", "InventoryElements", "UIEffect", "LockArrow"), new NoneOfComponents(typeof(Button)));

    //Ne comprend que ce qui le tag DreamFragmentUI
    private Family checkTags_f = FamilyManager.getFamily(new AnyOfTags("UIBackground"));
    

    //private Dictionary 

    public AccessibilitySystem ()
    {
        if (Application.isPlaying)
        {
            needUpdateFont_f.addEntryCallback(onNeedUpdateFont); //Ecouteur qui regarde quand un nouvel element rentre dans la famille et dans ce cas appel la méthode onNeedUpdate
            needUpdateFontSize_f.addEntryCallback(onNeedUpdateFontSize); //A chaque fois qu'on touche à la sliderBar taille police, on est rentré dans la famille needUpdateFontSize_f
            needUpdateFontOutlineWidth_f.addEntryCallback(onNeedUpdateFontOutlineWidth);
            textContour_f.addEntryCallback(onNewTextMeshProEnabled);
            needUpdateColorAlpha_f.addEntryCallback(onNeedUpdateAlpha);
            needUpdateAnimation_f.addEntryCallback(onNeedUpdateAnimation);
            needUpdateFontColor_f.addEntryCallback(onNeedUpdateFontColor);

            foreach (GameObject go in textWithMax_f)
            {
                go.GetComponent<MaxFontSize>().defaultSize = go.GetComponent<TMP_Text>().fontSize; 
            }

            //Permet de filtrer et d'identifier dans la console tous les objets qui ont le tag DreamFragmentUI
            Debug.Log("Tag Filter Start");
            foreach (GameObject go in checkTags_f)
            {
                Debug.Log(go.name);
            }
            Debug.Log("Tag Filter End");
        }
    }

    private void onNeedUpdateAlpha (GameObject go)
    {
        UpdateOpacity uo = go.GetComponent<UpdateOpacity>();
        foreach (GameObject alpha in UIColorAlpha_f)
        {
            Image ImageAlpha = alpha.GetComponent<Image>();
            ImageAlpha.color = new Color(ImageAlpha.color.r, ImageAlpha.color.g, ImageAlpha.color.b, uo.newColorAlpha);
        }
        GameObjectManager.removeComponent<UpdateOpacity>(go);
    }

    // Script pour modifier l'épaisseur contour des text pour chaque nouveau TMP s'activant (voir commentaire fonction "onNeedUpdateFontOutlineWidth") pour gérer le cas des TMPGUI
    // non actifs au moment où le slider est déplacé
    private void onNewTextMeshProEnabled(GameObject go)
    {
        Slider slider = countourSlider_f.First().GetComponent<Slider>();
        TMP_Text thickness = go.GetComponent<TMP_Text>();
        thickness.outlineWidth = slider.value;
        thickness.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, slider.value);
        
    }

    //Script pour modifier l'épaisseur contour des text
    private void onNeedUpdateFontOutlineWidth(GameObject go)
    {
        
        UpdateFontOutline ufo = go.GetComponent<UpdateFontOutline>();
        // Les TMPGUI ne contiennent pas de Material quand ils ne sont pas actifs dans la hierarchie => impossible de définir leur propriété "outlineWidth"
        // Donc on ne parcours que les TP actifs dans la hierarchie (cf famille : textContour_f)
        foreach (GameObject textFontOutline in textContour_f)
        {
            TMP_Text thickness = textFontOutline.GetComponent<TMP_Text>();
            thickness.outlineWidth = ufo.newWidthContour;
            thickness.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, ufo.newWidthContour);
        }
        GameObjectManager.removeComponent<UpdateFontOutline>(go);
    }

    //Script pour modifier la taille de la police 
    private void onNeedUpdateFontSize (GameObject go)
    {
        UpdateFontSize ufs = go.GetComponent<UpdateFontSize>();
        MaxFontSize mfs;
        foreach (GameObject textSize in textWithMax_f)
        {
            TMP_Text tmFontSize = textSize.GetComponent<TMP_Text>();
            mfs = textSize.GetComponent<MaxFontSize>();
            tmFontSize.fontSize = Mathf.Min(mfs.defaultSize + ufs.newFontSize, mfs.maxSize);
       
        }
        GameObjectManager.removeComponent<UpdateFontSize>(go);
    }

    private void onNeedUpdateFontColor (GameObject go)
    {
        Accessibility_settings accessSettings = go.GetComponent<Accessibility_settings>();
        Color textFontColor;

        if (!accessSettings.enableFontColor)
            textFontColor = accessSettings.defaultFontColor;
        else
        {
            textFontColor = accessSettings.couleur1;
            /*foreach (Transform child in go.transform)
            {
                if (child.gameObject.name == "FontColor")
                {
                    foreach (Transform c in child)
                    {
                        if (c.gameObject.name == "jaune")
                            textFontColor = accessSettings.color[0];
                        
                        if (c.gameObject.name == "rouge")
                            textFontColor = accessSettings.color[1];
                    }
                }
            }*/

        }
        foreach (GameObject textColor in text_f)
        {
            TMP_Text tmFontColor = textColor.GetComponent<TMP_Text>();
            tmFontColor.color = textFontColor;
        }
        //}
        GameObjectManager.removeComponent<UpdateFontColor>(go);
    }

    //Script pour switcher entre police par défaut et police accessible 
    private void onNeedUpdateFont(GameObject go)
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
            TMP_Text tm = textGo.GetComponent<TMP_Text>();
            tm.font = TM_Font;
        }
        
        GameObjectManager.removeComponent<UpdateFont>(go); //tuer updatefont sinon pas de possibilité de changer de police car tous les éléments seront déjà dans la famille
        
    }

    private void onNeedUpdateAnimation(GameObject go)
    {
        Accessibility_settings accessSettings = go.GetComponent<Accessibility_settings>();

        foreach (GameObject objAnimated in AnimatedObject_f) //parcours de tous les GO de la famille text_f
        {
            objAnimated.GetComponent<AnimatedSprites>().animate = accessSettings.animate;
        }

        GameObjectManager.removeComponent<UpdateAnimation>(go); 
    }
    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        
    }
}