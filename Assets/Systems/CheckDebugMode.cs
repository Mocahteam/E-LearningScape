using UnityEngine;
using FYFY;
using System.Collections.Generic;

public class CheckDebugMode : FSystem
{

    private Family f_lights = FamilyManager.getFamily(new AllOfComponents(typeof(Light)));
    private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));
    private Family f_wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"));

    public static FSystem instance;
    public static bool canPause = false;

    private List<KeyCode> code;
    private int codeCount = 0;

    private Light sun;

    public CheckDebugMode()
    {
        if (Application.isPlaying)
        {
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
            if (Input.GetKeyDown(code[codeCount]))
            {
                codeCount++;
                if (codeCount >= code.Count)
                {
                    codeCount = 0;
                    //sun.color = Color.green;
                    sun.gameObject.transform.rotation = Quaternion.Euler(Vector3.right * 90);
                    //Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = Color.black;
                    int nb = f_door.Count;
                    for (int i = 0; i < nb; i++)
                        f_door.getAt(i).transform.position += Vector3.forward * 4;
                    nb = f_wallIntro.Count;
                    for (int i = 0; i < nb; i++)
                        f_wallIntro.getAt(i).transform.position += Vector3.forward * 4;
                    nb = f_tabs.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (f_tabs.getAt(i).name == "Unlocked")
                        {
                            GameObjectManager.setGameObjectState(f_tabs.getAt(i).transform.parent.GetChild(0).gameObject, false);
                            GameObjectManager.setGameObjectState(f_tabs.getAt(i), true);
                        }
                    }
                    foreach (Transform room in f_gameRooms.First().transform)
                        if (room.gameObject.name.Contains(2.ToString()) || room.gameObject.name.Contains(3.ToString()))
                            GameObjectManager.setGameObjectState(room.gameObject, true);
                    canPause = true;
                    instance.Pause = true;
                    DebugModeSystem.canPause = false;
                    DebugModeSystem.instance.Pause = false;
                }
            }
            else
            {
                codeCount = 0;
            }
        }
    }
}