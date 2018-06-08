using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Pause : FSystem {

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    
    public static bool playerEnabled = true;
    private GameObject menuGO;
    public static bool askResume = false;

	private GameObject forGO;

    public Pause()
    {
        if (Application.isPlaying)
        {
            int nbCanvas = canvas.Count;
            //initialise pause menu's buttons with listeners
            for (int i = 0; i < nbCanvas; i++)
            {
                forGO = canvas.getAt(i);
                if (forGO.name == "PauseMenu")
                {
                    menuGO = forGO;
                    foreach (Button b in forGO.GetComponentsInChildren<Button>())
                    {
                        switch (b.name)
                        {
                            case "Resume":
                                b.onClick.AddListener(Resume);
                                break;

                            case "Restart":
                                b.onClick.AddListener(Restart);
                                break;

                            case "Menu":
                                b.onClick.AddListener(GoToMenu);
                                break;

                            default:
                                break;
                        }
                    }
                    GameObjectManager.setGameObjectState(menuGO, false);    //hide pause menu at the beginning
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
        if (askResume)
        {
            Resume();
        }
	}

    void Resume()   //resume game
    {
        GameObjectManager.setGameObjectState(menuGO, false); //hide pause menu
        if (playerEnabled)
        {
            //enable player moves
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        askResume = false;
    }

    void Restart() //restart game from beginning
    {
        GameObjectManager.loadScene("Sapiens");
    }

    void GoToMenu() //open menu scene
    {
        GameObjectManager.loadScene("Menu");
    }
}