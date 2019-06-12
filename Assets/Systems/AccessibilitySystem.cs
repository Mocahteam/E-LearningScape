using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AccessibilitySystem : FSystem {

    //creation de famille qui recupere tous les components type Accessibility_settings
    private Family needUpdateFont_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFont), typeof(Accessibility_settings)));
    private Family needUpdateDefaultSetting_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Slider), typeof(Toggle)), new AllOfComponents(typeof(DefaultValueSetting)));
    
    private Family needUpdateFontSize_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontSize)));
    private Family needUpdateValueSlider_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateValueSlider)));

    private Family UIColorAlpha_f = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfComponents(typeof(RawImage)));
    private Family needUpdateColorAlpha_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateOpacity)));

    //creation de famille qui recupere tous les components type Text; TextMeshPro et TextMeshProUGUI
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents (typeof(TextMeshPro), typeof(TextMeshProUGUI)));
    private Family textWithMax_f = FamilyManager.getFamily(new AnyOfComponents(typeof(TextMeshPro), typeof(TextMeshProUGUI)), new AllOfComponents(typeof(MaxFontSize)));
    
    private Family needUpdateAnimation_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateAnimation), typeof(Accessibility_settings)));
    private Family AnimatedObject_f = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)), new NoneOfTags("PlankE09", "InventoryElements", "UIEffect", "LockArrow"), new NoneOfComponents(typeof(Button)));

    private Family UICursorSize_f = FamilyManager.getFamily(new AnyOfTags("CursorImage"));
    private Family needUpdateCursorSize_f = FamilyManager.getFamily(new AllOfComponents(typeof(CursorSize)));

    //Ne comprend que ce qui a le tag DreamFragmentUI
    private Family checkTags_f = FamilyManager.getFamily(new AnyOfTags("NewItemFeedback"));

    private GameObject CursorUI;
    private GameObject CursorImage;

    public AccessibilitySystem ()
    {
        if (Application.isPlaying)
        {
            needUpdateFont_f.addEntryCallback(onNeedUpdateFont); //Ecouteur qui regarde quand un nouvel element rentre dans la famille et dans ce cas appel la méthode onNeedUpdate
            needUpdateFontSize_f.addEntryCallback(onNeedUpdateFontSize); //A chaque fois qu'on touche à la sliderBar taille police, on est rentré dans la famille needUpdateFontSize_f
            needUpdateValueSlider_f.addEntryCallback(onNeedUpdateResetValueSlider);
            needUpdateColorAlpha_f.addEntryCallback(onNeedUpdateAlpha);
            needUpdateAnimation_f.addEntryCallback(onNeedUpdateAnimation);
            needUpdateCursorSize_f.addEntryCallback(onNeedneedUpdateCursorSize_f);

            foreach (GameObject go in needUpdateDefaultSetting_f)
            {
                if (go.GetComponent<Slider>())
                    go.GetComponent<DefaultValueSetting>().defaultValue = go.GetComponent<Slider>().value;
                else
                    go.GetComponent<DefaultValueSetting>().defaultValue = go.GetComponent<Toggle>().isOn ? 1 : 0;
            }

            foreach (GameObject go in textWithMax_f)
            {
				go.GetComponent<MaxFontSize> ().defaultMaxSize = go.GetComponent<TMP_Text> ().fontSizeMax; 
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

    private void onNeedneedUpdateCursorSize_f (GameObject go)
    {
        CursorSize cs = go.GetComponent<CursorSize>();
        foreach (GameObject cursor in UICursorSize_f)
        {
            Image CursorImage = cursor.GetComponent<Image>();
            CursorImage.rectTransform.localScale = new Vector3(cs.newCursorSize, cs.newCursorSize, 0);
        }
        GameObjectManager.removeComponent<CursorSize>(go);
    }

    private void onNeedUpdateAlpha (GameObject go)
    {
        UpdateOpacity uo = go.GetComponent<UpdateOpacity>();
        foreach (GameObject alpha in UIColorAlpha_f)
        {
            RawImage IARalpha = alpha.GetComponent<RawImage>();
            IARalpha.color = new Color(IARalpha.color.r, IARalpha.color.g, IARalpha.color.b, uo.newColorAlpha);
        }
        GameObjectManager.removeComponent<UpdateOpacity>(go);
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
			tmFontSize.fontSizeMax = Mathf.Min(mfs.defaultMaxSize + ufs.newFontSizeMax, mfs.maxSize);
       
        }
        GameObjectManager.removeComponent<UpdateFontSize>(go);
    }

    //If we click on back value button in setting menu, we come back at default value on slider  
    private void onNeedUpdateResetValueSlider(GameObject go)
    {
        DefaultValueSetting vsl;
        foreach(GameObject backDefaultValue in needUpdateDefaultSetting_f)
        {
            if (backDefaultValue.GetComponent<Slider>())
            {
                Slider newVal = backDefaultValue.GetComponent<Slider>();
                vsl = backDefaultValue.GetComponent<DefaultValueSetting>();
                newVal.value = vsl.defaultValue;
            }
            else
            {
                Toggle newVal = backDefaultValue.GetComponent<Toggle>();
                vsl = backDefaultValue.GetComponent<DefaultValueSetting>();
                newVal.isOn = vsl.defaultValue != 0;
            }


        }
        GameObjectManager.removeComponent<UpdateValueSlider>(go);
    }
    
    //Script pour switcher entre police par défaut et police accessible 
    private void onNeedUpdateFont(GameObject go)
    {
        Accessibility_settings accessSettings = go.GetComponent<Accessibility_settings>();
        
        //Font textFont;
        TMP_FontAsset TM_Font;
        if (accessSettings.enableFont) //Si la case est cochée alors changement de toutes les polices
        {
            //textFont = accessSettings.accessibleFont;//attribution de la police defini dans Accessibility_settings à txt
            TM_Font = accessSettings.accessibleFontTMPro;
        } else //sinon mettre les polices par défaut
        {
            //textFont = accessSettings.defaultFont;
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