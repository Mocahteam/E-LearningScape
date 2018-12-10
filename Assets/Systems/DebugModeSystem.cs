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
    public static bool canPause = true;

    private List<KeyCode> code;
    private int codeCount = 0;

    private FirstPersonController player;
    private Light sun;
    private Color sunInitialColor;
    private Quaternion sunRotation;

    private float amplitude = 90f;
    private int nbFrame = 150;
    private int currentFrame;
    private Vector3 target;
    private Vector3 velocity = Vector3.zero;

    private bool running = false;

    public DebugModeSystem()
    {
        if (Application.isPlaying)
        {
            code = new List<KeyCode>();
            code.Add(KeyCode.H);
            code.Add(KeyCode.U);
            code.Add(KeyCode.M);
            code.Add(KeyCode.A);
            code.Add(KeyCode.N);

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
            if (Input.GetKeyDown(code[codeCount]))
            {
                codeCount++;
                if (codeCount >= code.Count)
                {
                    codeCount = 0;
                    sun.color = sunInitialColor;
                    sun.gameObject.transform.rotation = sunRotation;
                    Camera.main.clearFlags = CameraClearFlags.Skybox;
                    int nb = f_door.Count;
                    for (int i = 0; i < nb; i++)
                        f_door.getAt(i).transform.position += Vector3.back * 4;
                    nb = f_wallIntro.Count;
                    for (int i = 0; i < nb; i++)
                        f_wallIntro.getAt(i).transform.position += Vector3.back * 4;
                    player.m_WalkSpeed = 5;
                    player.m_RunSpeed = 5;
                    canPause = true;
                    instance.Pause = true;
                    CheckDebugMode.canPause = false;
                    CheckDebugMode.instance.Pause = false;
                }
            }
            else
            {
                codeCount = 0;
            }

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
                    player.gameObject.transform.position = player.gameObject.transform.position;

            }
        }

        if (running)
        {
            player.m_WalkSpeed = 10;
            player.m_RunSpeed = 10;
        }
        else
        {
            player.m_WalkSpeed = 5;
            player.m_RunSpeed = 5;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            running = !running;
        }
    }
}