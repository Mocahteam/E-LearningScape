using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : FSystem {
    
    //creation de famille qui recupere tous les components type Accessibility_settings
    //private Family needUpdateFont_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFont), typeof(Accessibility_settings)));
    private Family needUpdateDefaultSetting_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Slider), typeof(Toggle)), new AllOfComponents(typeof(DefaultValueSetting)));
    
    //private Family needUpdateFontSize_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontSize)));
    //private Family needUpdateValueSlider_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateValueSlider)));

    private Family UIColorAlpha_f = FamilyManager.getFamily(new AnyOfTags("UIBackground", "DreamFragmentUI", "Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(Image)));
   // private Family needUpdateColorAlpha_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateOpacity)));

    //creation de famille qui recupere tous les components type Text; TextMeshPro et TextMeshProUGUI
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents (typeof(TextMeshPro), typeof(TextMeshProUGUI)));
    //private Family textWithMax_f = FamilyManager.getFamily(new AnyOfComponents(typeof(TextMeshPro), typeof(TextMeshProUGUI)), new AllOfComponents(typeof(MaxFontSize)));
    
    //private Family needUpdateAnimation_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateAnimation), typeof(Accessibility_settings)));
    private Family AnimatedObject_f = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)), new NoneOfTags("PlankE09", "InventoryElements", "UIEffect", "LockArrow"), new NoneOfComponents(typeof(Button)));

    private Family UICursorSize_f = FamilyManager.getFamily(new AnyOfTags("CursorImage"));
    //private Family needUpdateCursorSize_f = FamilyManager.getFamily(new AllOfComponents(typeof(CursorSize)));

    //Ne comprend que ce qui a le tag DreamFragmentUI
    private Family checkTags_f = FamilyManager.getFamily(new AnyOfTags("NewItemFeedback"));

    private GameObject CursorUI;
    private GameObject CursorImage;

    public static SettingsManager instance = null;

    public SettingsManager ()
    {
        if (Application.isPlaying)
        {
            /*needUpdateFont_f.addEntryCallback(onNeedUpdateFont); //Ecouteur qui regarde quand un nouvel element rentre dans la famille et dans ce cas appel la méthode onNeedUpdate
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
            
            
            Debug.Log("Tag Filter Start");
            foreach (GameObject go in UIColorAlpha_f)
            {
                Debug.Log(go.name);
            }
            Debug.Log("Tag Filter End");*/

            instance = this;
        }
    }

    public void SwitchFont(bool accessibleFont)
    {
        TMP_FontAsset TM_Font;
        TMP_FontAsset TM_FontUI;
        if (accessibleFont)
        {
            // Load accessible font
            TM_Font = LoadGameContent.instance.AccessibleFont;
            TM_FontUI = LoadGameContent.instance.AccessibleFontUI;
        }
        else
        {
            // Load default font
            TM_Font = LoadGameContent.instance.DefaultFont;
            TM_FontUI = LoadGameContent.instance.DefaultFontUI;
        }

        foreach (GameObject textGo in text_f) // parse all TextMeshPro
        {
            TMP_Text tm = textGo.GetComponent<TMP_Text>();
            if (textGo.layer == 5) // 5 <=> UI
                tm.font = TM_FontUI;
            else
                tm.font = TM_Font;
        }
    }

    public void UpdateCursorSize (float newSize)
    {
        foreach (GameObject cursor in UICursorSize_f)
        {
            Image CursorImage = cursor.GetComponent<Image>();
            CursorImage.rectTransform.localScale = new Vector3(newSize*2, newSize*2, 0);
        }
    }

    public void UpdateAlpha (float newAlpha)
    {
        foreach (GameObject alpha in UIColorAlpha_f)
        {
            Image img = alpha.GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, newAlpha);
        }
    }

    public void ToggleTextAnimation(bool newState)
    {
        foreach (GameObject objAnimated in AnimatedObject_f)
            objAnimated.GetComponent<AnimatedSprites>().animate = newState;
    }

    /*

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

    */
    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        
    }
}