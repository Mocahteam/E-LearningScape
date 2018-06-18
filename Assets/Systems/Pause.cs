using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Pause : FSystem {

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family storyDisplayer = FamilyManager.getFamily(new AnyOfTags("StoryDisplayer"));

    public static bool playerEnabled = true;
    private GameObject menuGO;
    public static bool askResume = false;

	private GameObject forGO;

    public Pause()
    {
        if (Application.isPlaying)
        {
            ResetStaticVariables();
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
            foreach(Transform child in storyDisplayer.First().transform)
            {
                if(child.gameObject.name == "EndScreen")
                {
                    foreach(Transform c in child)
                    {
                        if(c.gameObject.name == "Restart")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(Restart);
                        }
                        else if (c.gameObject.name == "Menu")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(GoToMenu);
                        }
                    }
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
        ResetStaticVariables();
        GameObjectManager.loadScene("Sapiens");
    }

    void GoToMenu() //open menu scene
    {
        GameObjectManager.loadScene("Menu");
    }

    public static void ResetStaticVariables()
    {
        ShowUI.askCloseWindow = false;
        ShowUI.eraserDragged = false;
        SetAnswer.credits = false;
        IARTab.onIAR = false;
        IARTab.room1Unlocked = false;
        IARTab.room2Unlocked = false;
        IARTab.room3Unlocked = false;
        IARTab.askCloseIAR = false;
        Inventory.playerEnabled = true;
        Inventory.wireOn = true;
        CollectableGO.onInventory = false;
        CollectableGO.usingWire = false;
        CollectableGO.usingKeyE03 = false;
        CollectableGO.usingKeyE08 = false;
        CollectableGO.usingGlasses1 = false;
        CollectableGO.usingGlasses2 = false;
        CollectableGO.usingLamp = false;
        CollectableGO.askCloseInventory = false;
        CollectableGO.askOpenInventory = false;
        DreamFragmentCollect.onFragment = false;
        Selectable.selected = false;
        Selectable.askRight = false;
        Selectable.askWrong = false;
        Takable.objectTaken = false;
        Takable.mirrorOnPlank = false;
        StoryDisplaying.readingTransition = false;
        StoryDisplaying.readingEnding = false;
        StoryDisplaying.reading = false;
        playerEnabled = true;
        askResume = false;
        Timer.addTimer = false;
    }
}