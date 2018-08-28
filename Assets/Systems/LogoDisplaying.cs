using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

public class LogoDisplaying : FSystem {

    // This system manage displaying of the story

    private Family f_logo = FamilyManager.getFamily(new AllOfComponents(typeof(ImgBank)));

    private GameObject logoGo;
    private Image fadingImage;
    private Image background;

    // Contains all texts of the story
    private List<List<string>> storyTexts;
    // Contains current texts of the story
    private string[] readTexts;

    ImgBank logo;

    private int fadeSpeed = 3;

    private float readingTimer = -Mathf.Infinity;
    private int currentLogo = -1;
    private int nextLogo = 0;
    private bool showLogo = false;
    private bool closeLogo = false;

    public LogoDisplaying()
    {
        if (Application.isPlaying)
        {
            logoGo = f_logo.First();
            foreach (Transform child in logoGo.transform)
            {
                if (child.gameObject.name == "Background")
                    background = child.gameObject.GetComponent<Image>();
                else if (child.gameObject.name == "FadingImage")
                    fadingImage = child.gameObject.GetComponent<Image>();
            }

            logo = logoGo.GetComponent<ImgBank>();
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        // switch to next logo if mouse clicked or Escape pressed
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
            // change to next logo
            nextLogo++;

        if (nextLogo != currentLogo)
        {
            // make logo transparent (usefull in case of clicking)
            fadingImage.color = new Color(1, 1, 1, 0);
            // check if logo remaining
            if (nextLogo >= logo.bank.Length && !closeLogo)
            {
                closeLogo = true;
                readingTimer = Time.time;
            }
            else
            {
                currentLogo = nextLogo;
                fadingImage.sprite = logo.bank[currentLogo];
                fadingImage.preserveAspect = true;
                showLogo = true;
                readingTimer = Time.time;
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
            if (Time.time - readingTimer < fadeSpeed)
            {
                background.color = new Color(0, 0, 0, 1 - (Time.time - readingTimer) / fadeSpeed);
            }
            else // fade end
            {
                // Start main menu
                MenuSystem.instance.Pause = false;
                // Pause this
                this.Pause = true;
                // Disable Logo
                GameObjectManager.setGameObjectState(logoGo, false);
            }
        }
    }
}