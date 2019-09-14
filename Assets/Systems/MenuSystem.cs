using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MenuSystem : FSystem {

    // this system manage the first main menu

    private Family f_vlr = FamilyManager.getFamily(new AllOfComponents(typeof(VolumetricLightRenderer)));
    private Family f_postProcessBehaviour = FamilyManager.getFamily(new AllOfComponents(typeof(PostProcessingBehaviour)));
    private Family f_postProcessProfiles = FamilyManager.getFamily(new AllOfComponents(typeof(PostProcessingProfiles)));
    private Family f_menuCameraFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MenuCamera), typeof(Camera)));
    private Family f_particles = FamilyManager.getFamily(new AllOfComponents(typeof(ParticleSystem)));
    private Family f_reflectionProbe = FamilyManager.getFamily(new AllOfComponents(typeof(ReflectionProbe)));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_windowNavigator = FamilyManager.getFamily(new AllOfComponents(typeof(WindowNavigator)));
    private Family f_enabledSettingsMenu = FamilyManager.getFamily(new AllOfComponents(typeof(WindowNavigator)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Camera menuCamera;
    private float switchDelay = 12;
    private float switchTimer;
    private float fadingTimer = float.MinValue;
    private Image fadingBackground;
    private int displayedView = 0;
    private bool canSwitch = false;
    private float amplitude = 0.2f;
    private int nbFrame = 60;
    private int currentFrame;
    private Vector3 target;
    private Vector3 velocity = Vector3.zero;

    Toggle togglePuzzle;
    GameObject mainMenu;

    public static MenuSystem instance;

    public MenuSystem()
    {
        if (Application.isPlaying)
        {
            // Get singleton fading screen
            fadingBackground = GameObject.Find("MenuFadingBackground").GetComponent<Image>();
            // Get singleton MainMenu
            mainMenu = GameObject.Find("MainMenu");
            GameObjectManager.setGameObjectState(mainMenu, false);

            // Set specific quality settings
            menuCamera = f_menuCameraFamily.First().GetComponent<Camera>();
            if (QualitySettings.GetQualityLevel() == 0)
            {
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier1;
                foreach (GameObject vlrGo in f_vlr)
                    vlrGo.GetComponent<VolumetricLightRenderer>().enabled = false;
                foreach (GameObject ppGo in f_postProcessBehaviour)
                    if (ppGo.name == "FirstPersonCharacter")
                        ppGo.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[0];
                menuCamera.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[0];
                // disable reflect in first room
                GameObjectManager.setGameObjectState(f_reflectionProbe.First(), false);
                // disable particles except on DreamFragment
                foreach (GameObject partGo in f_particles)
                    if (!partGo.GetComponentInParent<DreamFragment>())
                        GameObjectManager.setGameObjectState(partGo, false);
            }
            else if(QualitySettings.GetQualityLevel() == 1)
            {
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier2;
                foreach (GameObject vlrGo in f_vlr)
                    vlrGo.GetComponent<VolumetricLightRenderer>().enabled = false;
                foreach (GameObject ppGo in f_postProcessBehaviour) // use for the First Person Character post process of the main Menu camera
                    if(ppGo.name == "FirstPersonCharacter")
                        ppGo.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[1];
                menuCamera.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[1];
                foreach (GameObject partGo in f_particles)
                    if(partGo.name.Contains("Poussiere particule")) // disable particles of the first room
                        GameObjectManager.setGameObjectState(partGo, false);
            }
            else if(QualitySettings.GetQualityLevel() == 2)
            {
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier3;
                foreach (GameObject ppGo in f_postProcessBehaviour)
                    if(ppGo.name == "FirstPersonCharacter") // use for the main Menu camera post process of the First Person Character
                        ppGo.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[2];
                menuCamera.GetComponent<PostProcessingBehaviour>().profile = f_postProcessProfiles.First().GetComponent<PostProcessingProfiles>().bank[2];
            }
            // set several camera positions for background animations on main menu
            menuCamera.gameObject.GetComponent<MenuCamera>().positions = new PosRot[3];
            menuCamera.gameObject.GetComponent<MenuCamera>().positions[0] = new PosRot(-20.21f, 1.42f, -1.95f, -1.516f, -131.238f, 0);
            menuCamera.gameObject.GetComponent<MenuCamera>().positions[1] = new PosRot(-10.24f,2.64f,-2.07f,1.662f,130.146f,-5.715f);
            menuCamera.gameObject.GetComponent<MenuCamera>().positions[2] = new PosRot(13.85f,1.26f,6.11f,-8.774f,131.818f,-2.642f);

            // Init timer
            switchTimer = Time.time;

            f_enabledSettingsMenu.addEntryCallback(onSettingMenuEnabled);
        }

        instance = this;
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
        GameObjectManager.setGameObjectState(f_menuCameraFamily.First(), true);
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
                if(displayedView >= menuCamera.gameObject.GetComponent<MenuCamera>().positions.Length)
                {
                    displayedView = 0;
                }
                menuCamera.transform.position = menuCamera.gameObject.GetComponent<MenuCamera>().positions[displayedView].position;
                menuCamera.transform.localRotation = Quaternion.Euler(menuCamera.gameObject.GetComponent<MenuCamera>().positions[displayedView].rotation);
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
        foreach (Transform room in f_gameRooms.First().transform)
            if (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString()))
                GameObjectManager.setGameObjectState(room.gameObject, false);
        // Disable UI
        GameObjectManager.setGameObjectState(mainMenu, false);
        GameObjectManager.setGameObjectState(fadingBackground.gameObject, false);

        // Link settings window to IAR, when game is playing if we open IAR Menu and then we close the setting popup we want to back in window IAR Menu and on setting button and not on setting button of main menu
        GameObject settingsMainMenu = null;
        GameObject IARMenuContent = null;
        foreach (GameObject go in f_windowNavigator)
        {
            if (go.name == "Settings_MainMenu")
                settingsMainMenu = go;
            if (go.name == "MenuContent")
                IARMenuContent = go;
        }
        if (settingsMainMenu != null && IARMenuContent != null)
        {
            WindowNavigator wn = settingsMainMenu.GetComponent<WindowNavigator>();
            wn.parent = IARMenuContent; //parent window is MenuContent in IAR
            wn.defaultUiInParent = wn.parent.transform.GetChild(2).gameObject;
        }

        GameObjectManager.addComponent<PlaySound>(mainMenu, new { id = 4 }); // id refer to FPSController AudioBank

        // Play story
        StoryDisplaying.instance.Pause = false;
    }
}

public struct PosRot
{
    public Vector3 position;
    public Vector3 rotation;

    public PosRot(Vector3 pos, Vector3 rot)
    {
        position = pos;
        rotation = rot;
    }

    public PosRot(float posx, float posy, float posz, float rotx, float roty, float rotz)
    {
        position = new Vector3(posx, posy, posz);
        rotation = new Vector3(rotx, roty, rotz);
    }
}