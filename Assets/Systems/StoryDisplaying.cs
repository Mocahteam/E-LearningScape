using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class StoryDisplaying : FSystem {

    // This system manage displaying of the story

    // Camera is required in this system to switch menuCamera to fpsCamera during displaying story
    public GameObject menuCamera;

    public MovingModeSelector movingModeSelector;

    public GameObject mainHUD;

    public TextMeshProUGUI sdText;
    public Image fadingImage;
    public Image background;
    public GameObject clickFeedback;
    public GameObject endText;

    // Contains all texts of the story
    private List<List<string>> storyTexts;
    // Contains current texts of the story
    private string[] readTexts;

    public StoryText storyText;

    private int fadeSpeed = 1;

    private float readingTimer = -Mathf.Infinity;
    private int textCount = -1;
    private bool plainToAlpha = false;
    private bool alphaToPlain = false;
    private bool fadingBackground = false;

    public Timer timer;

    public GameObject Chronometer;

    public static StoryDisplaying instance;

    private RectTransform tmpRT;

    public float Duration { get => Time.time - timer.startingTime; }
    public List<List<string>> StoryTexts { get => storyTexts; }

    public StoryDisplaying()
    {
        instance = this;
    }

    protected override void onStart()
    {
        storyTexts = new List<List<string>>();
        storyTexts.Add(new List<string>(storyText.intro));
        storyTexts.Add(new List<string>(storyText.transition));
        storyTexts.Add(new List<string>(storyText.end));

        // if there is no link, disable link button
        if (storyText.endLink == "")
        {
            fadingImage.transform.SetAsLastSibling();
            foreach (Transform child in endText.transform)
                if (child.GetComponent<Button>())
                    GameObjectManager.setGameObjectState(child.gameObject, false);
        }
    }

    public void OpenEndLink()
    {
        if(storyText.endLink != "")
        {
            try
            {
                Application.OpenURL(storyText.endLink);
            }
            catch (Exception)
            {
                Debug.LogError(string.Concat("Invalid end link: ", storyText.endLink));
            }
            GameObjectManager.addComponent<ActionPerformedForLRS>(endText, new
            {
                verb = "accessed",
                objectType = "link",
                activityExtensions = new Dictionary<string, string>() {
                    { "value", "end link" },
                    { "link", storyText.endLink }
                }
            });
        }
    }

    // If there are buttons, don't fade and reset the game unless the button to leave is cliked
    public void ResetGame()
    {
        fadingImage.transform.SetAsLastSibling();
        alphaToPlain = true;
        readingTimer = Time.time;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
        // Disable UI story
        GameObjectManager.setGameObjectState(storyText.gameObject, false);
    }

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame)
    {
        // Stop all systems except this, ActionManager, SendStatements and HelpSystem
        List<FSystem> allSystems = new List<FSystem>(FSystemManager.fixedUpdateSystems());
        allSystems.AddRange(FSystemManager.updateSystems());
        allSystems.AddRange(FSystemManager.lateUpdateSystems());
        foreach (FSystem syst in allSystems)
            if (syst != this && syst != ActionsManager.instance && syst != SendStatements.instance && syst != HelpSystem.instance)
                syst.Pause = true;
        // Enable UI Story
        GameObjectManager.setGameObjectState(storyText.gameObject, true);
        // Set first fading
        readingTimer = Time.time;
        alphaToPlain = true;
        fadingBackground = true;
        // Set current text
        sdText.text = "";
        textCount = -1;
        // define color fading
        if (storyText.storyProgression < storyTexts.Count - 1)
        {
            if (storyText.storyProgression == 0 && storyTexts[0].Count > 0)
            {
                timer.startingTime = Time.time;

                GameObjectManager.addComponent<ActionPerformedForLRS>(storyText.gameObject, new
                {
                    verb = "started",
                    objectType = "serious-game",
                    activityExtensions = new Dictionary<string, string>() {
                        { "value", "E-LearningScape" },
                        { "content", LoadGameContent.gameContent.theme },
                        { "help", LoadGameContent.internalGameContent.helpSystem.ToString() }
                    }
                });
            }
            fadingImage.color = Color.black;
            background.color = Color.black;
        }
        else
        {
            float d = Time.time - timer.startingTime;
            int hours = (int)d / 3600;
            int minutes = (int)(d % 3600) / 60;
            int seconds = (int)(d % 3600) % 60;
            storyTexts[storyText.storyProgression].Add(string.Concat("<align=\"center\">", LoadGameContent.gameContent.scoreText, Environment.NewLine, hours.ToString("D2"), ":", minutes.ToString("D2"), ":", seconds.ToString("D2")));
            GameObjectManager.addComponent<ActionPerformedForLRS>(storyText.gameObject, new
            {
                verb = "completed",
                objectType = "serious-game",
                activityExtensions = new Dictionary<string, string>() {
                    { "value", "E-LearningScape" },
                    { "content", LoadGameContent.gameContent.theme },
                    { "time", string.Concat(hours.ToString("D2"), ":", minutes.ToString("D2"), ":", seconds.ToString("D2")) }
                }
            });
            GameObjectManager.setGameObjectState(Chronometer, false);
            fadingImage.color = Color.white;
            background.color = Color.white;
        }
        // Get current set of texts
        readTexts = storyTexts[storyText.storyProgression].ToArray();
        // Disable HUD
        GameObjectManager.setGameObjectState(mainHUD, false);
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
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
                if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
                {
                    readingTimer = Time.time - (fadeSpeed + 1);
                }
            }
            else
            {
                // fade ends
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 1);
                if (fadingBackground)
                {
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 1);
                    fadingBackground = false;
                    // stop Moving systems
                    movingModeSelector.pauseMovingSystems();
                }
                alphaToPlain = false;
                // pass to the next text
                textCount++;
                if (textCount < readTexts.Length)
                {
                    sdText.text = readTexts[textCount];
                    if(storyText.storyProgression == storyTexts.Count - 1 && textCount == readTexts.Length - 1)
                    {
                        // enable endText when last text is reached
                        GameObjectManager.setGameObjectState(endText, true);
                        if(LoadGameContent.gameContent.endExplainationText != "")
                        {
                            tmpRT = sdText.GetComponent<RectTransform>();
                            tmpRT.anchoredPosition = new Vector2(tmpRT.anchoredPosition.x, -55);
                            tmpRT.sizeDelta = new Vector2(tmpRT.sizeDelta.x, 80);
                        }
                    }
                }
                else
                {
                    // end text reached
                    if (storyText.storyProgression < storyTexts.Count - 1)
                    {
                        sdText.text = "";
                        fadingBackground = true;
                        // Enable fps camera (=> disable menuCamera)
                        GameObjectManager.setGameObjectState(menuCamera, false);
                        // Start all required systems
                        movingModeSelector.resumeMovingSystems();
                        SpritesAnimator.instance.Pause = false;
                        DreamFragmentCollecting.instance.Pause = false;
                        IARNewItemAvailable.instance.Pause = false;
                        Highlighter.instance.Pause = false;
                        MirrorSystem.instance.Pause = false;
                        ToggleObject.instance.Pause = false;
                        CollectObject.instance.Pause = false;
                        MoveInFrontOf.instance.Pause = false;
                        UIEffectPlayer.instance.Pause = false;
                        ActionsManager.instance.Pause = !LoadGameContent.internalGameContent.trace;
                        HelpSystem.instance.Pause = HelpSystem.shouldPause;
                        SendStatements.instance.Pause = !LoadGameContent.internalGameContent.traceToLRS;
                        SaveManager.instance.Pause = false;
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
                if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
                {
                    readingTimer = Time.time - (fadeSpeed + 1);
                }
            }
            else
            {
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 0);
                plainToAlpha = false;
                // Displaying text if it is not the end text or there is no end link
                GameObjectManager.setGameObjectState(clickFeedback, !(storyText.storyProgression == storyTexts.Count - 1 && textCount == readTexts.Length - 1 && storyText.endLink != ""));
                if (fadingBackground)
                {
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
                    fadingBackground = false;
                    //Enable IARSystem (done after the others to prevent a bug)
                    IARTabNavigation.instance.Pause = false;
                    this.Pause = true; // Stop this system

                    // Enable HUD
                    GameObjectManager.setGameObjectState(mainHUD, true);
                }
            }
        }
        else
        {
            if (Input.GetButtonDown ("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
            {
                // if it is not the end text or there is no end link
                if (!(storyText.storyProgression == storyTexts.Count - 1 && textCount == readTexts.Length - 1 && storyText.endLink != ""))
                {
                    alphaToPlain = true;
                    readingTimer = Time.time;
                }
            }
        }
    }

    public void LoadStoryProgression(int storyProgressionCount)
    {
        storyText.storyProgression = storyProgressionCount;
        // start without reading text
        storyTexts[storyProgressionCount].Clear();
    }

    public int GetStoryProgression()
    {
        return storyText.storyProgression;
    }
}