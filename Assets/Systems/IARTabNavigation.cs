using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class IARTabNavigation : FSystem {

    // Manage base IAR integration (Open/Close + tab switching)

    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));
    private Family f_fgm = FamilyManager.getFamily(new AllOfComponents(typeof(FocusedGOMaterial)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfComponents(typeof(PointerSensitive)));
    private Family f_iarDisplayed = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfComponents(typeof(PointerSensitive)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_HUD = FamilyManager.getFamily(new AnyOfTags("HUD_Main"));
    private Family f_atWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));
    private Family f_settings = FamilyManager.getFamily(new AllOfComponents(typeof(WindowNavigator)), new AnyOfTags("UIBackground"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    private Sprite selectedTabSprite;
    private Sprite defaultTabSprite;

    private GameObject iar;
    private GameObject iarBackground;

    private int tabIdToFocusOn;

    private Dictionary<FSystem, bool> systemsStates;

    public bool skipNextClose = false; // enbale to skip the next IAR close (see IARQueryEvaluator)

    public static IARTabNavigation instance;

    private UnlockedRoom unlockedRoom;

    public IARTabNavigation()
    {
        if (Application.isPlaying)
        {
            selectedTabSprite = f_fgm.First().GetComponent<FocusedGOMaterial>().selectedTabSprite;
            defaultTabSprite = f_fgm.First().GetComponent<FocusedGOMaterial>().defaultTabSprite;

            f_iarDisplayed.addEntryCallback(onIarDisplayed);

            iarBackground = f_iarBackground.First();
            iar = iarBackground.transform.parent.gameObject;

            systemsStates = new Dictionary<FSystem, bool>();

            unlockedRoom = f_unlockedRoom.First().GetComponent<UnlockedRoom>();
        }
        instance = this;
    }

    private void onIarDisplayed(GameObject go)
    {
        // force EventSystem affectation
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(f_tabs.getAt(tabIdToFocusOn));
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // Open/Close IAR with Escape and A keys
        if (iar.activeInHierarchy && f_settings.Count == 0 && !skipNextClose && (Input.GetButtonDown("Cancel") || (Input.GetButtonDown("Fire1") && iarBackground.GetComponent<PointerOver>())))
            closeIar();
        else
        {
            skipNextClose = false;
            if (!iar.activeInHierarchy)
            {
                if (Input.GetButtonDown("ToggleInventory"))
                    openIar(0); // Open IAR on the first visible tab
                else if (Input.GetButtonDown("ToggleQuestions") && unlockedRoom.roomNumber > 0 && unlockedRoom.roomNumber < 4)
                    openIar(unlockedRoom.roomNumber); // Open IAR on the last query visible tab
                else if (Input.GetButtonDown("ToggleHelp") && !HelpSystem.shouldPause)
                    openIar(f_tabs.Count - 2); // Open IAR on the second to last tab
                else if (Input.GetButtonDown("Cancel") && f_atWork.Count == 0) // Check not working in enigma because else Cancel action has to leave enigma and not open IAR
                    openIar(f_tabs.Count - 1); // Open IAR on the last tab
            }
        }
    }

    private void openIar(int tabId)
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(iar, new { verb = "activated", objectType = "menu", objectName = iar.name });
        GameObjectManager.setGameObjectState(f_HUD.First(), false); // hide HUD
        GameObjectManager.setGameObjectState(iar, true); // open IAR

        GameObjectManager.addComponent<PlaySound>(iar, new { id = 15 }); // id refer to FPSController AudioBank

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
        MovingSystem.instance.Pause = true;
        DreamFragmentCollecting.instance.Pause = true;
        Highlighter.instance.Pause = true;
        MirrorSystem.instance.Pause = true;
        ToggleObject.instance.Pause = true;
        CollectObject.instance.Pause = true;
        IARViewItem.instance.Pause = false;
        IARGearsEnigma.instance.Pause = false;
        MoveInFrontOf.instance.Pause = true;
        LockResolver.instance.Pause = true;
        PlankAndWireManager.instance.Pause = true;
        BallBoxManager.instance.Pause = true;
        LoginManager.instance.Pause = true;
        SatchelManager.instance.Pause = true;
        PlankAndMirrorManager.instance.Pause = true;
        WhiteBoardManager.instance.Pause = true;
    }

    public void closeIar()
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(iar, new { verb = "deactivated", objectType = "menu", objectName = iar.name });
        GameObjectManager.setGameObjectState(iar, false); // close IAR

        GameObjectManager.addComponent<PlaySound>(iar, new { id = 16 }); // id refer to FPSController AudioBank

        // Restaure systems state (exception for LampManager)
        bool backLampManagerState = LampManager.instance.Pause;
        foreach (FSystem sys in systemsStates.Keys)
            sys.Pause = systemsStates[sys];
        LampManager.instance.Pause = backLampManagerState;
        GameObjectManager.setGameObjectState(f_HUD.First(), true); // show HUD
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
        // set new tab text and image
        newSelectedTab.GetComponentInChildren<Image>().sprite = selectedTabSprite;
        newSelectedTab.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
        // enable new content
        GameObjectManager.setGameObjectState(newSelectedTab.GetComponent<LinkedWith>().link, true);
        GameObjectManager.addComponent<ActionPerformedForLRS>(newSelectedTab.GetComponent<LinkedWith>().link, new { verb = "accessed", objectType = "menu", objectName = newSelectedTab.GetComponent<LinkedWith>().link.name });
    }
}