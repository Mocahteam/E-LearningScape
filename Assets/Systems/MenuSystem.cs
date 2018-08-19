using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class MenuSystem : FSystem {

    private Family buttons = FamilyManager.getFamily(new AnyOfTags("MainMenuButton"));
    private Family vlr = FamilyManager.getFamily(new AllOfComponents(typeof(VolumetricLightRenderer)));
    private Family postprocess = FamilyManager.getFamily(new AllOfComponents(typeof(PostProcessingBehaviour)));
    private Family menuCameraFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MenuCamera), typeof(Camera)));
    private Family particles = FamilyManager.getFamily(new AllOfComponents(typeof(ParticleSystem)));
    private Family gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));

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

    GameObject mainMenu;

    public MenuSystem()
    {
        if (Application.isPlaying)
        {
            //initialise menu's buttons with listeners
            foreach (GameObject b in buttons)
            {
                switch (b.name)
                {
                    case "Play":
                        b.GetComponent<Button>().onClick.AddListener(Play);
                        break;

                    case "Quit":
                        b.GetComponent<Button>().onClick.AddListener(QuitGame);
                        break;

                    default:
                        break;
                }
            }

            // Get singleton fading screen
            fadingBackground = GameObject.Find("MenuFadingBackground").GetComponent<Image>();
            // Get singleton MainMenu
            mainMenu = GameObject.Find("MainMenu");

            // Set specific quality settings
            menuCamera = menuCameraFamily.First().GetComponent<Camera>();
            if (QualitySettings.GetQualityLevel() == 0)
            {
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier1;
                foreach (GameObject vlrGo in vlr)
                    vlrGo.GetComponent<VolumetricLightRenderer>().enabled = false;
                foreach (GameObject ppGo in postprocess)
                    ppGo.GetComponent<PostProcessingBehaviour>().enabled = false;
                foreach (GameObject partGo in particles)
                    GameObjectManager.setGameObjectState(partGo, false);
            }
            else if(QualitySettings.GetQualityLevel() == 1)
            {
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier2;
                foreach (GameObject ppGo in postprocess)
                    if(ppGo.name == "FirstPersonCharacter")
                        ppGo.GetComponent<PostProcessingBehaviour>().profile = menuCamera.GetComponent<PostProcessingBehaviour>().profile;
                foreach (GameObject vlrGo in vlr)
                    vlrGo.GetComponent<VolumetricLightRenderer>().enabled = false;
                foreach (GameObject partGo in particles)
                    if(partGo.name == "Poussiere particule (1)")
                        GameObjectManager.setGameObjectState(partGo, false);
            }
            else if(QualitySettings.GetQualityLevel() == 2)
            {
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier3;
                foreach (GameObject ppGo in postprocess)
                    if(ppGo.name == "FirstPersonCharacter")
                        menuCamera.GetComponent<PostProcessingBehaviour>().profile = ppGo.GetComponent<PostProcessingBehaviour>().profile;
            }
            // set several camera positions for background animations on main menu
            menuCamera.gameObject.GetComponent<MenuCamera>().positions = new PosRot[3];
            menuCamera.gameObject.GetComponent<MenuCamera>().positions[0] = new PosRot(-20.21f, 1.42f, -1.95f, -1.516f, -131.238f, 0);
            menuCamera.gameObject.GetComponent<MenuCamera>().positions[1] = new PosRot(-10.24f,2.64f,-2.07f,1.662f,130.146f,-5.715f);
            menuCamera.gameObject.GetComponent<MenuCamera>().positions[2] = new PosRot(13.85f,1.26f,6.11f,-8.774f,131.818f,-2.642f);
            // Init timer
            switchTimer = Time.time;
        }
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        // Pause all systems except this
        foreach (FSystem syst in FSystemManager.updateSystems())
            if (syst != this)
                syst.Pause = true;
        // Set particular effects
        RenderSettings.fogDensity = 0.005f;
        // Init timer
        switchTimer = Time.time;
        // Enable MainMenu
        GameObjectManager.setGameObjectState(mainMenu, true);
        GameObjectManager.setGameObjectState(fadingBackground.gameObject, true);
        // Enable camera
        GameObjectManager.setGameObjectState(menuCameraFamily.First(), true);
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

    void Play()
    {
        this.Pause = true;
        // Disable second and third room
/*        foreach (Transform room in gameRooms.First().transform)
            if (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString()))
                GameObjectManager.setGameObjectState(room.gameObject, false);*/
        // Disable UI
        GameObjectManager.setGameObjectState(mainMenu, false);
        GameObjectManager.setGameObjectState(fadingBackground.gameObject, false);
        // Play story
        StoryDisplaying.instance.Pause = false;
    }

    void QuitGame()
    {
        Application.Quit(); //quit the game
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