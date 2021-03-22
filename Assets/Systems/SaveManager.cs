using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.IO;
using Newtonsoft.Json;
using FYFY;
using FYFY_plugins.Monitoring;
using FYFY_plugins.PointerManager;
using FYFY_plugins.TriggerManager;
using TMPro;

public class SaveManager : FSystem {

	private Family f_prefabs = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)));
	private Family f_menuButtons = FamilyManager.getFamily(new AllOfComponents(typeof(FadingMenu), typeof(Button)));
	private Family f_inGameMenu = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(WindowNavigator)));

	private Family f_fpsController = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));
	private Family f_timer = FamilyManager.getFamily(new AllOfComponents(typeof(Timer)));
	private Family f_componentMonitoring = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));
	private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

	private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));
	private Family f_queries = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(QuerySolution)));
	private Family f_gearsSet = FamilyManager.getFamily(new AnyOfTags("Gears"), new AllOfComponents(typeof(LinkedWith)));
	private Family f_room2Password = FamilyManager.getFamily(new AnyOfTags("PasswordRoom2"));

	private Family f_collectable = FamilyManager.getFamily(new AllOfComponents(typeof(LinkedWith), typeof(ComponentMonitoring)), new NoneOfComponents(typeof(DreamFragment), typeof(Locker)), new NoneOfLayers(5), new NoneOfTags("Plank"));
	private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

	private Family f_pressY = FamilyManager.getFamily(new AnyOfTags("PressY"));
	private Family f_toggleable = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleableGO), typeof(Animator)));
	private Family f_whiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"));
	private Family f_boardTexture = FamilyManager.getFamily(new AllOfComponents(typeof(ChangePixelColor)));
	private Family f_CrouchHint = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites), typeof(PointerSensitive), typeof(LinkedWith), typeof(BoxCollider)));
	private Family f_OutOfFirstRoom = FamilyManager.getFamily(new AllOfComponents(typeof(TriggerSensitive3D), typeof(LinkedWith)));
	private Family f_fragmentNotif = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragmentFlag)));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_enabledHintsIAR = FamilyManager.getFamily(new AllOfComponents(typeof(HintContent)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));

    public static SaveManager instance;

	private string saveFolderPath = "./Savegames";
	private string autoSaveFileName = "auto_save";
	private string saveFilesExtension = ".txt";

	private GameObject loadPopup;
	private GameObject loadButtonPrefab;
	private GameObject loadListContainer;
	// the load button in main menu
	private Button menuLoadButton;
	// the load button in the popup window
	private Button popupLoadButton;
	private GameObject selectedLoadButton;

	private GameObject savePopup;
	private GameObject saveButtonPrefab;
	private GameObject saveListContainer;
	// the save button in the in game menu
	private Button menuSaveButton;
	private GameObject menuSaveButtonNotice;
	private TMP_InputField popupSaveInputfield;
	// popup windows displayed when the player clicks on popupSaveButton
	private GameObject popupSaveInvalid;
	private GameObject popupSaveOverride;
	private GameObject popupSaveDone;

	private bool autosaveExists;

	private string tmpPath;
	private DreamFragment tmpDF;
	private GameObject tmpGO;
	private DreamFragmentToggle tmpDFToggle;
	private ComponentMonitoring[] tmpMonitorsArray;
	private FileInfo[] tmpFI;

    private float lastAutoSave = 0;
    private float autoSaveFrequency = 10;

    public SaveManager()
	{
		if(Application.isPlaying)
		{
			// look for the load window
			foreach (GameObject go in f_prefabs)
			{
				if (go.name == "LoadPopup")
				{
					loadPopup = go;
					loadButtonPrefab = go.GetComponent<PrefabContainer>().prefab;
					loadListContainer = go.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
					foreach (Button b in go.GetComponentsInChildren<Button>())
						if (b.gameObject.name == "LoadButton")
						{
							popupLoadButton = b;
							break;
						}
				}
				else if (go.name == "SavePopup")
				{
					savePopup = go;
					saveButtonPrefab = go.GetComponent<PrefabContainer>().prefab;
					saveListContainer = go.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
                    popupSaveInputfield = go.GetComponentInChildren<TMP_InputField>();
                    foreach (LinkedWith link in go.GetComponents<LinkedWith>())
                    {
                        switch (link.link.name)
                        {
							case "Invalid":
								popupSaveInvalid = link.link.gameObject;
								break;

							case "Override":
								popupSaveOverride = link.link.gameObject;
								break;

							case "Saved":
								popupSaveDone = link.link.gameObject;
								break;

                            default:
								break;
                        }
                    }
				}
			}
			// find main menu load button 
			foreach (GameObject go in f_menuButtons)
				if (go.name == "Load")
				{
					menuLoadButton = go.GetComponent<Button>();
					break;
				}
			// find in game menu save button
			foreach(GameObject go in f_inGameMenu)
				if(go.name == "MenuContent")
                {
					tmpGO = go;
					break;
                }
			foreach(Transform child in tmpGO.transform)
				if(child.gameObject.name == "Save")
                {
					menuSaveButton = child.GetComponent<Button>();
					break;
                }
			menuSaveButtonNotice = menuSaveButton.transform.GetChild(1).gameObject;

            // browse save folder
            DirectoryInfo di;
            if (!Directory.Exists(saveFolderPath))
                di = Directory.CreateDirectory(saveFolderPath);
            else
                di = new DirectoryInfo(saveFolderPath);
            tmpFI = di.GetFiles(string.Concat("*", saveFilesExtension));
            foreach (FileInfo file in tmpFI)
            {
                if (file.Name == autoSaveFileName+saveFilesExtension)
                    autosaveExists = true;
                AddNewItemToLists(file);
            }
            // enable loading button if there are valid saves
            if (loadListContainer && loadListContainer.transform.childCount > 0)
                menuLoadButton.interactable = true;

            GameObjectManager.setGameObjectState(menuLoadButton.gameObject, LoadGameContent.gameContent.saveAndLoadProgression);
			GameObjectManager.setGameObjectState(menuSaveButton.gameObject, LoadGameContent.gameContent.saveAndLoadProgression);
		}
		instance = this;
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        if (Time.time - lastAutoSave > autoSaveFrequency && f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber > 0)
        {
            menuSaveButton.interactable = true;
            lastAutoSave = Time.time;
            AutoSave();
        }
    }

    /// <summary>
    /// Save the game in an auto save file
    /// </summary>
    private void AutoSave()
    {
        // don't save if auto save is disabled or if introduction is not completed
        if (!LoadGameContent.gameContent.autoSaveProgression || f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber == 0)
            return;

        string fullpath = SaveOnFile(autoSaveFileName);
        if (!autosaveExists && fullpath != "")
        {
            // if the auto save didn't already exist, add it to the list in the popup
            tmpPath = string.Concat(saveFolderPath, "/", autoSaveFileName, saveFilesExtension);
            // check if file exists
            FileInfo newSaveGame = new FileInfo(tmpPath);
            if (newSaveGame.Exists)
                AddNewItemToLists(newSaveGame);

            autosaveExists = true;
        }
    }

    private void AddNewItemToLists(FileInfo file)
    {
        SaveContent saveContent = null;

        if (file.Exists)
        {
            try
            {
                saveContent = JsonConvert.DeserializeObject<SaveContent>(File.ReadAllText(file.FullName)); 
            }
            catch (Exception) {
                Debug.LogError("The save couldn't be loaded because of invalid content.");
                File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - The save couldn't be loaded because of invalid content."));
                return;
            }
        }
        else
        {
            Debug.LogError("The save couldn't be loaded because of invalid file path.");
            File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - The save couldn't be loaded because of invalid file path."));
            return;
        }

        // create button in load UI
        GameObject loadListElem = GameObject.Instantiate(loadButtonPrefab);
        loadListElem.name = Path.GetFileNameWithoutExtension(file.Name);
        loadListElem.GetComponentInChildren<TextMeshProUGUI>().text = loadListElem.name;
        loadListElem.GetComponent<SaveComponent>().content = saveContent;
        loadListElem.GetComponent<Button>().onClick.AddListener(delegate
        {
            selectedLoadButton = loadListElem;
            popupLoadButton.interactable = true;
        });
        loadListElem.transform.SetParent(loadListContainer.transform);
        loadListElem.transform.localScale = Vector3.one;
        GameObjectManager.bind(loadListElem);
        // create button in save UI
        GameObject saveListElem = GameObject.Instantiate(saveButtonPrefab);
        saveListElem.name = Path.GetFileNameWithoutExtension(file.Name);
        saveListElem.GetComponentInChildren<TextMeshProUGUI>().text = saveListElem.name;
        saveListElem.GetComponent<Button>().onClick.AddListener(delegate {
            popupSaveInputfield.text = saveListElem.name;
        });
        saveListElem.transform.SetParent(saveListContainer.transform);
        saveListElem.transform.localScale = Vector3.one;
        GameObjectManager.bind(saveListElem);
    }
    
    /// <summary>
    /// Called when clicking on the save button in the popup or when answering yes to override file
    /// </summary>
    /// <param name="checkName">checkName is false when already checked and this function is called after answering yes to override</param>
    public void TrySaving(bool checkName = true)
    {
        // don't save if introduction is not completed
        if (f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber == 0)
            return;

        if (checkName && !CheckSaveNameValidity())
            // display popup for invalid name
            GameObjectManager.setGameObjectState(popupSaveInvalid, true);
        else
        {
            if (!checkName) // means overriding
            {
                // We don't have to check name (already done), we proceed to save
                string fullpath = SaveOnFile(popupSaveInputfield.text);
                if (fullpath != "")
                {
                    GameObjectManager.setGameObjectState(popupSaveDone, true);

                    GameObjectManager.addComponent<ActionPerformedForLRS>(savePopup, new
                    {
                        verb = "saved",
                        objectType = "serious-game",
                        objectName = "E-LearningScape progression"
                    });
                }
                else
                    // error: display popup for invalid name
                    GameObjectManager.setGameObjectState(popupSaveInvalid, true);
            }
            else
            {
                // check if this name is already used
                bool usedName = false;
                foreach (Transform child in saveListContainer.transform)
                    if (popupSaveInputfield.text == child.gameObject.name)
                    {
                        usedName = true;
                        break;
                    }
                if (usedName)
                    // display popup asking for override
                    GameObjectManager.setGameObjectState(popupSaveOverride, true);
                else
                {
                    string fullpath = SaveOnFile(popupSaveInputfield.text);
                    // check if file exists
                    FileInfo newSaveGame = new FileInfo(fullpath);
                    if (fullpath != "" && newSaveGame.Exists)
                    {
                        GameObjectManager.setGameObjectState(popupSaveDone, true);

                        AddNewItemToLists(newSaveGame);

                        GameObjectManager.addComponent<ActionPerformedForLRS>(savePopup, new
                        {
                            verb = "saved",
                            objectType = "serious-game",
                            objectName = "E-LearningScape progression"
                        });
                    }
                    else
                        // error: display popup for invalid name
                        GameObjectManager.setGameObjectState(popupSaveInvalid, true);
                }
            }
        }
    }

    /// <summary>
    /// Encrypt and save the content of "save" in a file
    /// </summary>
    /// <param name="path"></param>
    private string SaveOnFile(string fileName)
	{
		// don't save if introduction is not completed
		if (f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber == 0)
			return "";

        SaveContent saveContent = new SaveContent();
        saveContent.sessionID = LoadGameContent.sessionID;
        saveContent.storyTextCount = StoryDisplaying.instance.GetStoryProgression();
        saveContent.lastRoomUnlocked = f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber;
        Vector3 fpsPosition = f_fpsController.First().gameObject.transform.position;
        saveContent.playerPosition[0] = fpsPosition.x;
        saveContent.playerPosition[1] = fpsPosition.y;
        saveContent.playerPosition[2] = fpsPosition.z;
        saveContent.playingDuration = StoryDisplaying.instance.Duration;

        // set collectable objects states
        foreach (GameObject go in f_collectable)
        {
            saveContent.collectableItemsStates.Add(go.name, 1); // default collected
            if (go.activeSelf)
                saveContent.collectableItemsStates[go.name] = 0; // set not collected
            else if (go.GetComponent<LinkedWith>().link.activeSelf)
            {
                // some linked go in IAR is a set of GO (Scrolls and Puzzles). In these cases we have to get state of sublinked go with the same name
                GameObject iarLink = go.GetComponent<LinkedWith>().link;
                if (iarLink.GetComponent<LinkedWith>())
                    if (iarLink.GetComponent<LinkedWith>().link.transform.Find(go.name))
                        if (!iarLink.GetComponent<LinkedWith>().link.transform.Find(go.name).gameObject.activeSelf)
                            saveContent.collectableItemsStates[go.name] = 2;
            }
            else
                saveContent.collectableItemsStates[go.name] = 2;
        }

        // set dream fragments states
        foreach (GameObject go in f_dreamFragments)
        {
            saveContent.dreamFragmentsStates.Add(go.name, 0);
            if (go.GetComponent<DreamFragment>().viewed)
            {
                saveContent.dreamFragmentsStates[go.name] = 1;
                LinkedWith linkWith = go.GetComponent<LinkedWith>();
                if (linkWith && linkWith.link.GetComponent<NewDreamFragment>() == null) // Green fragment aren't linked with
                    saveContent.dreamFragmentsStates[go.name] = 2;
            }
        }

        saveContent.pressY_displayed = f_pressY.First().activeSelf;
        saveContent.ballbox_opened = !BallBoxManager.instance.IsLocked();
        saveContent.wireOnPlank = PlankAndWireManager.instance.IsResolved();
        saveContent.satchel_opened = !SatchelManager.instance.IsLocked();
        saveContent.mirrorOnPlank = PlankAndMirrorManager.instance.IsMirrorOnPlank();

        // set toggleables states
        foreach (GameObject go in f_toggleable)
            saveContent.toggleablesStates.Add(go.name, go.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("TurnOn"));

        // Save board texture
        Texture2D tmpTex = (Texture2D)f_boardTexture.First().GetComponent<Renderer>().material.mainTexture;
        saveContent.boardEraseTexture = tmpTex.GetRawTextureData();
        // set eraser position
        Vector3 eraserPosition = f_whiteBoard.First().transform.GetChild(2).position;
        saveContent.boardEraserPosition[0] = eraserPosition.x;
        saveContent.boardEraserPosition[1] = eraserPosition.y;
        saveContent.boardEraserPosition[2] = eraserPosition.z;

        // set queries states
        foreach (GameObject query in f_queries)
        {
            GameObject answerGO = query.transform.GetChild(3).gameObject;
            saveContent.iarQueriesStates.Add(query.name, answerGO.activeSelf);
            if (answerGO.activeSelf)
            {
                saveContent.iarQueriesAnswer.Add(query.name, answerGO.transform.GetChild(0).GetComponent<TMP_Text>().text);
                saveContent.iarQueriesDesc.Add(query.name, answerGO.transform.GetChild(1).GetComponent<TMP_Text>().text);
            }
            else
            {
                saveContent.iarQueriesAnswer.Add(query.name, "");
                saveContent.iarQueriesDesc.Add(query.name, "");
            }
        }

        // Set gears state
        if (!f_gearsSet.First().activeSelf)
            saveContent.gearEnigmaState = 0;
        else if (!IARGearsEnigma.instance.IsResolved())
            saveContent.gearEnigmaState = 1;
        else
            saveContent.gearEnigmaState = 2;
        
        // generate received hints
        foreach (GameObject hint in f_enabledHintsIAR)
        {
            HintContent hintContent = hint.GetComponent<HintContent>();
            SaveContent.HintData hintSave = new SaveContent.HintData(hintContent.monitor.id, hintContent.actionName, hintContent.text, hintContent.link, hint.GetComponent<NewHint>() == null);
            saveContent.accessibleHints.Add(hintSave);
        }

        saveContent.petriNetsMarkings = MonitoringManager.getPetriNetsMarkings();
        saveContent.hintCooldown = HelpSystem.instance.HintCooldown < 0 ? 0 : HelpSystem.instance.HintCooldown;
        saveContent.systemHintTimer = HelpSystem.instance.SystemHintTimer;
        saveContent.helpLabelCount = HelpSystem.instance.LabelCount;
        saveContent.HintDictionary = HelpSystem.instance.GameHints.dictionary;
        saveContent.HintWrongAnswerFeedbacks = HelpSystem.instance.GameHints.wrongAnswerFeedbacks;

        tmpPath = string.Concat(saveFolderPath, "/", fileName, saveFilesExtension);
        try
        {
            // Create all necessary directories if they don't exist
            Directory.CreateDirectory(Path.GetDirectoryName(tmpPath));
            File.WriteAllText(tmpPath, JsonConvert.SerializeObject(saveContent));
            return tmpPath;
        }
        catch (Exception e)
        {
            Debug.LogError("Tried to save with an invalid SaveContent. Nothing was done.");
            File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Save fails with exception: " + e + ". Nothing was done."));
        }
        return "";
    }

    // Called from UI
    public void LoadSelectedSave()
    {
        SaveContent saveContent = selectedLoadButton.GetComponent<SaveComponent>().content;
        GameObjectManager.setGameObjectState(loadPopup, false);
        popupSaveInputfield.text = selectedLoadButton.name;
        // Check save validity
        if (saveContent == null)
        {
            Debug.LogError("The save couldn't be loaded because of invalid content.");
            File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - The save couldn't be loaded because of invalid content."));
        }
        else
        {
            // Load save

            // set session ID
            if (saveContent.sessionID != "")
            {
                LoadGameContent.sessionID = saveContent.sessionID;
                if (LoadGameContent.gameContent.traceToLRS)
                    SendStatements.instance.initGBLXAPI();
            }

            // set story reading progression
            StoryDisplaying.instance.LoadStoryProgression(saveContent.storyTextCount);

            // set room progression
            f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = saveContent.lastRoomUnlocked;
            // Enable rooms
            if (saveContent.lastRoomUnlocked > 1)
            {
                GameObjectManager.setGameObjectState(f_gameRooms.First().transform.GetChild(2).gameObject, true);
                GameObjectManager.setGameObjectState(f_gameRooms.First().transform.GetChild(3).gameObject, true);
            }
            // set doors states
            if (saveContent.lastRoomUnlocked > 0)
                LockResolver.instance.UnlockIntroWall();
            if (saveContent.lastRoomUnlocked > 1)
                LoginManager.instance.UnlockLoginDoor();
            if (saveContent.lastRoomUnlocked > 2)
            {
                LockResolver.instance.UnlockRoom2Fences();
                // disable queries
                GameObjectManager.setGameObjectState(f_room2Password.First().transform.parent.GetChild(0).gameObject, false);
                // enable final code
                GameObjectManager.setGameObjectState(f_room2Password.First(), true);
            }

            // set player position
            f_fpsController.First().gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
            f_fpsController.First().gameObject.transform.position = new Vector3(saveContent.playerPosition[0], saveContent.playerPosition[1], saveContent.playerPosition[2]);

            // disable black wall at the beginning
            GameObject night = GameObject.Find("Night");
            night.GetComponent<Animator>().enabled = true;
            night.GetComponent<Collider>().enabled = false;
            // display movement HUD
            MovingSystem.instance.firstCrouchOccurs = true;
            MovingSystem.instance.SetHUD(true);
            // disable movement HUD warnings
            foreach (LinkedWith link in f_OutOfFirstRoom.First().GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(link.link, false);
            foreach (LinkedWith link in f_CrouchHint.First().GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(link.link, false);

            // set starting time
            f_timer.First().GetComponent<Timer>().startingTime = Time.time - saveContent.playingDuration;

            // set collectable objects states
            foreach (GameObject go in f_collectable)
            {
                if (saveContent.collectableItemsStates.ContainsKey(go.name))
                {
                    int code = saveContent.collectableItemsStates[go.name];
                    // if collected, disable it in the scene
                    GameObjectManager.setGameObjectState(go, code == 0);
                    GameObjectManager.setGameObjectState(go.GetComponent<LinkedWith>().link, code == 1);
                    // some linked go in IAR is a set of GO (Scrolls and Puzzles). In these cases we have to set state of sublinked go with the same name
                    GameObject iarLink = go.GetComponent<LinkedWith>().link;
                    if (code == 1 && iarLink.GetComponent<LinkedWith>() && iarLink.GetComponent<LinkedWith>().link.transform.Find(go.name))
                        GameObjectManager.setGameObjectState(iarLink.GetComponent<LinkedWith>().link.transform.Find(go.name).gameObject, true);
                }
            }

            // set dream fragments states
            bool atLeastOneFragmentClicked = false;
            foreach (GameObject go in f_dreamFragments)
            {
                if (saveContent.dreamFragmentsStates.ContainsKey(go.name))
                {
                    int code = saveContent.dreamFragmentsStates[go.name];
                    if (code != 0)
                    {
                        atLeastOneFragmentClicked = true;
                        // if collected, turn off fragment in scene
                        DreamFragmentCollecting.instance.TurnOffDreamFragment(go);
                        // enable in IAR
                        if (go.GetComponent<LinkedWith>()) // Green fragments don't contain LinkedWith component
                        {
                            tmpGO = go.GetComponent<LinkedWith>().link;
                            GameObjectManager.setGameObjectState(tmpGO, true);
                            if (code == 2)
                            {
                                // set as seen in IAR
                                tmpDFToggle = tmpGO.GetComponent<DreamFragmentToggle>();
                                tmpGO.GetComponentInChildren<Image>().sprite = tmpDFToggle.offState;
                                tmpDFToggle.currentState = tmpDFToggle.offState;
                                GameObjectManager.removeComponent<NewDreamFragment>(tmpDFToggle.gameObject);
                            }
                        }
                    }
                }
            }

            // enable IAR tab and HUD
            if (atLeastOneFragmentClicked)
            {
                if (LoadGameContent.gameContent.virtualDreamFragment)
                {
                    GameObjectManager.setGameObjectState(f_tabs.First().transform.parent.GetChild(1).gameObject, true);
                    GameObjectManager.setGameObjectState(f_fragmentNotif.First().transform.parent.gameObject, true);
                }
            }

            /*          // Normalement géré par les NewDreamFragment => à vérifier
                *          // disable dream fragment HUD warning if there are no unseen fragment
                        if (!atleastOneFragmentUnseen)
                            GameObjectManager.setGameObjectState(f_fragmentNotif.First(), false);*/

            // set pressY state
            GameObjectManager.setGameObjectState(f_pressY.First(), saveContent.pressY_displayed);

            // set Ballbox lock state
            if (saveContent.ballbox_opened)
                BallBoxManager.instance.UnlockBallBox();

            // set Wire on plank
            if (saveContent.wireOnPlank)
                PlankAndWireManager.instance.DisplayWireOnSolution();

            // set Satchel lock state
            if (saveContent.satchel_opened)
                SatchelManager.instance.UnlockSatchel();

            // set mirror on plank
            if (saveContent.mirrorOnPlank)
                PlankAndMirrorManager.instance.PutMirrorOnPlank();

            // set toggleables states
            foreach (GameObject go in f_toggleable)
                if (saveContent.toggleablesStates.ContainsKey(go.name))
                    if (saveContent.toggleablesStates[go.name])
                        go.GetComponent<Animator>().SetTrigger("turnOn");

            // load room 3 board texture
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(saveContent.boardEraseTexture);
            f_boardTexture.First().GetComponent<Renderer>().material.mainTexture = tex;
            // set eraser position
            f_whiteBoard.First().transform.GetChild(2).position = new Vector3(saveContent.boardEraserPosition[0], saveContent.boardEraserPosition[1], saveContent.boardEraserPosition[2]);

            // set queries states
            foreach (GameObject query in f_queries)
                if (saveContent.iarQueriesStates.ContainsKey(query.name) && saveContent.iarQueriesAnswer.ContainsKey(query.name) && saveContent.iarQueriesDesc.ContainsKey(query.name))
                    if (saveContent.iarQueriesStates[query.name])
                        // Toggle all childs of queries except the first one
                        for (int i = 1; i < query.transform.childCount; i++)
                        {
                            GameObject child = query.transform.GetChild(i).gameObject;
                            GameObjectManager.setGameObjectState(child, !child.activeSelf);
                            // For the third set answer and description
                            if (i == 3) {
                                child.transform.GetChild(0).GetComponent<TMP_Text>().text = saveContent.iarQueriesAnswer[query.name];
                                child.transform.GetChild(1).GetComponent<TMP_Text>().text = saveContent.iarQueriesDesc[query.name];
                            }
                        }
            
            // Set gears state
            if (saveContent.gearEnigmaState > 0)
            {
                // display gears
                GameObjectManager.setGameObjectState(f_gearsSet.First(), true);
                // Hide queries
                GameObjectManager.setGameObjectState(f_gearsSet.First().GetComponent<LinkedWith>().link, false);
                // Check if gears enigma was solved
                if (saveContent.gearEnigmaState == 2)
                    IARGearsEnigma.instance.SolveGearsEnigma();
            }

            // generate received hints
            foreach (SaveContent.HintData hint in saveContent.accessibleHints)
            {
                // find the monitoring component corresponding to this hint data
                bool monitorFound = false;
                foreach (GameObject monitor in f_componentMonitoring)
                {
                    tmpMonitorsArray = monitor.GetComponents<ComponentMonitoring>();
                    foreach (ComponentMonitoring monitoringComponent in tmpMonitorsArray)
                        if (monitoringComponent.id == hint.monitorID)
                        {
                            Button b = HelpSystem.instance.CreateHintButton(monitoringComponent, hint.name, hint.text, hint.link, true);
                            monitorFound = true;
                            if (hint.seen)
                                IARHintManager.instance.SetNormalColor(b);
                            break;
                        }
                    if (monitorFound)
                        break;
                }
            }

            // load complete and filtered petri nets markings
            if (saveContent.petriNetsMarkings != null)
                MonitoringManager.setPetriNetsMarkings(saveContent.petriNetsMarkings);

            // set HelpSystem with loaded petri nets

            HelpSystem.instance.LoadHelpSystemValues(saveContent.hintCooldown, saveContent.systemHintTimer, saveContent.helpLabelCount);
            HelpSystem.instance.GameHints.dictionary = saveContent.HintDictionary;
            HelpSystem.instance.GameHints.wrongAnswerFeedbacks = saveContent.HintWrongAnswerFeedbacks;

            GameObjectManager.addComponent<ActionPerformedForLRS>(savePopup, new
            {
                verb = "loaded",
                objectType = "serious-game",
                objectName = "E-LearningScape progression"
            });

            MenuSystem.instance.StartGame();
        }
    }

	/// <summary>
	/// Called when trying to save
	/// </summary>
	public bool CheckSaveNameValidity()
    {
		bool isValid = true;

		// remove file extension
		if (popupSaveInputfield.text.EndsWith(saveFilesExtension))
			popupSaveInputfield.text = popupSaveInputfield.text.Substring(popupSaveInputfield.text.Length - saveFilesExtension.Length, saveFilesExtension.Length);

		isValid = popupSaveInputfield.text != "";

		char[] chars = Path.GetInvalidFileNameChars();

		foreach(char c in chars)
            if (popupSaveInputfield.text.IndexOf(c) != -1)
            {
				isValid = false;
				break;
            }

		return isValid;
    }

	/// <summary>
	/// Called when changing tab or closing IAR
	/// </summary>
	public void CloseSavePopup()
    {
		GameObjectManager.setGameObjectState(savePopup, false);
		GameObjectManager.setGameObjectState(popupSaveInvalid, false);
		GameObjectManager.setGameObjectState(popupSaveOverride, false);
		GameObjectManager.setGameObjectState(popupSaveDone, false);
	}

	/// <summary>
	/// Called when mouse enters or exits the in game save button
	/// </summary>
	/// <param name="enabled"></param>
	public void SetSaveNoticeState(bool enabled)
    {
		if (!menuSaveButton.interactable)
			GameObjectManager.setGameObjectState(menuSaveButtonNotice, enabled);
        else
            GameObjectManager.setGameObjectState(menuSaveButtonNotice, false);
    }
}