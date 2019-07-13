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

    private Family f_storyDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));
    private Family f_game = FamilyManager.getFamily(new AnyOfTags("GameRooms"));

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

    private Timer timer;

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
            List<string> epilog = new List<string>(st.end);
            epilog.AddRange(st.credit);
            storyTexts.Add(epilog);

            GameObjectManager.addComponent<Timer>(f_game.First());

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
        // Stop all systems except this, ActionManager, SendStatements and HelpSystem
        List<FSystem> allSystems = new List<FSystem>(FSystemManager.fixedUpdateSystems());
        allSystems.AddRange(FSystemManager.updateSystems());
        allSystems.AddRange(FSystemManager.lateUpdateSystems());
        foreach (FSystem syst in allSystems)
            if (syst != this && syst != ActionsManager.instance && syst != SendStatements.instance && syst != HelpSystem.instance)
                syst.Pause = true;
        // Enable UI Story
        GameObjectManager.setGameObjectState(f_storyDisplayer.First(), true);
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
            if (st.storyProgression == 0)
            {
                timer = f_game.First().GetComponent<Timer>();
                timer.startingTime = Time.time;

                GameObjectManager.addComponent<ActionPerformedForLRS>(sdGo, new
                {
                    verb = "started",
                    objectType = "serious-game",
                    objectName = "E-LearningScape",
                    activityExtensions = new Dictionary<string, List<string>>() {
                    { "content", new List<string>() { LoadGameContent.gameContent.theme } }
                }
                });
            }
            fadingImage.color = Color.black;
            background.color = Color.black;
        }
        else
        {
            float duration = Time.time - timer.startingTime;
            int hours = (int)duration / 3600;
            int minutes = (int)(duration % 3600) / 60;
            int seconds = (int)(duration % 3600) % 60;
            storyTexts[st.storyProgression].Add(string.Concat("<align=\"center\">", LoadGameContent.gameContent.scoreText, Environment.NewLine, hours.ToString("D2"), ":", minutes.ToString("D2"), ":", seconds.ToString("D2")));
            GameObjectManager.addComponent<ActionPerformedForLRS>(sdGo, new
            {
                verb = "completed",
                objectType = "serious-game",
                objectName = "E-LearningScape",
                activityExtensions = new Dictionary<string, List<string>>() {
                    { "content", new List<string>() { LoadGameContent.gameContent.theme } },
                    { "time", new List<string>() { string.Concat(hours.ToString("D2"), ":", minutes.ToString("D2"), ":", seconds.ToString("D2")) } }
                }
            });
            fadingImage.color = Color.white;
            background.color = Color.white;
        }
        // Get current set of texts
        readTexts = storyTexts[st.storyProgression].ToArray();
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
                {
                    readingTimer = Time.time - (fadeSpeed + 1);
                    GameObjectManager.addComponent<ActionPerformedForLRS>(fadingImage.gameObject, new { verb = "skipped", objectType = "animation", objectName = fadingImage.gameObject.name });
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
                        IARNewHintAvailable.instance.Pause = false;
                        Highlighter.instance.Pause = false;
                        MirrorSystem.instance.Pause = false;
                        ToggleObject.instance.Pause = false;
                        CollectObject.instance.Pause = false;
                        MoveInFrontOf.instance.Pause = false;
                        UIEffectPlayer.instance.Pause = false;
                        ActionsManager.instance.Pause = !LoadGameContent.gameContent.trace;
                        HelpSystem.instance.Pause = !LoadGameContent.gameContent.helpSystem;
                        SendStatements.instance.Pause = !LoadGameContent.gameContent.traceToLRS;
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
                {
                    readingTimer = Time.time - (fadeSpeed + 1);
                    GameObjectManager.addComponent<ActionPerformedForLRS>(fadingImage.gameObject, new { verb = "skipped", objectType = "animation", objectName = fadingImage.gameObject.name });
                }
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
                    //Enable IARSystem (done after the others to prevent a bug)
                    IARTabNavigation.instance.Pause = false;
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
                GameObjectManager.addComponent<ActionPerformedForLRS>(sdGo, new
                {
                    verb = "read",
                    objectType = "text",
                    objectName = sdGo.name,
                    activityExtensions = new Dictionary<string, List<string>>() { { "content", new List<string>() { sdText.text } } }
                });
            }
        }
    }
}