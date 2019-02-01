using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class DebugModeSystem : FSystem
{
    private Family f_lights = FamilyManager.getFamily(new AllOfComponents(typeof(Light)));
    private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));
    private Family f_wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));
    private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));

    public static FSystem instance;
    //this bool is used to switch between CheckDebugMode paused and DebugModeSystem paused
    //the "canPause" of the two systems never have the same value
    public static bool canPause = true;

    //list of KeyCode expected to stop debug mode
    private List<KeyCode> code;
    //number of consicutive keycode of the list "code" pressed
    //used to know what is the next keycode expected
    private int codeCount = 0;

    private FirstPersonController player;
    
    //variables used for the sun behaviour (randomly moving)
    private Light sun;
    private Color sunInitialColor;
    private Quaternion sunRotation;
    private float amplitude = 90f;
    private int nbFrame = 150;
    private int currentFrame;
    private Vector3 target;
    private Vector3 velocity = Vector3.zero;

    //In debug mode the player can run by pressing Shift
    //This bool is true when the player is running
    private bool running = false;

    public DebugModeSystem()
    {
        if (Application.isPlaying)
        {
            //set the code to stop debug mode to "HUMAN"
            code = new List<KeyCode>();
            code.Add(KeyCode.H);
            code.Add(KeyCode.U);
            code.Add(KeyCode.M);
            code.Add(KeyCode.A);
            code.Add(KeyCode.N);

            //set player and sun variables
            player = f_player.First().GetComponent<FirstPersonController>();
            int nbLights = f_lights.Count;
            for (int i = 0; i < nbLights; i++)
            {
                if (f_lights.getAt(i).name == "Soleil")
                {
                    sun = f_lights.getAt(i).GetComponent<Light>();
                    break;
                }
            }
            sunInitialColor = sun.color;
            sunRotation = sun.gameObject.transform.rotation;
            currentFrame = nbFrame;
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
        //move the sun
        if (currentFrame > nbFrame)
        {
            currentFrame = 0;
            amplitude = 75 + Random.value * 40;
            target = new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude));
        }
        sun.gameObject.transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(sun.gameObject.transform.rotation.eulerAngles, sun.gameObject.transform.rotation.eulerAngles + target, ref velocity, 4));
        currentFrame++;

        if (Input.anyKeyDown)
        {
            //when a key is pressed, check if it corresponds to the next key expected in the code
            if (Input.GetKeyDown(code[codeCount]))
            {
                //increase codeCount to check the next key in the list
                codeCount++;
                //if the keycode pressed was the last key of the list, stop debug mode
                if (codeCount >= code.Count)
                {
                    codeCount = 0;

                    //set sun state to the initial state
                    sun.color = sunInitialColor;
                    sun.gameObject.transform.rotation = sunRotation;
                    Camera.main.clearFlags = CameraClearFlags.Skybox;

                    //set the doors to their initial position (moving them to the right)
                    int nb = f_door.Count;
                    for (int i = 0; i < nb; i++)
                        f_door.getAt(i).transform.position += Vector3.back * 4;
                    nb = f_wallIntro.Count;
                    for (int i = 0; i < nb; i++)
                        f_wallIntro.getAt(i).transform.position += Vector3.back * 4;

                    //set player speeds to initial speeds
                    player.m_WalkSpeed = 5;
                    player.m_RunSpeed = 5;

                    //pause this system and enable CheckDebugMode
                    canPause = true;
                    instance.Pause = true;
                    CheckDebugMode.canPause = false;
                    CheckDebugMode.instance.Pause = false;
                }
            }
            else
            {
                //if the key pressed is not the expected key, reset codeCount and the code has to be typed again from the beginning
                codeCount = 0;
            }

            //The player can be teleported to room X by pressing Ctrl + X with X the room number (0 for intro and 4 for end room)
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                    player.gameObject.transform.position = new Vector3(-41, -1, -1);
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                    player.gameObject.transform.position = new Vector3(-11, 2, -1);
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    player.gameObject.transform.position = new Vector3(11, 2, -2);
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    player.gameObject.transform.position = new Vector3(30, 2, -2);
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    /* TODO:
                     * _disable all rooms and enable end room, then teleport the player to the end room
                     * _set a bool to true and when the player teleports to another room, if this bool is true, disable end room, enable all other rooms and teleport the player
                     */
                    player.gameObject.transform.position = player.gameObject.transform.position;

            }
        }

        if (running)
        {
            //if the player is running increase its standing and crouching speed
            player.m_WalkSpeed = 10;
            player.m_RunSpeed = 10;
        }
        else
        {
            //if the player is not running set its standing and crouching speed to normal
            //in debug mode the player is not slowed when crouching
            player.m_WalkSpeed = 5;
            player.m_RunSpeed = 5;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //check if Shift key is pressed and switch state between running and walking
            running = !running;
        }
    }
}