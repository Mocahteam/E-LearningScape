using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class DebugModeSystem : FSystem
{
    public static FSystem instance;
    //this bool is used to switch between CheckDebugMode paused and DebugModeSystem paused
    //the "canPause" of the two systems never have the same value
    public static bool canPause = true;

    //list of KeyCode expected to stop debug mode
    public List<string> code;
    //number of consicutive keycode of the list "code" pressed
    //used to know what is the next keycode expected
    private int codeCount = 0;

    public FirstPersonController player;
    
    //variables used for the sun behaviour (randomly moving)
    public Light sun;
    private Color sunInitialColor;
    private Quaternion sunRotation;
    private float amplitude = 90f;
    private int nbFrame = 150;
    private int currentFrame;
    private Vector3 target;
    private Vector3 velocity = Vector3.zero;

    public static float initialHintCooldownDuration;

    public DebugModeSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {   
        sunInitialColor = sun.color;
        sunRotation = sun.gameObject.transform.rotation;
        currentFrame = nbFrame;
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
        player.m_WalkSpeed = player.m_WalkSpeed * 2;
        player.m_RunSpeed = player.m_RunSpeed * 2;
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

                    //set player speeds to initial speeds
                    player.m_WalkSpeed = 5;
                    player.m_RunSpeed = 5;

                    //set hint cooldown to initial value
                    HelpSystem.config.playerHintCooldownDuration = initialHintCooldownDuration;

                    //pause this system and enable CheckDebugMode
                    canPause = true;
                    instance.Pause = true;
                    CheckDebugMode.canPause = false;
                    CheckDebugMode.instance.Pause = false;
                    player.m_WalkSpeed = player.m_WalkSpeed / 2;
                    player.m_RunSpeed = player.m_RunSpeed / 2;
                }
            }
            else
            {
                //if the key pressed is not the expected key, reset codeCount and the code has to be typed again from the beginning
                codeCount = 0;
            }

            //The player can be teleported to room X by pressing Alt + X with X the room number (0 for intro and 4 for end room)
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr) || Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.RightApple))
            {
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.DoubleQuote)) // room 0
                    player.gameObject.transform.position = new Vector3(-41, -1, -1);
                else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Quote)) // room 1
                    player.gameObject.transform.position = new Vector3(-11, 2, -1);
                else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.LeftParen)) // room 2
                    player.gameObject.transform.position = new Vector3(11, 2, -2);
                else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Dollar)) // room 3
                    player.gameObject.transform.position = new Vector3(30, 2, -2);

            }
        }
    }
}