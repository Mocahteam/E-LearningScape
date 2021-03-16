using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.IO;

public class SettingsManager : FSystem {

    // System has to be before LoadGameContent system
    
    private Family DefaultSetting_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Slider), typeof(Toggle)), new AllOfComponents(typeof(DefaultValueSetting)));
    private Family UIColorAlpha_f = FamilyManager.getFamily(new AnyOfTags("UIBackground", "DreamFragmentUI", "Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(Image)));
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents (typeof(TextMeshPro), typeof(TextMeshProUGUI)));
    private Family AnimatedObject_f = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites)), new NoneOfTags("InventoryElements", "UIEffect"));

    private Family UICursorSize_f = FamilyManager.getFamily(new AnyOfTags("CursorImage"));

    public static SettingsManager instance = null;

    private SettingsSave settingsSave;
    private SettingsSave tmpSettings;

    private GameObject tmpGO;

    private bool isAccessibilityFontSelected = false;

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

            text_f.addEntryCallback(OnNewText);
            instance = this;
        }
    }

    public void OnNewText(GameObject go)
    {
        if (isAccessibilityFontSelected)
        {
            TMP_Text tm = go.GetComponent<TMP_Text>();
            if (go.layer == 5) // 5 <=> UI
                tm.font = LoadGameContent.instance.AccessibleFontUI;
            else
                tm.font = LoadGameContent.instance.AccessibleFont;
        }
    }

    public void SwitchFont(bool accessibleFont)
    {
        isAccessibilityFontSelected = accessibleFont;
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
            CursorImage.rectTransform.localScale = new Vector3(newSize*4, newSize*4, 0);
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

    public void SaveSettings()
    {
        InitializeSettingSaveDictionary();

        foreach (GameObject go in DefaultSetting_f)
        {
            if (go.GetComponent<Slider>())
                settingsSave.slidersValues.Add(go.GetComponent<Slider>().value);
            else if (go.GetComponent<Toggle>())
                settingsSave.togglesValues.Add(go.GetComponent<Toggle>().isOn ? 1 : 0);
        }

        Directory.CreateDirectory("./Data");
        File.WriteAllText("./Data/GameSettings.txt", JsonUtility.ToJson(settingsSave, true));
    }

    public void LoadSettings()
    {
        if (File.Exists("./Data/GameSettings.txt"))
        {
            tmpSettings = null;
            try
            {
                // load settings from file
                tmpSettings = JsonUtility.FromJson<SettingsSave>(File.ReadAllText("./Data/GameSettings.txt"));
            }
            catch (System.Exception) { }

            if (CheckSettings(tmpSettings))
            {
                settingsSave = tmpSettings;
                // set settings using loaded data
                int sliderCounter = 0, toggleCounter = 0;
                foreach (GameObject go in DefaultSetting_f)
                {
                    if (go.GetComponent<Slider>())
                    {
                        go.GetComponent<Slider>().value = settingsSave.slidersValues[sliderCounter];
                        sliderCounter++;
                    }
                    else if (go.GetComponent<Toggle>())
                    {
                        go.GetComponent<Toggle>().isOn = settingsSave.togglesValues[toggleCounter] != 0;
                        toggleCounter++;
                    }
                }

                Debug.Log("Settings loaded");
            }
            else
            {
                Debug.LogError("Couldn't load settings from file because of invalid content.");
                File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Couldn't load settings from file because of invalid content."));
            }
        }
    }

    /// <summary>
    /// Check if the loaded dictionary is valid and has the correct number of parameters
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    private bool CheckSettings(SettingsSave settings)
    {
        if (settings != null && settings.slidersValues != null && settings.togglesValues != null)
        {
            int sliderCounter = 0, toggleCounter = 0;
            foreach (GameObject go in DefaultSetting_f)
            {
                if (go.GetComponent<Slider>())
                    sliderCounter++;
                else if (go.GetComponent<Toggle>())
                    toggleCounter++;
            }

            if (sliderCounter == settings.slidersValues.Count && toggleCounter == settings.togglesValues.Count)
                return true;
        }

        return false;
    }

    private void InitializeSettingSaveDictionary()
    {
        // initialize dictionary
        if (settingsSave == null)
            settingsSave = new SettingsSave();
        if (settingsSave.slidersValues == null)
            settingsSave.slidersValues = new List<float>();
        else
            settingsSave.slidersValues.Clear();
        if (settingsSave.togglesValues == null)
            settingsSave.togglesValues = new List<float>();
        else
            settingsSave.togglesValues.Clear();
    }
}