using UnityEngine;
using UnityEngine.UI;
using FYFY;
using System.Collections.Generic;

public class CheckDebugMode : FSystem
{
    private Family f_lights = FamilyManager.getFamily(new AllOfComponents(typeof(Light)));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));
    private Family f_dreamFragmentsToggles = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle), typeof(DreamFragmentToggle)));

    public static FSystem instance;
    //this bool is used to switch between CheckDebugMode paused and DebugModeSystem paused
    //the "canPause" of the two systems never have the same value
    public static bool canPause = false;

    //list of KeyCode expected to start debug mode
    private List<KeyCode> code;
    //number of consicutive keycode of the list "code" pressed
    //used to know what is the next keycode expected
    private int codeCount = 0;

    private Light sun;

    public CheckDebugMode()
    {
        if (Application.isPlaying)
        {
            //set the code to launch debug mode to "GODMODE"
            code = new List<KeyCode>();
            code.Add(KeyCode.G);
            code.Add(KeyCode.O);
            code.Add(KeyCode.D);
            code.Add(KeyCode.M);
            code.Add(KeyCode.O);
            code.Add(KeyCode.D);
            code.Add(KeyCode.E);

            int nbLights = f_lights.Count;
            for (int i = 0; i < nbLights; i++)
            {
                if (f_lights.getAt(i).name == "Soleil")
                {
                    //the sun of the game
                    sun = f_lights.getAt(i).GetComponent<Light>();
                    break;
                }
            }
        }
        instance = this;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        if (!canPause)
        {
            instance.Pause = false;
        }
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        if (canPause)
        {
            instance.Pause = true;
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (Input.anyKeyDown)
        {
            //when a key is pressed, check if it corresponds to the next key expected in the code
            if (Input.GetKeyDown(code[codeCount]))
            {
                //increase codeCount to check the next key in the list
                codeCount++;
                //if the keycode pressed was the last key of the list, start debug mode
                if (codeCount >= code.Count)
                {
                    codeCount = 0;

                    //change sun state
                    //sun.color = Color.green;
                    sun.gameObject.transform.rotation = Quaternion.Euler(Vector3.right * 90);
                    //Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = Color.black;

                    //unlock all IAR tabs
                    int nb = f_tabs.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if(IARDreamFragmentManager.virtualDreamFragment || f_tabs.getAt(i).name != "DreamFragments")
                            GameObjectManager.setGameObjectState(f_tabs.getAt(i), true);
                    }

                    //unlock all dream fragments in IAR
                    foreach (GameObject go in f_dreamFragmentsToggles)
                        GameObjectManager.setGameObjectState(go, true);

                    //enable room 2 and 3
                    foreach (Transform room in f_gameRooms.First().transform)
                        if (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString()))
                            GameObjectManager.setGameObjectState(room.gameObject, true);

                    f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 3;

                    //remove hint cooldown
                    DebugModeSystem.initialHintCooldownDuration = HelpSystem.config.playerHintCooldownDuration;
                    HelpSystem.config.playerHintCooldownDuration = 0;

                    //pause this system and unable DebugModeSystem
                    canPause = true;
                    instance.Pause = true;
                    DebugModeSystem.canPause = false;
                    DebugModeSystem.instance.Pause = false;
                }
            }
            else
            {
                //if the key pressed is not the expected key, reset codeCount and the code has to be typed again from the beginning
                codeCount = 0;
            }
        }
    }
}