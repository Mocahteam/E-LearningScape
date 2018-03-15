using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Pause : FSystem {

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private bool gamePaused = false;
    private bool playerEnabled = true;

    public Pause()
    {
        foreach(GameObject ui in canvas)
        {
            if(ui.name == "PauseMenu")
            {
                foreach(Button b in ui.GetComponentsInChildren<Button>())
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
                ui.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (gamePaused)
            {
                Resume();
            }
            else
            {
                foreach (GameObject ui in canvas)
                {
                    if (ui.name == "PauseMenu")
                    {
                        ui.SetActive(true);
                    }
                }
                foreach(FSystem s in FSystemManager.updateSystems())
                {
                    if(s.ToString() != "Pause" && s.ToString() != "TimerSystem")
                    {
                        s.Pause = true;
                    }
                }
                playerEnabled = player.First().GetComponent<FirstPersonController>().enabled;
                //disable player moves
                player.First().GetComponent<FirstPersonController>().enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                gamePaused = true;
            }
        }
	}

    void Resume()
    {
        foreach (GameObject ui in canvas)
        {
            if (ui.name == "PauseMenu")
            {
                ui.SetActive(false);
            }
        }
        foreach (FSystem s in FSystemManager.updateSystems())
        {
            if (s.ToString() != "Pause" && s.ToString() != "TimerSystem")
            {
                s.Pause = false;
            }
        }
        if (playerEnabled)
        {
            //enale player moves
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        gamePaused = false;
    }

    void Restart()
    {

    }

    void GoToMenu()
    {
        GameObjectManager.loadScene("Menu");
    }
}