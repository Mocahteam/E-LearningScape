using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class IARTabNavigation : FSystem {

    // Manage base IAR integration (Open/Close + tab switching)

    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));
    private Family f_iarDisplayed = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfComponents(typeof(PointerSensitive)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_atWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));
    private Family f_settings = FamilyManager.getFamily(new AllOfComponents(typeof(WindowNavigator)), new AnyOfTags("UIBackground"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_selectedTab = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedTab)));
    private Family f_selectedTerminal = FamilyManager.getFamily(new AnyOfTags("Terminal"), new AllOfComponents(typeof(ReadyToWork)));

    public Sprite selectedTabSprite;
    public Sprite defaultTabSprite;
    public GameObject mainHUD;
    public UnlockedRoom unlockedRoom;

    public GameObject iar;

    private int tabIdToFocusOn;

    private Dictionary<FSystem, bool> systemsStates;

    public bool skipNextClose; // enbale to skip the next IAR close (see IARQueryEvaluator)

    public static IARTabNavigation instance;

    private GameObject selectedTerminal = null;

    public IARTabNavigation()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_iarDisplayed.addEntryCallback(onIarDisplayed);
        f_selectedTerminal.addEntryCallback(onClickTerminal);

        systemsStates = new Dictionary<FSystem, bool>();
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        GameObjectManager.setGameObjectState(mainHUD, false); // hide HUD (case in the end room, see EndManager.cs)
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        GameObjectManager.setGameObjectState(mainHUD, true); // show HUD
    }

    private void onIarDisplayed(GameObject go)
    {
        // force EventSystem affectation
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(f_tabs.getAt(tabIdToFocusOn));
    }

    private void onClickTerminal(GameObject go)
    {
        selectedTerminal = go;
        openLastQuestions();
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // Open/Close IAR with Escape and A keys
        if (iar.activeInHierarchy && f_settings.Count == 0 && !skipNextClose && Input.GetButtonDown("Cancel"))
            closeIar();
        else
        {
            skipNextClose = false;
            if (!iar.activeInHierarchy)
            {
                if (Input.GetButtonDown("ToggleInventory"))
                    openIar(0); // Open IAR on the dream fragments tab if at least one fragment has been found
                else if (Input.GetButtonDown("ToggleFragments") && IARDreamFragmentManager.virtualDreamFragment && IARNewDreamFragmentAvailable.instance.firstFragmentOccurs)
                    openIar(1); // Open IAR on the last query visible tab
                else if (Input.GetButtonDown("ToggleQuestions") && (unlockedRoom.roomNumber > 0 && unlockedRoom.roomNumber < 4 || SceneManager.GetActiveScene().name.Contains("Tuto")))
                    openLastQuestions(); // Open IAR on the last query visible tab
                else if (Input.GetButtonDown("ToggleHelp") && !HelpSystem.shouldPause)
                    openIar(-2); // Open IAR on the second to last tab
                else if (Input.GetButtonDown("Cancel") && f_atWork.Count == 0) // Check not working in enigma because else Cancel action has to leave enigma and not open IAR
                    openIar(-1); // Open IAR on the last tab
            }
        }
    }

    public void openLastQuestions()
    {
        openIar(unlockedRoom.roomNumber+1); // Open IAR on the last query visible tab
    }

    public void openIar(int tabId)
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(iar, new
        {
            verb = "activated",
            objectType = "iar"
        });
        GameObjectManager.setGameObjectState(mainHUD, false); // hide HUD
        GameObjectManager.setGameObjectState(iar, true); // open IAR

        GameObjectManager.addComponent<PlaySound>(iar, new { id = 15 }); // id refer to FPSController AudioBank

        if (tabId < 0)
            tabId = f_tabs.Count + tabId;

        SwitchTab(f_tabs.getAt(tabId)); // switch to the desired tab
        tabIdToFocusOn = tabId;
        systemsStates.Clear();
        // save systems states
        foreach (FSystem sys in FSystemManager.fixedUpdateSystems())
            systemsStates[sys] = sys.Pause;
        foreach (FSystem sys in FSystemManager.updateSystems())
            systemsStates[sys] = sys.Pause;
        foreach (FSystem sys in FSystemManager.lateUpdateSystems())
            systemsStates[sys] = sys.Pause;
        // set required systems states
        MoveInFrontOf.instance.Pause = true;
        Highlighter.instance.Pause = true;
        CollectObject.instance.Pause = true;
        DreamFragmentCollecting.instance.Pause = true;
        IARDreamFragmentManager.instance.Pause = false;
        IARViewItem.instance.Pause = false;
        MovingSystem_TeleportMode.instance.Pause = true;
        MovingSystem_FPSMode.instance.Pause = true;
        MovingSystem_UIMode.instance.Pause = true;
        if (!SceneManager.GetActiveScene().name.Contains("Tuto"))
        {
            MirrorSystem.instance.Pause = true;
            ToggleObject.instance.Pause = true;
            IARGearsEnigma.instance.Pause = false;
            LockResolver.instance.Pause = true;
            PlankAndWireManager.instance.Pause = true;
            BallBoxManager.instance.Pause = true;
            LoginManager.instance.Pause = true;
            SatchelManager.instance.Pause = true;
            PlankAndMirrorManager.instance.Pause = true;
            WhiteBoardManager.instance.Pause = true;
        }
    }

    public void closeIar()
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(iar, new 
        {
            verb = "deactivated",
            objectType = "iar"
        });
        GameObjectManager.setGameObjectState(iar, false); // close IAR

        // remove selected tabs notification
        foreach (GameObject tab in f_selectedTab)
            GameObjectManager.removeComponent<SelectedTab>(tab);

        GameObjectManager.addComponent<PlaySound>(iar, new { id = 16 }); // id refer to FPSController AudioBank

        // Restaure systems state (exception for LampManager)
        bool backLampManagerState = false;
        if (LampManager.instance != null)
            backLampManagerState = LampManager.instance.Pause;
        foreach (FSystem sys in systemsStates.Keys)
            sys.Pause = systemsStates[sys];
        if (LampManager.instance != null)
            LampManager.instance.Pause = backLampManagerState;
        GameObjectManager.setGameObjectState(mainHUD, true); // show HUD

        // close save popup if it was on
        if (SaveManager.instance != null)
            SaveManager.instance.CloseSavePopup();

        //if a terminal was selected, exit it when leaving IAR
        if (selectedTerminal)
        {
            GameObjectManager.removeComponent<ReadyToWork>(selectedTerminal);
            selectedTerminal = null;
        }
    }

    public void SwitchTab(GameObject newSelectedTab)
    {
        // reset all tabs (text and image) and disable all contents
        foreach (GameObject oldTab in f_tabs)
        {
            oldTab.GetComponentInChildren<Image>().sprite = defaultTabSprite;
            oldTab.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Normal;
            GameObjectManager.setGameObjectState(oldTab.GetComponent<LinkedWith>().link, false);
        }
        // clean previous selected tabs
        foreach (GameObject tab in f_selectedTab)
            GameObjectManager.removeComponent<SelectedTab>(tab);
        // set new tab text and image
        newSelectedTab.GetComponentInChildren<Image>().sprite = selectedTabSprite;
        newSelectedTab.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
        // enable new content
        GameObjectManager.setGameObjectState(newSelectedTab.GetComponent<LinkedWith>().link, true);
        GameObjectManager.addComponent<ActionPerformedForLRS>(newSelectedTab.GetComponent<LinkedWith>().link, new 
        {
            verb = "accessed",
            objectType = "iarTab",
            activityExtensions = new Dictionary<string, string>() {
                { "value", newSelectedTab.GetComponent<LinkedWith>().link.name }
            }
        });
        // notify this tab as selected
        if (newSelectedTab.GetComponent<SelectedTab>() == null)
            GameObjectManager.addComponent<SelectedTab>(newSelectedTab);

        // close save popup if it was on
        if (SaveManager.instance != null)
            SaveManager.instance.CloseSavePopup();
    }
}