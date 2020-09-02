using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : FSystem {
    
    private Family DefaultSetting_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Slider), typeof(Toggle)), new AllOfComponents(typeof(DefaultValueSetting)));
    private Family UIColorAlpha_f = FamilyManager.getFamily(new AnyOfTags("UIBackground", "DreamFragmentUI", "Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(Image)));
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents (typeof(TextMeshPro), typeof(TextMeshProUGUI)));
    private Family AnimatedObject_f = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)), new NoneOfTags("InventoryElements", "UIEffect"));

    private Family UICursorSize_f = FamilyManager.getFamily(new AnyOfTags("CursorImage"));

    public static SettingsManager instance = null;

    public SettingsManager ()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject go in DefaultSetting_f)
            {
                if (go.GetComponent<Slider>())
                    go.GetComponent<DefaultValueSetting>().defaultValue = go.GetComponent<Slider>().value;
                else
                    go.GetComponent<DefaultValueSetting>().defaultValue = go.GetComponent<Toggle>().isOn ? 1 : 0;
            }
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

    public void ResetDefaultValues()
    {
        foreach (GameObject go in DefaultSetting_f)
        {
            if (go.GetComponent<Slider>())
                go.GetComponent<Slider>().value = go.GetComponent<DefaultValueSetting>().defaultValue;
            else if (go.GetComponent<Toggle>())
                go.GetComponent<Toggle>().isOn = go.GetComponent<DefaultValueSetting>().defaultValue != 0;
        }
    }
}