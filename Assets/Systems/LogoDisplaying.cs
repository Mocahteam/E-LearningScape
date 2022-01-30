﻿using UnityEngine;
using UnityEngine.PostProcessing;
using FYFY;
using FYFY_plugins.Monitoring;
using UnityEngine.UI;

public class LogoDisplaying : FSystem {

    // This system manage displaying of the story

    private Family f_fadingMenuElems = FamilyManager.getFamily(new AllOfComponents(typeof(FadingMenu)));

    public GameObject logoGo;
    public Image fadingImage;
    public Image background;

    public ImgBank logos;

    private int fadeSpeed = 3;

    private float readingTimer = -Mathf.Infinity;
    private int currentLogo = -1;
    private int nextLogo = 0;
    private bool showLogo = false;
    private bool closeLogo = false;

    public GameObject loadingFragment;
    public Renderer lfMat;
    private Vector3 targetEmissionColor;
    public Vector3 currentColor;
    private bool menuFaded = false;
    private bool motionBlurWasOn;

    public static LogoDisplaying instance;

    public LogoDisplaying()
    {
        instance = this;
    }

    protected override void onStart()
    {
        targetEmissionColor = Random.insideUnitSphere * 155 + Vector3.one * 100;
    }

    protected override void onPause(int currentFrame)
    {
        GameObjectManager.setGameObjectState(logoGo, false);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (MonitoringManager.Instance.inGameAnalysis && MonitoringManager.Instance.waitingForLaalys && !HelpSystem.shouldPause)
        {
            if(!motionBlurWasOn)
                motionBlurWasOn = loadingFragment.transform.parent.GetComponent<PostProcessingBehaviour>().profile.motionBlur.enabled;
            loadingFragment.transform.parent.GetComponent<PostProcessingBehaviour>().profile.motionBlur.enabled = false;
            //Loading animation
            loadingFragment.transform.Rotate(0, 2, 0);
            if (currentColor != targetEmissionColor)
                currentColor = Vector3.MoveTowards(currentColor, targetEmissionColor, 20);
            else
                targetEmissionColor = Random.insideUnitSphere * 155 + Vector3.one * 100;
            lfMat.material.SetColor("_EmissionColor", new Color(currentColor.x / 256, currentColor.y / 256, currentColor.z / 256) * Mathf.LinearToGammaSpace(2));
        }
        else
        {
            if (logoGo.transform.GetChild(2).gameObject.activeSelf)
            {
                GameObjectManager.setGameObjectState(loadingFragment, false);
                GameObjectManager.setGameObjectState(logoGo.transform.GetChild(2).gameObject, false);
                if(motionBlurWasOn)
                    loadingFragment.transform.parent.GetComponent<PostProcessingBehaviour>().profile.motionBlur.enabled = true;
            }
            // switch to next logo if mouse clicked or Escape pressed
            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
                // change to next logo
                nextLogo++;

            if (nextLogo != currentLogo)
            {
                // make logo transparent (usefull in case of clicking)
                fadingImage.color = new Color(1, 1, 1, 0);
                // check if logo remaining
                if (nextLogo < logos.bank.Length)
                {
                    currentLogo = nextLogo;
                    fadingImage.sprite = logos.bank[currentLogo];
                    fadingImage.preserveAspect = true;
                    showLogo = true;
                    readingTimer = Time.time;
                }
                else
                {
                    if (!closeLogo)
                    {
                        int nbFadinElems = f_fadingMenuElems.Count;
                        for (int i = 0; i < nbFadinElems; i++)
                            GameObjectManager.setGameObjectState(f_fadingMenuElems.getAt(i), true);
                        closeLogo = true;
                        readingTimer = Time.time;
                        // Start main menu
                        MenuSystem.instance.Pause = false;
                    }
                }

            }

            if (!closeLogo)
            {
                if (showLogo) // alpha to plain
                {
                    if (Time.time - readingTimer < fadeSpeed)
                        // fade progress
                        fadingImage.color = new Color(1, 1, 1, (Time.time - readingTimer) / fadeSpeed);
                    else
                    {
                        // fade ends
                        fadingImage.color = new Color(1, 1, 1, 1);
                        showLogo = false;
                        readingTimer = Time.time;
                    }
                }
                else // plain to alpha
                {
                    if (Time.time - readingTimer < fadeSpeed)
                        fadingImage.color = new Color(1, 1, 1, 1 - (Time.time - readingTimer) / fadeSpeed);
                    else
                    {
                        fadingImage.color = new Color(1, 1, 1, 0);
                        showLogo = true;
                        readingTimer = Time.time;
                        nextLogo++;
                    }
                }
            }
            else
            {
                float alpha = 0f;
                if (Time.time - readingTimer < fadeSpeed)
                {
                    alpha = 1f;
                    if (!menuFaded)
                    {
                        alpha = (Time.time - readingTimer) / fadeSpeed;
                        background.color = new Color(0, 0, 0, 1-alpha);
                    }
                }
                else // fade end
                {
                    alpha = 1f;
                    if(menuFaded)
                        // Pause this
                        this.Pause = true;
                    else
                    {
                        menuFaded = true;
                        // Disable Logo
                        GameObjectManager.setGameObjectState(logoGo, false);
                        readingTimer = Time.time;
                    }
                }
                GameObject tmpGO = null;
                int nbFadinElems = f_fadingMenuElems.Count;
                for (int i = 0; i < nbFadinElems; i++)
                {
                    tmpGO = f_fadingMenuElems.getAt(i);
                    if (tmpGO.GetComponent<Image>())
                        tmpGO.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                    else if (tmpGO.GetComponent<Button>())
                    {
                        ColorBlock cb = tmpGO.GetComponent<Button>().colors;
                        cb.normalColor = new Color(cb.normalColor.r, cb.normalColor.g, cb.normalColor.b, alpha);
                        cb.highlightedColor = new Color(cb.highlightedColor.r, cb.highlightedColor.g, cb.highlightedColor.b, alpha);
                        cb.pressedColor = new Color(cb.pressedColor.r, cb.pressedColor.g, cb.pressedColor.b, alpha);
                        cb.selectedColor = new Color(cb.selectedColor.r, cb.selectedColor.g, cb.selectedColor.b, alpha);
                        cb.disabledColor = new Color(cb.disabledColor.r, cb.disabledColor.g, cb.disabledColor.b, alpha/2);
                        tmpGO.GetComponent<Button>().colors = cb;
                    }
                    else if (tmpGO.GetComponent<TMPro.TMP_Text>())
                        tmpGO.GetComponent<TMPro.TMP_Text>().color = new Color(1, 1, 1, alpha);
                }
            }
        }
    }
}