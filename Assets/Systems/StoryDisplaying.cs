using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StoryDisplaying : FSystem {

    // This system manage displaying of the story

    private Family f_storyDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    // Camera is required in this system to switch menuCamera to fpsCamera during displaying story
    private Family menuCamera = FamilyManager.getFamily(new AllOfComponents(typeof(MenuCamera), typeof(Camera)));

    private GameObject sdGo;
    private TextMeshProUGUI sdText;
    private Image fadingImage;
    private Image background;
    private GameObject clickFeedback;

    // Contains all texts of the story
    private List<List<string>> storyTexts;
    // Contains current texts of the story
    private string[] readTexts;

    StoryText st;

    private int fadeSpeed = 1;

    private float readingTimer = -Mathf.Infinity;
    private int textCount = -1;
    private bool plainToAlpha = false;
    private bool alphaToPlain = false;
    private bool fadingBackground = false;

    public static StoryDisplaying instance;

    public StoryDisplaying()
    {
        if (Application.isPlaying)
        {
            sdGo = f_storyDisplayer.First();
            foreach (Transform child in sdGo.transform)
            {
                if (child.gameObject.name == "Background")
                    background = child.gameObject.GetComponent<Image>();
                else if (child.gameObject.name == "Text")
                    sdText = child.gameObject.GetComponent<TextMeshProUGUI>();
                else if (child.gameObject.name == "FadingImage")
                    fadingImage = child.gameObject.GetComponent<Image>();
                else if (child.gameObject.name == "Click")
                    clickFeedback = child.gameObject;
            }

            st = sdGo.GetComponent<StoryText>();

            storyTexts = new List<List<string>>();
            storyTexts.Add(new List<string>(st.intro));
            storyTexts.Add(new List<string>(st.transition));
            storyTexts.Add(new List<string>(st.end));

            instance = this;
        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
        // Disable UI story
        GameObjectManager.setGameObjectState(f_storyDisplayer.First(), false);
    }

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame)
    {
        // Stop all systems except this
        foreach (FSystem syst in FSystemManager.updateSystems())
            if (syst != this)
                syst.Pause = true;
        // Enable UI Story
        GameObjectManager.setGameObjectState(f_storyDisplayer.First(), true);
        // Get current set of texts
        readTexts = storyTexts[st.storyProgression].ToArray();
        // Set first fading
        readingTimer = Time.time;
        alphaToPlain = true;
        fadingBackground = true;
        // Set current text
        sdText.text = "";
        textCount = -1;
        // define color fading
        if (st.storyProgression < storyTexts.Count - 1)
        {
            fadingImage.color = Color.black;
            background.color = Color.black;
            sdText.color = Color.white;
        }
        else
        {
            fadingImage.color = Color.white;
            background.color = Color.white;
            sdText.color = Color.black;
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (alphaToPlain)
        {
            if (Time.time - readingTimer < fadeSpeed)
            {
                // fade progress
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, (Time.time - readingTimer) / fadeSpeed);
                // fade background if required
                if (fadingBackground)
                    background.color = new Color(background.color.r, background.color.g, background.color.b, (Time.time - readingTimer) / fadeSpeed);
                // stop fading if mouse clicked
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
                    readingTimer = Time.time - (fadeSpeed + 1);
            }
            else
            {
                // fade ends
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 1);
                if (fadingBackground)
                {
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 1);
                    fadingBackground = false;
                    // stop systems
                    MovingSystem.instance.Pause = true;
                }
                alphaToPlain = false;
                // pass to the next text
                textCount++;
                if (textCount < readTexts.Length)
                    sdText.text = readTexts[textCount];
                else
                {
                    // end text reached
                    if (st.storyProgression < storyTexts.Count - 1)
                    {
                        sdText.text = "";
                        fadingBackground = true;
                        // Enable fps camera (=> disable menuCamera)
                        GameObjectManager.setGameObjectState(menuCamera.First(), false);
                        // Start all required systems
                        MovingSystem.instance.Pause = false;
                        SpritesAnimator.instance.Pause = false;
                        DreamFragmentCollecting.instance.Pause = false;
                        IARNewItemAvailable.instance.Pause = false;
                        IARTabNavigation.instance.Pause = false;
                        Highlighter.instance.Pause = false;
                        MirrorSystem.instance.Pause = false;
                        ToggleObject.instance.Pause = false;
                        CollectObject.instance.Pause = false;
                        MoveInFrontOf.instance.Pause = false;
                        UIEffectPlayer.instance.Pause = false;
                    }
                    else
                        GameObjectManager.loadScene(SceneManager.GetActiveScene().name); // reset game
                }
                readingTimer = Time.time;
                plainToAlpha = true;
            }
        }
        else if (plainToAlpha)
        {
            if (Time.time - readingTimer < fadeSpeed)
            {
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 1 - (Time.time - readingTimer) / fadeSpeed);
                if (fadingBackground)
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 1 - (Time.time - readingTimer) / fadeSpeed);
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
                    readingTimer = Time.time - (fadeSpeed + 1);
            }
            else
            {
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 0);
                plainToAlpha = false;
                // Displaying text
                GameObjectManager.setGameObjectState(clickFeedback, true);
                if (fadingBackground)
                {
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
                    fadingBackground = false;
                    this.Pause = true; // Stop this system
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Escape))
            {
                alphaToPlain = true;
                readingTimer = Time.time;
            }
        }
    }
}