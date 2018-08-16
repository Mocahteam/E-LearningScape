using UnityEngine;
using FYFY;
using UnityEngine.UI;
using PauseSystem = IARMenuManager;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.PointerManager;

public class IARTab_old : FSystem {

    // gère le changement de TAB + la fermeture de l'IAR + gère les animations de changement de salle

    private Family inventoryFamily = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family screens = FamilyManager.getFamily(new AnyOfTags("ScreenIAR"));
    private Family tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"));
    private Family audioSourceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)));
    private Family door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));
    private Family fences = FamilyManager.getFamily(new AnyOfTags("Fence"));
    private Family lockR2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"));
    private Family lockIntro = FamilyManager.getFamily(new AnyOfTags("LockIntro"));
    private Family fgm = FamilyManager.getFamily(new AllOfComponents(typeof(FocusedGOMaterial)));
    private Family backgrounds = FamilyManager.getFamily(new AnyOfTags("UIBackground"));
    private Family hud = FamilyManager.getFamily(new AnyOfTags("HUDInputs"));
    private Family inputfields = FamilyManager.getFamily(new AllOfComponents(typeof(InputField)));
    private Family game = FamilyManager.getFamily(new AnyOfTags("GameRooms"));

    private GameObject tabsGO;
    public static bool onIAR = false;
    public static bool room1Unlocked = false;
    public static bool room2Unlocked = false;
    public static bool room3Unlocked = false;
    private bool listenerAddedRoom1 = false;
    private bool listenerAddedRoom2 = false;
    private bool listenerAddedRoom3 = false;

    private GameObject inventory;
    private GameObject screenR1;
    private GameObject screenR2;
    private GameObject screenR3;
    //private GameObject menu;

    private Image inventoryButtonImage;
    private Image screen1ButtonImage;
    private Image screen2ButtonImage;
    private Image screen3ButtonImage;
    private Image menuButtonImage;
    private Sprite selectedTabSprite;
    private Sprite initialTabSprite;

    public static bool askCloseIAR;
    private bool onInventory = false;
    private bool onMenu = false;
    private GameObject activeUI = null;
    private bool playerEnabled = true;
    private Material initialBGMaterial;
    private bool inputfieldFocused = false;

    private bool wasOnInventory = false;
    private bool wasOnMenu = false;
    private bool windowClosed = false;

    private GameObject fence1;
    private GameObject fence2;

    private bool playerLookingAtDoor = false;
    private Vector3 tmpTarget;
    private float angleCount = 0;
    private float speed;

    private AudioSource gameAudioSource;

    private bool hideMouse = false;
    private bool canHideMouse = false;

    public IARTab_old()
    {
        if (Application.isPlaying)
        {
            door.First().transform.position += Vector3.up * (5.73f - door.First().transform.position.y); //opened
            door.First().transform.position += Vector3.up * (2.13f - door.First().transform.position.y); //closed

            tabsGO = tabs.First().transform.parent.gameObject;
            inventory = inventoryFamily.First();
            //initialBGMaterial = backgrounds.First().GetComponent<Image>().material;

            int nb = screens.Count;
            for (int i = 0; i < nb; i++)
            {
                if (screens.getAt(i).name.Contains(1.ToString()))
                {
                    screenR1 = screens.getAt(i);
                }
                else if (screens.getAt(i).name.Contains(2.ToString()))
                {
                    screenR2 = screens.getAt(i);
                }
                else if (screens.getAt(i).name.Contains(3.ToString()))
                {
                    screenR3 = screens.getAt(i);
                }
            }

//            menu = GameObject.Find("MenuContent");

            nb = tabs.Count;
            for (int i = 0; i < nb; i++)
            {
                switch (tabs.getAt(i).name)
                {
                    case "InventoryTab":
                        inventoryButtonImage = tabs.getAt(i).GetComponent<Image>();
                        tabs.getAt(i).GetComponent<Button>().onClick.AddListener(delegate {
                            SwitchTab(inventory, inventoryButtonImage);
                            onInventory = true;
                        });
                        break;

                    case "ScreenR1Tab":
                        screen1ButtonImage = tabs.getAt(i).GetComponent<Image>();
                        break;

                    case "ScreenR2Tab":
                        screen2ButtonImage = tabs.getAt(i).GetComponent<Image>();
                        break;

                    case "ScreenR3Tab":
                        screen3ButtonImage = tabs.getAt(i).GetComponent<Image>();
                        break;

/*                    case "MenuTab":
                        menuButtonImage = tabs.getAt(i).GetComponent<Image>();
                        tabs.getAt(i).GetComponent<Button>().onClick.AddListener(delegate {
                            SwitchTab(menu, menuButtonImage);
                            onMenu = true;
                        });
                        break;*/

                    default:
                        break;
                }
            }

            selectedTabSprite = fgm.First().GetComponent<FocusedGOMaterial>().selectedTabSprite;
            initialTabSprite = inventoryButtonImage.sprite;

            nb = audioSourceFamily.Count;
            for (int i = 0; i < nb; i++)
            {
                if (audioSourceFamily.getAt(i).name == "Game")
                {
                    gameAudioSource = audioSourceFamily.getAt(i).GetComponent<AudioSource>();
                }
            }

            nb = fences.Count;
            for (int i = 0; i < nb; i++)
            {
                if (fences.getAt(i).name.Contains(1.ToString()))
                {
                    fence1 = fences.getAt(i);
                }
                else if (fences.getAt(i).name.Contains(2.ToString()))
                {
                    fence2 = fences.getAt(i);
                }
            }
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        speed = 50 * Time.deltaTime;

        if (askCloseIAR)
        {
            if (activeUI)
            {
                GameObjectManager.setGameObjectState(activeUI, false);
            }
            askCloseIAR = false;
        }

        if (inventory.activeSelf && !onInventory)
        {
            onIAR = true;
            playerEnabled = Inventory.playerEnabled;
            SwitchTab(inventory, inventoryButtonImage);
            GameObjectManager.setGameObjectState(tabsGO,true);
        }
/*        else if (menu.activeSelf && !onMenu)
        {
            onIAR = true;
            playerEnabled = PauseSystem.playerEnabled;
            SwitchTab(menu, menuButtonImage);
            GameObjectManager.setGameObjectState(tabsGO,true);
        }
        onMenu = menu.activeSelf;*/
        onInventory = inventory.activeSelf;

        wasOnInventory = onInventory || wasOnInventory;
        wasOnMenu = onMenu || wasOnMenu;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeUI)
            {
                GameObjectManager.setGameObjectState(activeUI, false);
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            inputfieldFocused = false;
            int nbInputFields = inputfields.Count;
            for (int i = 0; i < nbInputFields; i++)
            {
                if (inputfields.getAt(i).GetComponent<InputField>().isFocused)
                {
                    inputfieldFocused = true;
                    break;
                }
            }
            if (activeUI && !inputfieldFocused)
            {
                GameObjectManager.setGameObjectState(activeUI, false);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            int nb = backgrounds.Count;
            for(int i = 0; i < nb; i++)
            {
                if (backgrounds.getAt(i).GetComponent<PointerOver>())
                {
                    GameObjectManager.setGameObjectState(activeUI, false);
                    break;
                }
            }
        }

        if (activeUI)
        {
            if (!activeUI.activeSelf)
            {
                if (room2Unlocked && !listenerAddedRoom2)
                {
                    if (windowClosed)
                    {
                        onIAR = false;
                        activeUI = null;
                        GameObjectManager.setGameObjectState(tabsGO,false);
                        if (wasOnInventory)
                        {
                            CollectableGO.askCloseInventory = true;
                        }
                        if (wasOnMenu)
                        {
//                            PauseSystem.askResume = true;
                        }
                        wasOnInventory = false;
                        wasOnMenu = false;
                        Inventory.playerEnabled = true;
//                        PauseSystem.playerEnabled = true;
                        windowClosed = false;
                    }
                    else
                    {
                        ShowUI.askCloseWindow = true;
                        windowClosed = true;
                    }
                }
                else
                {
                    onIAR = false;
                    activeUI = null;
                    GameObjectManager.setGameObjectState(tabsGO,false);
                    if (wasOnInventory)
                    {
                        CollectableGO.askCloseInventory = true;
                    }
                    if (wasOnMenu)
                    {
//                        PauseSystem.askResume = true;
                    }
                    wasOnInventory = false;
                    wasOnMenu = false;
                    Inventory.playerEnabled = playerEnabled;
//                    PauseSystem.playerEnabled = playerEnabled;
                }
            }
        }
        if (room1Unlocked && !listenerAddedRoom1)
        {
            player.First().GetComponent<FirstPersonController>().enabled = false;
            Cursor.visible = false;
            if (!playerLookingAtDoor)
            {
                tmpTarget = wallIntro.First().transform.position + Vector3.up - Camera.main.transform.position;
                Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180 * speed, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                {
                    Camera.main.transform.forward = tmpTarget;
                    gameAudioSource.clip = lockIntro.First().GetComponent<Selectable>().right;
                    gameAudioSource.PlayDelayed(0);
                    gameAudioSource.loop = true;
                    playerLookingAtDoor = true;
                    tmpTarget = wallIntro.First().transform.position + Vector3.up * (-4f - wallIntro.First().transform.position.y);
                }
            }
            else
            {
                wallIntro.First().transform.position = Vector3.MoveTowards(wallIntro.First().transform.position, tmpTarget, 0.1f * speed);
                if (wallIntro.First().transform.position == tmpTarget)
                {
                    GameObjectManager.setGameObjectState(wallIntro.First(),false);
                    playerLookingAtDoor = false;
                    gameAudioSource.loop = false;
                    screen1ButtonImage.GetComponent<Button>().onClick.AddListener(delegate {
                        SwitchTab(screenR1, screen1ButtonImage);
                    });
                    GameObjectManager.setGameObjectState(screen1ButtonImage.gameObject, true);
                    listenerAddedRoom1 = true;
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                }
            }
        }
        if (room2Unlocked && !listenerAddedRoom2)
        {
            if (!activeUI)
            {
                player.First().GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = false;
                if (!playerLookingAtDoor)
                {
                    tmpTarget = door.First().transform.position - Camera.main.transform.position;
                    Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180 * speed, 0);
                    Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                    if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                    {
                        Camera.main.transform.forward = tmpTarget;
                        gameAudioSource.clip = door.First().GetComponent<Door>().openAudio;
                        gameAudioSource.PlayDelayed(0);
                        gameAudioSource.loop = true;
                        playerLookingAtDoor = true;
                        tmpTarget = door.First().transform.position + Vector3.up * (5.73f - door.First().transform.position.y);

                        foreach (Transform room in game.First().transform)
                        {
                            if (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString()))
                            {
                                GameObjectManager.setGameObjectState(room.gameObject, true);
                            }
                        }
                    }
                }
                else
                {
                    door.First().transform.position = Vector3.MoveTowards(door.First().transform.position, tmpTarget, 0.1f * speed);
                    if (door.First().transform.position == tmpTarget)
                    {
                        playerLookingAtDoor = false;
                        gameAudioSource.loop = false;
                        screen2ButtonImage.GetComponent<Button>().onClick.AddListener(delegate {
                            SwitchTab(screenR2, screen2ButtonImage);
                        });
                        GameObjectManager.setGameObjectState(screen2ButtonImage.gameObject, true);
                        listenerAddedRoom2 = true;
                        StoryDisplaying.readingTransition = true;
                    }
                }
            }
            else
            {
                GameObjectManager.setGameObjectState(activeUI, false);
            }
        }
        if (room3Unlocked && !listenerAddedRoom3)
        {
            player.First().GetComponent<FirstPersonController>().enabled = false;
            Cursor.visible = false;
            if (!playerLookingAtDoor)
            {
                tmpTarget = (fence1.transform.position + fence2.transform.position)/2 - Camera.main.transform.position;
                Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180 * speed, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                {
                    Camera.main.transform.forward = tmpTarget;
                    gameAudioSource.PlayOneShot(lockR2.First().GetComponent<Selectable>().right);
                    playerLookingAtDoor = true;
                }
            }
            else
            {
                fence1.transform.Rotate(0, 0, -Time.deltaTime * 100);
                fence2.transform.Rotate(0, 0, Time.deltaTime * 100);
                angleCount+= Time.deltaTime * 100;
                if (angleCount > 103)
                {
                    fence1.transform.Rotate(0, 0, -(103 - angleCount));
                    fence2.transform.Rotate(0, 0, 103 - angleCount);
                    angleCount = 0;
                    playerLookingAtDoor = false;
                    screen3ButtonImage.GetComponent<Button>().onClick.AddListener(delegate {
                        SwitchTab(screenR3, screen3ButtonImage);
                    });
                    GameObjectManager.setGameObjectState(screen3ButtonImage.gameObject, true);
                    listenerAddedRoom3 = true;
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                }
            }
        }
    }

    private void SwitchTab(GameObject tabContent, Image button)
    {
        if (!CollectableGO.onInventory && tabContent.tag == "Inventory")
        {
            CollectableGO.askOpenInventory = true;
        }
        GameObjectManager.setGameObjectState(inventory, false);
        GameObjectManager.setGameObjectState(screenR1, false);
        GameObjectManager.setGameObjectState(screenR2, false);
        GameObjectManager.setGameObjectState(screenR3, false);
        //GameObjectManager.setGameObjectState(menu, false);
        int nb = tabs.Count;
        for(int i = 0; i < nb; i++)
        {
            tabs.getAt(i).GetComponent<Image>().sprite = initialTabSprite;
            tabs.getAt(i).GetComponentInChildren<Text>().fontStyle = FontStyle.Normal;
        }

        GameObjectManager.setGameObjectState(tabContent, true);
        button.sprite = selectedTabSprite;
        button.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        activeUI = tabContent;
        Inventory.playerEnabled = playerEnabled;
//        PauseSystem.playerEnabled = playerEnabled;
    }
}