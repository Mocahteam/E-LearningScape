using FYFY;
using UnityEngine;

public class MovingModeSelector : MonoBehaviour
{
    public int currentState = 0;

    public GameObject FPSControl;
    public GameObject ClickControl;

    public Transform verticalRotation;
    public Transform horizontalRotation;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("ToggleTarget"))
            switchMovingMode();
    }

    public void enableAppropriateHUD(bool FPS_Control, bool UI_Control)
    {
        GameObjectManager.setGameObjectState(FPSControl, FPS_Control);
        GameObjectManager.setGameObjectState(ClickControl, UI_Control);
    }

    public void pauseMovingSystems()
    {
        MovingSystem_FPSMode.instance.Pause = true;
        MovingSystem_TeleportMode.instance.Pause = true;
        MovingSystem_UIMode.instance.Pause = true;
    }

    public void resumeMovingSystems()
    {
        switch (currentState)
        {
            case 0:
                MovingSystem_FPSMode.instance.Pause = false;
                MovingSystem_TeleportMode.instance.Pause = true;
                MovingSystem_UIMode.instance.Pause = true;
                enableAppropriateHUD(true, false);
                break;
            case 1:
                MovingSystem_FPSMode.instance.Pause = false;
                MovingSystem_TeleportMode.instance.Pause = false;
                MovingSystem_UIMode.instance.Pause = true;
                enableAppropriateHUD(true, false);
                break;
            case 2:
                MovingSystem_FPSMode.instance.Pause = true;
                MovingSystem_TeleportMode.instance.Pause = true;
                MovingSystem_UIMode.instance.Pause = false;
                enableAppropriateHUD(false, true);
                // TODO : Tracer les actions de déplacement en mode UI + faire menu avant texte d'intro pour choisir son mode de déplacement
                break;
        }
    }


    public void switchMovingMode()
    {
        currentState = (currentState + 1) % 3;
        resumeMovingSystems();
    }
}
