using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using FYFY;
using UnityEngine.PostProcessing;

public class SetResolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropDown;
    public Toggle toggle;
    public TMP_Dropdown qualityDropDown;


    private Family f_vlr;
    private Family f_postProcessBehaviour;
    private Family f_postProcessProfiles;
    private Family f_particles;
    private Family f_reflectionProbe;

    // Start is called before the first frame update
    void Start()
    {
        // Because we are not inside a system we init families inside Start
        f_vlr = FamilyManager.getFamily(new AllOfComponents(typeof(VolumetricLightRenderer)));
        f_postProcessBehaviour = FamilyManager.getFamily(new AllOfComponents(typeof(PostProcessingBehaviour)));
        f_postProcessProfiles = FamilyManager.getFamily(new AllOfComponents(typeof(PostProcessingProfiles)));
        f_particles = FamilyManager.getFamily(new AllOfComponents(typeof(ParticleSystem)));
        f_reflectionProbe = FamilyManager.getFamily(new AllOfComponents(typeof(ReflectionProbe)));

        Resolution[] resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        resolutionDropDown.value = 0;

        // Set dropdown with 16:9 resolutions
        foreach (Resolution resolution in resolutions)
        {
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(resolution.width + " x " + resolution.height);
            // check if this resolution is already included into dropdown
            bool found = false;
            foreach (TMP_Dropdown.OptionData option in resolutionDropDown.options)
            {
                if (option.text == newOption.text)
                    found = true;
            }
            if (!found && resolution.width/resolution.height == 16/9)
            {
                resolutionDropDown.options.Add(newOption);
                if (resolution.width == Screen.width && resolution.height == Screen.height)
                    resolutionDropDown.value = resolutionDropDown.options.Count - 1;
            }
        }

        if (resolutionDropDown.options.Count == 0)
        {
            // No 16:9 resolution found we propose all others resolutions
            foreach (Resolution resolution in resolutions)
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(resolution.width + " x " + resolution.height);
                // check if this resolution is already included into dropdown
                bool found = false;
                foreach (TMP_Dropdown.OptionData option in resolutionDropDown.options)
                {
                    if (option.text == newOption.text)
                        found = true;
                }
                if (!found)
                {
                    resolutionDropDown.options.Add(newOption);
                    if (resolution.width == Screen.width && resolution.height == Screen.height)
                        resolutionDropDown.value = resolutionDropDown.options.Count - 1;
                }
            }
        }

        string[] qualities = QualitySettings.names;
        // Set dropdown qualities
        qualityDropDown.ClearOptions();
        qualityDropDown.value = 2;
        foreach (string quality in qualities)
        {
            qualityDropDown.options.Add(new TMP_Dropdown.OptionData(quality));
            if (qualityDropDown.options.Count-1 == QualitySettings.GetQualityLevel())
                qualityDropDown.value = resolutionDropDown.options.Count - 1;
        }

        toggle.isOn = !Screen.fullScreen;

        optimizeQuality();
    }

    private void optimizeQuality()
    {
        // Set specific quality settings
        if (QualitySettings.GetQualityLevel() == 0)
        {
            Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier1;
            foreach (GameObject vlrGo in f_vlr)
                vlrGo.GetComponent<VolumetricLightRenderer>().enabled = false;
            foreach (GameObject ppGo in f_postProcessBehaviour)
                ppGo.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[0];
            // disable reflect in first room
            GameObjectManager.setGameObjectState(f_reflectionProbe.First(), false);
            // disable particles
            foreach (GameObject partGo in f_particles)
                if (!partGo.GetComponentInParent<DreamFragment>()) // do not touch DreamFragment
                    GameObjectManager.setGameObjectState(partGo, false);
        }
        else if (QualitySettings.GetQualityLevel() == 1)
        {
            Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier2;
            foreach (GameObject vlrGo in f_vlr)
                vlrGo.GetComponent<VolumetricLightRenderer>().enabled = false;
            foreach (GameObject ppGo in f_postProcessBehaviour) // use for the First Person Character post process of the main Menu camera
                ppGo.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[1];
            // enable reflect in first room
            GameObjectManager.setGameObjectState(f_reflectionProbe.First(), true);
            foreach (GameObject partGo in f_particles)
                if (partGo.name.Contains("Poussiere particule")) // disable particles of the first room
                    GameObjectManager.setGameObjectState(partGo, false);
                else if (!partGo.GetComponentInParent<DreamFragment>()) // do not touch DreamFragment
                    GameObjectManager.setGameObjectState(partGo, true);
        }
        else if (QualitySettings.GetQualityLevel() == 2)
        {
            Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier3;
            foreach (GameObject vlrGo in f_vlr)
                vlrGo.GetComponent<VolumetricLightRenderer>().enabled = true;
            foreach (GameObject ppGo in f_postProcessBehaviour)
                ppGo.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[2];
            // enable reflect in first room
            GameObjectManager.setGameObjectState(f_reflectionProbe.First(), true);
            // enable particles
            foreach (GameObject partGo in f_particles)
                if (!partGo.GetComponentInParent<DreamFragment>()) // do not touch DreamFragment
                    GameObjectManager.setGameObjectState(partGo, true);
        }
    }

    public void SetResolutionAndWindow()
    {
        string[] res = resolutionDropDown.options[resolutionDropDown.value].text.Split(new string[] { " x " }, StringSplitOptions.RemoveEmptyEntries);
        int width = Int32.Parse(res[0]);
        int height = Int32.Parse(res[1]);
        Screen.SetResolution(width, height, !toggle.isOn);
        QualitySettings.SetQualityLevel(qualityDropDown.value);
        optimizeQuality();
    }
}
