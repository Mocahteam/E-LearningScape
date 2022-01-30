using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using FYFY_plugins.Monitoring;
using System.Collections;
using DIG.GBLXAPI.Internal;

public class MenuSystem : FSystem {

    // this system manage the first main menu

    private Family f_enabledSettingsMenu = FamilyManager.getFamily(new AllOfComponents(typeof(WindowNavigator)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    public MenuCamera menuCamera;
    public float switchDelay;
    private float switchTimer;
    private float fadingTimer = float.MinValue;
    public Image fadingBackground;
    private int displayedView = 0;
    private bool canSwitch = false;
    public float amplitude;
    public int nbFrame;
    private int currentFrame;
    private Vector3 target;
    private Vector3 velocity = Vector3.zero;

    public Transform rooms;
    public UnlockedRoom unlockedRoom;

    public GameObject mainMenu;
    public GameObject settingsMainMenu;
    public GameObject IARMenuContent;

    public static MenuSystem instance;

    public MenuSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        GameObjectManager.setGameObjectState(mainMenu, false);

        // Init timer
        switchTimer = Time.time;

        f_enabledSettingsMenu.addEntryCallback(onSettingMenuEnabled);
    }

    // WindowNavigation manages UI windows displaying but due to Fyfy delay on GameObjectManager.setGameObjectState, EventSystem doesn't display properly the current UI element. We have to select again the current UI.
    private void onSettingMenuEnabled(GameObject go)
    {
        WindowNavigator smm = go.GetComponent<WindowNavigator>();
        // force currentSelectedGameObject to be reinitialized
        GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        if (currentSelection == null || currentSelection.activeInHierarchy == false)
            EventSystem.current.SetSelectedGameObject(smm.defaultUiInWindow);
        else
            EventSystem.current.SetSelectedGameObject(currentSelection);
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        // Pause all systems except this, LogoDisplaying, SendStatements and HelpSystem
        List<FSystem> allSystems = new List<FSystem>(FSystemManager.fixedUpdateSystems());
        allSystems.AddRange(FSystemManager.updateSystems());
        allSystems.AddRange(FSystemManager.lateUpdateSystems());
        foreach (FSystem syst in allSystems)
            if (syst != this && syst != LogoDisplaying.instance && syst != SendStatements.instance && syst != HelpSystem.instance)
                syst.Pause = true;
        // Init timer
        switchTimer = Time.time;
        // Enable MainMenu
        GameObjectManager.setGameObjectState(mainMenu, true);
        GameObjectManager.setGameObjectState(fadingBackground.gameObject, true);
        // Enable camera
        GameObjectManager.setGameObjectState(menuCamera.gameObject, true);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // Background animation
        if(Time.time - switchTimer > switchDelay)
        {
            fadingTimer = Time.time;
            switchTimer = Time.time;
        }
        if(Time.time - fadingTimer < 2)
        {
            Color c = fadingBackground.color;
            fadingBackground.color = new Color(c.r, c.g, c.b, Time.time - fadingTimer);
            canSwitch = true;
        }
        else if(Time.time - fadingTimer < 4)
        {
            if (canSwitch)
            {
                displayedView++;
                if(displayedView >= menuCamera.positions.Length)
                {
                    displayedView = 0;
                }
                menuCamera.transform.position = menuCamera.positions[displayedView].position;
                menuCamera.transform.localRotation = Quaternion.Euler(menuCamera.positions[displayedView].rotation);
                canSwitch = false;
            }
            Color c = fadingBackground.color;
            fadingBackground.color = new Color(c.r, c.g, c.b, (4 - Time.time + fadingTimer)/2);
        }
        if(currentFrame > nbFrame)
        {
            currentFrame = 0;
            target = new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude));
        }
        menuCamera.transform.position = Vector3.SmoothDamp(menuCamera.transform.position, menuCamera.transform.position + target, ref velocity, 4);
        currentFrame++;
    }

    public void StartGame()
    {
        this.Pause = true;
        // Disable second and third room
        foreach (Transform room in rooms.transform)
            if (unlockedRoom.roomNumber < 2 &&
                (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString())))
                GameObjectManager.setGameObjectState(room.gameObject, false);
        // Disable UI
        GameObjectManager.setGameObjectState(mainMenu, false);
        GameObjectManager.setGameObjectState(fadingBackground.gameObject, false);

        // Link settings window to IAR, when game is playing if we open IAR Menu and then we close the setting popup we want to back in window IAR Menu and on setting button and not on setting button of main menu
        WindowNavigator wn = settingsMainMenu.GetComponent<WindowNavigator>();
        wn.parent = IARMenuContent; //parent window is MenuContent in IAR
        wn.defaultUiInParent = wn.parent.transform.GetChild(2).gameObject;

        GameObjectManager.addComponent<PlaySound>(mainMenu, new { id = 4 }); // id refer to FPSController AudioBank

        // Play story
        StoryDisplaying.instance.Pause = false;
    }

    public void QuitGame()
    {
        MainLoop.instance.StartCoroutine(DelayQuit());
    }

    private IEnumerator DelayQuit()
    {
        if (LrsRemoteQueue.Instance != null) {
            LrsRemoteQueue.Instance.flushQueuedStatements(false);
            yield return new WaitForSeconds(1);
        }
        Application.Quit();
    }

    public void RestartGame()
    {
        GameObjectManager.loadScene(SceneManager.GetActiveScene().name);
    }

    public void StartTuto()
    {
        // clean all systems instances
        MonitoringManager.Instance = null;
        SendStatements.instance = null;
        CheckDebugMode.instance = null;
        DebugModeSystem.instance = null;
        MovingSystem_FPSMode.instance = null;
        SettingsManager.instance = null;
        LoadGameContent.instance = null;
        LogoDisplaying.instance = null;
        MenuSystem.instance = null;
        StoryDisplaying.instance = null;
        SpritesAnimator.instance = null;
        MoveInFrontOf.instance = null;
        LockResolver.instance = null;
        PlankAndWireManager.instance = null;
        BallBoxManager.instance = null;
        LoginManager.instance = null;
        SatchelManager.instance = null;
        PlankAndMirrorManager.instance = null;
        LampManager.instance = null;
        WhiteBoardManager.instance = null;
        IARQueryEvaluator.instance = null;
        IARTabNavigation.instance = null;
        IARNewItemAvailable.instance = null;
        IARNewDreamFragmentAvailable.instance = null;
        IARNewHintAvailable.instance = null;
        IARNewQuestionsAvailable.instance = null;
        IARViewItem.instance = null;
        IARGearsEnigma.instance = null;
        IARHintManager.instance = null;
        IARDreamFragmentManager.instance = null;
        UIEffectPlayer.instance = null;
        HelpSystem.instance = null;
        Highlighter.instance = null;
        MirrorSystem.instance = null;
        ToggleObject.instance = null;
        CollectObject.instance = null;
        DreamFragmentCollecting.instance = null;
        SaveManager.instance = null;
        MovingSystem_TeleportMode.instance = null;
        MovingSystem_UIMode.instance = null;

        GameObjectManager.loadScene("Tutoriel");
    }
}

[System.Serializable]
public struct PosRot
{
    public Vector3 position;
    public Vector3 rotation;
}