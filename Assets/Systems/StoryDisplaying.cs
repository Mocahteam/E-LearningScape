using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;

public class StoryDisplaying : FSystem {

    private Family storyDisplayer = FamilyManager.getFamily(new AnyOfTags("StoryDisplayer"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family storyText = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));
    private Family game = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family buttons = FamilyManager.getFamily(new AnyOfTags("MainMenuButton"));
    private Family cameras = FamilyManager.getFamily(new AllOfComponents(typeof(Camera)));

    private Image background;
    private TextMeshProUGUI sdText;
    private Image fadingImage;
    private GameObject clickFeedback;
    private GameObject endScreen;

    public static bool readingIntro = false;
    public static bool readingTransition = false;
    public static bool readingEnding = false;
    public static bool reading = false;

    private string[] readTexts;
    private float readingTimer = -Mathf.Infinity;
    private int textCount = 0;
    private bool fadingIn = false;
    private bool fadingOut = false;
    private bool fadingToReadingMode = false;
    private bool fadingOutOfReadingMode = false;
    private bool end = false;

    private string[] introText;
    private string[] transitionText;
    private string[] endingText;

    private GameObject menuCamera;

    public StoryDisplaying()
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in storyDisplayer.First().transform)
            {
                if (child.gameObject.name == "Background")
                {
                    background = child.gameObject.GetComponent<Image>();
                }
                else if (child.gameObject.name == "Text")
                {
                    sdText = child.gameObject.GetComponent<TextMeshProUGUI>();
                }
                else if (child.gameObject.name == "FadingImage")
                {
                    fadingImage = child.gameObject.GetComponent<Image>();
                }
                else if (child.gameObject.name == "Click")
                {
                    clickFeedback = child.gameObject;
                }
                else if(child.gameObject.name == "EndScreen")
                {
                    endScreen = child.gameObject;
                }
            }
            introText = storyText.First().GetComponent<StoryText>().intro;
            transitionText = storyText.First().GetComponent<StoryText>().transition;
            endingText = storyText.First().GetComponent<StoryText>().end;
            int nb = cameras.Count;
            for(int i = 0; i < nb; i++)
            {
                if(cameras.getAt(i).name == "MenuCamera")
                {
                    menuCamera = cameras.getAt(i);
                    break;
                }
            }
        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if(readingIntro || readingTransition || readingEnding)
        {
            reading = true;
            if(readTexts == null)
            {
                GameObjectManager.setGameObjectState(storyDisplayer.First(),true);
                player.First().GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = false;
                if (readingIntro)
                {
                    readTexts = introText;
                    readingTimer = Time.time;
                    fadingToReadingMode = true;
                    fadingImage.color = Color.black;
                    background.color = Color.black;
                    sdText.text = "";
                }
                else if (readingTransition)
                {
                    readTexts = transitionText;
                    readingTimer = Time.time;
                    fadingToReadingMode = true;
                    fadingImage.color = Color.black;
                    background.color = Color.black;
                    sdText.text = "";
                }
                else if (readingEnding)
                {
                    readTexts = endingText;
                    readingTimer = Time.time;
                    fadingToReadingMode = true;
                    fadingImage.color = Color.white - Color.black;
                    background.color = Color.white - Color.black;
                    sdText.color = Color.black;
                    sdText.text = "";
                }
            }
            
            if (fadingIn)
            {
                if (Time.time - readingTimer < 2)
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, 1 - (Time.time - readingTimer) / 2);
                    if (Input.GetMouseButtonDown(0))
                    {
                        readingTimer = Time.time - 3;
                    }
                }
                else
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, 0);
                    fadingIn = false;
                    GameObjectManager.setGameObjectState(clickFeedback,true);
                }
            }
            else if (fadingOut)
            {
                if (Time.time - readingTimer < 2)
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, (Time.time - readingTimer) / 2);
                    if (Input.GetMouseButtonDown(0))
                    {
                        readingTimer = Time.time - 3;
                    }
                }
                else
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, 1);
                    textCount++;
                    fadingOut = false;
                    if(textCount < readTexts.Length)
                    {
                        sdText.text = readTexts[textCount];
                        fadingIn = true;
                        readingTimer = Time.time;
                    }
                    else
                    {
                        if (readingEnding)
                        {
                            GameObjectManager.setGameObjectState(sdText.gameObject, false);
                            fadingIn = true;
                            readingTimer = Time.time;
                            end = true;
                            GameObjectManager.setGameObjectState(endScreen, true);
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.lockState = CursorLockMode.Confined;
                            Cursor.visible = true;
                        }
                        else
                        {
                            sdText.text = "";
                            fadingOutOfReadingMode = true;
                            readingTimer = Time.time;
                        }
                    }
                }
            }
            else if (fadingToReadingMode)
            {
                if (readingIntro)
                {
                    if (MenuSystem.onGame)
                    {
                        readingTimer = Time.time - 4;
                    }
                }
                if (Time.time - readingTimer < 4)
                {
                    Color c = fadingImage.color;
                    if (readingEnding)
                    {
                        fadingImage.color = Color.white;
                        if(Time.time - readingTimer < 2)
                        {
                            readingTimer = Time.time - 2;
                        }
                    }
                    else
                    {
                        fadingImage.color = new Color(c.r, c.g, c.b, (Time.time - readingTimer) / 4);
                    }
                    c = background.color;
                    background.color = new Color(c.r, c.g, c.b, (Time.time - readingTimer) / 4);
                }
                else
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, 1);
                    c = background.color;
                    background.color = new Color(c.r, c.g, c.b, 1);
                    fadingToReadingMode = false;
                    if (readingIntro)
                    {
                        RenderSettings.fogDensity = 0.03f;
                        foreach (FSystem s in FSystemManager.fixedUpdateSystems())
                        {
                            s.Pause = s.ToString() == "MenuSystem";
                        }
                        foreach (FSystem s in FSystemManager.updateSystems())
                        {
                            s.Pause = s.ToString() == "MenuSystem";
                        }
                        foreach (FSystem s in FSystemManager.lateUpdateSystems())
                        {
                            s.Pause = s.ToString() == "MenuSystem";
                        }
                        foreach (Transform room in game.First().transform)
                        {
                            if (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString()))
                            {
                                GameObjectManager.setGameObjectState(room.gameObject, false);
                            }
                        }
                        foreach (GameObject go in ui)
                        {
                            if (go.name == "Cursor")
                            {
                                GameObjectManager.setGameObjectState(go, true);
                                break;
                            }
                        }
                        GameObjectManager.setGameObjectState(menuCamera, false);
                        foreach (Camera camera in player.First().GetComponentsInChildren<Camera>())
                        {
                            if (camera.gameObject.name == "FirstPersonCharacter")
                            {
                                camera.gameObject.tag = "MainCamera";
                                break;
                            }
                        }
                        GameObjectManager.setGameObjectState(buttons.First().transform.parent.gameObject, false);
                        foreach (Transform child in buttons.First().transform.parent.parent)
                        {
                            if (child.gameObject.name == "MenuFadingBackground")
                            {
                                GameObjectManager.setGameObjectState(child.gameObject, false);
                                break;
                            }
                        }
                    }
                    if (textCount < readTexts.Length)
                    {
                        sdText.text = readTexts[textCount];
                        fadingIn = true;
                        readingTimer = Time.time;
                    }
                    else
                    {
                        sdText.text = "";
                        fadingOutOfReadingMode = true;
                        readingTimer = Time.time;
                    }
                }
            }
            else if (fadingOutOfReadingMode)
            {
                if(Time.time - readingTimer < 4)
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, 1 - (Time.time - readingTimer) / 4);
                    c = background.color;
                    background.color = new Color(c.r, c.g, c.b, 1 - (Time.time - readingTimer) / 4);
                }
                else
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, 1);
                    c = background.color;
                    background.color = new Color(c.r, c.g, c.b, 1);
                    GameObjectManager.setGameObjectState(storyDisplayer.First(),false);
                    fadingOutOfReadingMode = false;
                    textCount = 0;
                    readTexts = null;
                    readingIntro = false;
                    readingTransition = false;
                    readingEnding = false;
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && !end)
                {
                    fadingOut = true;
                    readingTimer = Time.time;
                    GameObjectManager.setGameObjectState(clickFeedback,false);
                }
            }
        }
        else
        {
            reading = false;
        }
	}
}