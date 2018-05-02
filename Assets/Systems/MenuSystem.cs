using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class MenuSystem : FSystem {

    private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
    private Family menu = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));

    public MenuSystem()
    {
        //initialise menu's buttons with listeners
        foreach (GameObject b in buttons)
        {
            switch (b.name)
            {
                case "Play":
                    b.GetComponent<Button>().onClick.AddListener(Play);
                    break;

                case "Sapiens":
                    b.GetComponent<Button>().onClick.AddListener(Sapiens);
                    break;

                case "Option":
                    b.GetComponent<Button>().onClick.AddListener(OpenOption);
                    break;

                case "Quit":
                    b.GetComponent<Button>().onClick.AddListener(QuitGame);
                    break;

                case "Back":
                    b.GetComponent<Button>().onClick.AddListener(BackFromOption);
                    break;

                default:
                    break;
            }
        }
        foreach (Transform child in menu.First().transform)
        {
            if(child.gameObject.name == "Main") //show main menu
            {
                child.gameObject.SetActive(true);
            }
            else if (child.gameObject.name == "Option") //hide option menu
            {
                child.gameObject.SetActive(false);
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
	}

    void Play()
    {
        GameObjectManager.loadScene("Proto");   //load prototype scene (not used anymore)
    }

    void Sapiens()
    {
        GameObjectManager.loadScene("Sapiens"); //load Sapiens escape game scene
    }

    void OpenOption()
    {
        foreach (Transform child in menu.First().transform)
        {
            if (child.gameObject.name == "Main")    //hide main menu
            {
                child.gameObject.SetActive(false);
            }
            else if (child.gameObject.name == "Option") //show option menu
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    void QuitGame()
    {
        Application.Quit(); //quit the game
    }

    void BackFromOption()
    {
        foreach (Transform child in menu.First().transform)
        {
            if (child.gameObject.name == "Main")    //show main menu
            {
                child.gameObject.SetActive(true);
            }
            else if (child.gameObject.name == "Option") //hide option menu
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}