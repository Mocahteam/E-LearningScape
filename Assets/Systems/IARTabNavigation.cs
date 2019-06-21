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
    private Family f_HUD_A = FamilyManager.getFamily(new AnyOfTags("HUD_A"));
    private Family f_HUD_H = FamilyManager.getFamily(new AnyOfTags("HUD_H"));
    private Family f_atWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));
    //Image component is added to this family in order to exclude IAR Menu tab 
    private Family f_settingsOpened = FamilyManager.getFamily(new AllOfComponents(typeof(SettingsMainMenu), typeof(Image)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_inputFieldMasterMind = FamilyManager.getFamily(new AnyOfComponents(typeof(InputField), typeof(Button)), new NoneOfLayers(5), new AnyOfTags("Login"));

    private Sprite selectedTabSprite;
    private Sprite defaultTabSprite;

    private GameObject iar;
    private GameObject iarBackground;

    private bool openedAtLeastOnce = false;

    private int tabIdToFocusOn;

    private Dictionary<FSystem, bool> systemsStates;

    public static IARTabNavigation instance;

    public IARTabNavigation()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject tab in f_tabs)
            {
                tab.GetComponent<Button>().onClick.AddListener(delegate {
                    SwitchTab(tab);
                });
            }

            selectedTabSprite = f_fgm.First().GetComponent<FocusedGOMaterial>().selectedTabSprite;
            defaultTabSprite = f_fgm.First().GetComponent<FocusedGOMaterial>().defaultTabSprite;

            f_iarDisplayed.addEntryCallback(onIarDisplayed);

            iarBackground = f_iarBackground.First();
            iar = iarBackground.transform.parent.gameObject;

            systemsStates = new Dictionary<FSystem, bool>();

        }
        instance = this;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        GameObjectManager.setGameObjectState(f_HUD_A.First(), false); // hide HUD "A"
        if (f_HUD_H.Count > 0)
            GameObjectManager.setGameObjectState(f_HUD_H.First(), false); // hide HUD "H"
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        if (openedAtLeastOnce)
        {
            GameObjectManager.setGameObjectState(f_HUD_A.First(), true); // display HUD "A"
            if (f_HUD_H.Count > 0)
                GameObjectManager.setGameObjectState(f_HUD_H.First(), true); // display HUD "H"
        }
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
        // Open/Close IAR with Escape and A keys or A and start button of xbox one controller
        if (iar.activeInHierarchy && f_settingsOpened.Count == 0 && ((Input.GetKeyDown(KeyCode.A) || Input.GetButtonDown("A_Button")) || (Input.GetKeyDown(KeyCode.H) && !HelpSystem.shouldPause) || (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Start_button")) || Input.GetButtonDown("B_Button") || (Input.GetMouseButtonDown(0) && iarBackground.GetComponent<PointerOver>())))
            closeIar();
        else if (!iar.activeInHierarchy && ((Input.GetKeyDown(KeyCode.A) || Input.GetButtonDown("A_Button")) || (Input.GetKeyDown(KeyCode.H) && !HelpSystem.shouldPause) || (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Start_button"))))
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetButtonDown("A_Button"))
                openIar(0); // Open IAR on the first tab
               
            else if (Input.GetKeyDown(KeyCode.H) && !HelpSystem.shouldPause)
                openIar(f_tabs.Count - 2); // Open IAR on the second last tab
            else
                // Open IAR on the last tab only if player doesn't work on selectable enigm (Escape enables to exit the enigm)
                if (f_atWork.Count == 0)
                    openIar(f_tabs.Count - 1); // Open IAR on the last tab
        }
    }

    private void openIar(int tabId)
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(iar, new { verb = "activated", objectType = "menu", objectName = iar.name });
        openedAtLeastOnce = true;
        GameObjectManager.setGameObjectState(f_HUD_A.First(), false); // hide HUD "A"
        if (f_HUD_H.Count > 0)
            GameObjectManager.setGameObjectState(f_HUD_H.First(), false); // hide HUD "H"
        GameObjectManager.setGameObjectState(iar, true); // open IAR
        
        //To allow a good automatic navigation keyboard in menu we disabled InputField and button component in mastermind
        foreach (GameObject inputF in f_inputFieldMasterMind)
        {
            if (inputF.GetComponent<InputField>())
                inputF.GetComponent<InputField>().enabled = false;
            if (inputF.GetComponent<Button>())
                inputF.GetComponent<Button>().enabled = false;
        }

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
        SoundEffectObjet.instance.Pause = false;
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
        //When the is no longuer stop we have to enable these component to allow the gamer to interact with mastermind object 
        foreach (GameObject inputF in f_inputFieldMasterMind)
        {
            if (inputF.GetComponent<InputField>())
                inputF.GetComponent<InputField>().enabled = true;
            if (inputF.GetComponent<Button>())
                inputF.GetComponent<Button>().enabled = true;
        }
        // Restaure systems state (exception for LampManager)
        bool backLampManagerState = LampManager.instance.Pause;
        foreach (FSystem sys in systemsStates.Keys)
            sys.Pause = systemsStates[sys];
        LampManager.instance.Pause = backLampManagerState;
        // display HUD "A"
        GameObjectManager.setGameObjectState(f_HUD_A.First(), true);
        // display HUD "H"
        if (f_HUD_H.Count > 0)
            GameObjectManager.setGameObjectState(f_HUD_H.First(), true);
    }

    private void SwitchTab(GameObject newSelectedTab)
    {
        // reset all tabs (text and image) and disable all contents
        foreach (GameObject oldTab in f_tabs)
        {
            oldTab.GetComponentInChildren<Image>().sprite = defaultTabSprite;
            oldTab.GetComponent<TMP_Text>().fontStyle = FontStyles.Normal;
            GameObjectManager.setGameObjectState(oldTab.GetComponent<LinkedWith>().link, false);
        }
        // set new tab text and image
        newSelectedTab.GetComponentInChildren<Image>().sprite = selectedTabSprite;
        newSelectedTab.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        // enable new content
        GameObjectManager.setGameObjectState(newSelectedTab.GetComponent<LinkedWith>().link, true);
        GameObjectManager.addComponent<ActionPerformedForLRS>(newSelectedTab.GetComponent<LinkedWith>().link, new { verb = "accessed", objectType = "menu", objectName = newSelectedTab.GetComponent<LinkedWith>().link.name });
    }
}