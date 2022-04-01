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

    public TextMeshProUGUI sdText; // the text that contains text story
    public Image fadingImage; // used to hide the current text of the story
    public Image background; // used to hide scene
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
    private bool fadingBackground = false; //used to hide scene

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

        // if there is no survey link, disable associated button
        if (storyText.endLink == "")
            GameObjectManager.setGameObjectState(endText.transform.Find("OpenLink").gameObject, false);

        // if there is no explanation, disable associated text
        if (LoadGameContent.gameContent.endExplainationText == "")
            GameObjectManager.setGameObjectState(endText.transform.Find("TextContent").gameObject, false);

        // if there is no debriefing link, disable associated button
        if (LoadGameContent.gameContent.debriefingLink == "")
            GameObjectManager.setGameObjectState(endText.transform.Find("WatchDebriefing").gameObject, false);
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

    public void OpenDebriefingLink()
    {
        if (LoadGameContent.gameContent.debriefingLink != "")
        {
            try
            {
                Application.OpenURL(LoadGameContent.gameContent.debriefingLink);
            }
            catch (Exception)
            {
                Debug.LogError(string.Concat("Invalid debriefing link: ", LoadGameContent.gameContent.debriefingLink));
            }
            GameObjectManager.addComponent<ActionPerformedForLRS>(endText, new
            {
                verb = "accessed",
                objectType = "link",
                activityExtensions = new Dictionary<string, string>() {
                    { "value", "debriefing link" },
                    { "link", LoadGameContent.gameContent.debriefingLink }
                }
            });
        }
    }

    // If there are buttons, don't fade and reset the game unless the button to leave is cliked
    public void ResetGame()
    {
        // start the fading process, game will restart at the end of the fading process => see LoadScene in onProcess
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
        // Set first fading (including background to hide scene)
        readingTimer = Time.time;
        alphaToPlain = true;
        fadingBackground = true;
        // Set current text
        sdText.text = "";
        textCount = -1;
        // define color fading
        if (storyText.storyProgression < storyTexts.Count - 1)
        {
            // this is not the last story texts => use black color
            fadingImage.color = Color.black;
            background.color = Color.black;
            // check if we start the story
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
        }
        else
        {
            // this is the last story texts => use white color
            fadingImage.color = Color.white;
            background.color = Color.white;
            // compute timer
            float d = Time.time - timer.startingTime;
            int hours = (int)d / 3600;
            int minutes = (int)(d % 3600) / 60;
            int seconds = (int)(d % 3600) % 60;
            // display timer indise the story text
            storyTexts[storyText.storyProgression].Add(string.Concat("<align=\"center\">", LoadGameContent.gameContent.scoreText, Environment.NewLine, hours.ToString("D2"), ":", minutes.ToString("D2"), ":", seconds.ToString("D2")));
            // hide in game timer
            GameObjectManager.setGameObjectState(Chronometer, false);
            // trace that the game is over
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
        }
        // Get current set of texts
        readTexts = storyTexts[storyText.storyProgression].ToArray();
        // Disable HUD
        GameObjectManager.setGameObjectState(mainHUD, false);
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        // check if we have to hide current view => make masks not transparent
        if (alphaToPlain)
        {
            // check if we have to continue to fade
            if (Time.time - readingTimer < fadeSpeed)
            {
                // fade background? if true fade the background to hide scene
                if (fadingBackground)
                    background.color = new Color(background.color.r, background.color.g, background.color.b, (Time.time - readingTimer) / fadeSpeed);
                // hide current text of the story
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, (Time.time - readingTimer) / fadeSpeed);
                // stop fading if mouse clicked
                if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
                    readingTimer = Time.time - (fadeSpeed + 1);
            }
            else
            {
                // fade ends => now scene and text are hidden
                alphaToPlain = false;
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 1);
                // check if we controled background for this fading
                if (fadingBackground)
                {
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 1);
                    fadingBackground = false; // do not control background for next fading
                    movingModeSelector.pauseMovingSystems(); // stop Moving systems to avoid to move while the scene is not visible
                }
                // pass to the next text
                textCount++;
                // check if we have a new text to display
                if (textCount < readTexts.Length)
                {
                    // display the new text
                    sdText.text = readTexts[textCount];
                    // check if this text is the last one => if true enable end panel
                    if(storyText.storyProgression == storyTexts.Count - 1 && textCount == readTexts.Length - 1)
                    {
                        // enable endText when last text is reached
                        GameObjectManager.setGameObjectState(endText, true);
                        // if explanation is defined, move story text at the top of the screen
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
                    // check if we are not at the end of the story
                    if (storyText.storyProgression < storyTexts.Count - 1)
                    {
                        sdText.text = "";
                        fadingBackground = true; // next fading will require to fade background
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
                        // we reached the end of the game => reset game
                        GameObjectManager.loadScene(SceneManager.GetActiveScene().name);
                }
                // Text was changed, now ask to hide mask to make text visible 
                plainToAlpha = true;
                readingTimer = Time.time;
                // Hide click notification
                GameObjectManager.setGameObjectState(clickFeedback, false);
            }
        }
        // check if we have to hide masks => make current view visible
        else if (plainToAlpha)
        {
            // check if we have to continue to fade
            if (Time.time - readingTimer < fadeSpeed)
            {
                // fade background? if true fade the background to make scene visible
                if (fadingBackground)
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 1 - (Time.time - readingTimer) / fadeSpeed);
                // hide mask to make current text of the story visible
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 1 - (Time.time - readingTimer) / fadeSpeed);
                // stop fading if mouse clicked
                if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
                    readingTimer = Time.time - (fadeSpeed + 1);
            }
            else
            {
                // fade ends => now scene or text are visible (depending on background fading)
                plainToAlpha = false;
                fadingImage.color = new Color(fadingImage.color.r, fadingImage.color.g, fadingImage.color.b, 0);
                // check if we controled background for this fading
                if (fadingBackground)
                {
                    background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
                    fadingBackground = false; // do not control background for next fading
                    //Enable IARSystem (done after the others to prevent a bug)
                    IARTabNavigation.instance.Pause = false;
                    this.Pause = true; // Stop this system
                    // Enable HUD
                    GameObjectManager.setGameObjectState(mainHUD, true);
                    // Hide click notification
                    GameObjectManager.setGameObjectState(clickFeedback, false);
                } else
                    // Displaying click notification if it is not the end text
                    GameObjectManager.setGameObjectState(clickFeedback, !(storyText.storyProgression == storyTexts.Count - 1 && textCount == readTexts.Length - 1));
            }
        }
        else
        {
            // fades are over, story text of the story is visible, waiting to click to pass to the next step (except if we are at the end of the game)
            if ((Input.GetButtonDown ("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit")) && !(storyText.storyProgression == storyTexts.Count - 1 && textCount == readTexts.Length - 1))
            {
                alphaToPlain = true;
                readingTimer = Time.time;
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