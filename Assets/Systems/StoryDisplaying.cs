using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;

public class StoryDisplaying : FSystem {

    private Family storyDisplayer = FamilyManager.getFamily(new AnyOfTags("StoryDisplayer"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private Image background;
    private TextMeshProUGUI sdText;
    private Image fadingImage;
    private GameObject clickFeedback;

    private bool readingIntro = false;
    public static bool readingTransition = false;
    public static bool readingEnding = false;

    private string[] readTexts;
    private float readingTimer = -Mathf.Infinity;
    private int textCount = 0;
    private bool fadingIn = false;
    private bool fadingOut = false;
    private bool fadingToReadingMode = false;
    private bool fadingOutOfReadingMode = false;

    private string[] introText;
    private string[] transitionText;
    private string[] endingText;
    
    public StoryDisplaying()
    {
        foreach(Transform child in storyDisplayer.First().transform)
        {
            if(child.gameObject.name == "Background")
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
        }

        introText = new string[3];
        introText[0] = "Il est déjà tard le soir, Camille reste inquiète pour son premier enseignement à Sorbonne Université. Ce n’est pas la première fois qu’elle enseigne, mais maintenant elle est Maître de Conférences, son premier poste permanent.";
        introText[1] = "Quand elle brosse ses dents, elle révise une fois de plus le cours du prof qu’elle connaît déjà par coeur. Puis elle se couche, et plein de questions tournent dans sa tête : et si j’ai un blanc ? et si les étudiants ne sont pas motivés ? et s’il n’y a pas de craie ?";
        introText[2] = "Camille fini par s'endormir... C'est alors que les marchands de sable entrent en action pour aider Camille à structurer sa pensée pendant son sommeil.";

        transitionText = new string[1];
        transitionText[0] = "Camille est tombée en sommeil paradoxal, et elle se met à rêver. L’université se transforme en jungle pleine de dangers. Les marchands de sable restent avec elle pour la protéger.";

        endingText = new string[2];
        endingText[0] = "Camille se réveille. Elle est en pleine forme, et tout est clair dans sa tête pour son premier enseignement. Elle a hâte de commencer.";
        endingText[1] = "Les marchands de sable ont bien travaillé et se retrouvent pour un débriefing sur tout leur travail";

        if (storyDisplayer.First().activeSelf)
        {
            readTexts = introText;
            player.First().GetComponent<FirstPersonController>().enabled = false;
            Cursor.visible = false;
            readingTimer = Time.time;
            fadingIn = true;
            readingIntro = true;
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
            if(readTexts == null)
            {
                storyDisplayer.First().SetActive(true);
                player.First().GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = false;
                if (readingIntro)
                {
                    readTexts = introText;
                    readingTimer = Time.time;
                    fadingIn = true;
                    fadingImage.color = Color.black;
                    background.color = Color.black;
                    sdText.text = readTexts[0];
                }
                else if (readingTransition)
                {
                    readTexts = transitionText;
                    readingTimer = Time.time;
                    fadingToReadingMode = true;
                    fadingImage.color = Color.black*0;
                    background.color = Color.black*0;
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
                    clickFeedback.SetActive(true);
                }
            }
            else if (fadingOut)
            {
                if (Time.time - readingTimer < 2)
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, (Time.time - readingTimer) / 2);
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
                        sdText.text = "";
                        fadingOutOfReadingMode = true;
                        readingTimer = Time.time;
                    }
                }
            }
            else if (fadingToReadingMode)
            {
                if (Time.time - readingTimer < 4)
                {
                    Color c = fadingImage.color;
                    fadingImage.color = new Color(c.r, c.g, c.b, (Time.time - readingTimer) / 4);
                    c = background.color;
                    background.color = new Color(c.r, c.g, c.b, (Time.time - readingTimer) / 4);
                }
                else
                {
                    fadingImage.color = Color.black;
                    background.color = Color.black;
                    fadingToReadingMode = false;
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
                    storyDisplayer.First().SetActive(false);
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
                if (Input.GetMouseButtonDown(0))
                {
                    fadingOut = true;
                    readingTimer = Time.time;
                    clickFeedback.SetActive(false);
                }
            }
        }
	}
}