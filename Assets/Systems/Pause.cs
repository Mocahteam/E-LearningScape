using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Pause : FSystem {

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private bool gamePaused = false;
    public static bool playerEnabled = true;
    private GameObject menuGO;
    public static bool askResume = false;

	private GameObject forGO;

    public Pause()
    {
		int nbCanvas = canvas.Count;
        //initialise pause menu's buttons with listeners
		for(int i = 0; i < nbCanvas; i++)
        {
			forGO = canvas.getAt (i);
			if(forGO.name == "PauseMenu")
            {
                menuGO = forGO;
				foreach(Button b in forGO.GetComponentsInChildren<Button>())
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
				forGO.SetActive(false);    //hide pause menu at the beginning
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
        gamePaused = menuGO.activeSelf;
        if (Input.GetKeyDown(KeyCode.Escape) && ((menuGO.activeSelf && IARTab.onIAR) || !IARTab.onIAR)) //when escape key is pressed
        {
            if (gamePaused)
            {
                Resume(); //resume the game if it was paused
            }
            else
            {
				int nbCanvas = canvas.Count;
				for(int i = 0; i < nbCanvas; i++)
                {
					forGO = canvas.getAt (i);
					if (forGO.name == "PauseMenu")
                    {
						forGO.SetActive(true); //show pause menu
                    }
                }
                /*foreach(FSystem s in FSystemManager.updateSystems())
                {
                    //stop all FYFY systems but "Pause" and the timer
                    if (s.ToString() != "Pause" && s.ToString() != "TimerSystem" && s.ToString() != "IARTab")
                    {
                        s.Pause = true;
                    }
                }*/
                playerEnabled = player.First().GetComponent<FirstPersonController>().enabled; //save the playing state of the player (moving or object selected/using cursor)
                //disable player moves
                player.First().GetComponent<FirstPersonController>().enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                gamePaused = true;
            }
        }
        if (askResume)
        {
            Resume();
        }
	}

    void Resume()   //resume game
	{
		menuGO.SetActive(false); //hide pause menu
        //unpause all FYFY systems
        /*foreach (FSystem s in FSystemManager.updateSystems())
        {
            s.Pause = false;
        }*/
        if (playerEnabled)
        {
            //enable player moves
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        gamePaused = false;
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