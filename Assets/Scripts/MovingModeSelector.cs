using FYFY;
using UnityEngine;

public class MovingModeSelector : MonoBehaviour
{
    public int currentState = 0;

    public GameObject FPSControl;
    public GameObject TeleportControl;
    public GameObject ClickControl;

    public Transform verticalRotation;
    public Transform horizontalRotation;

    public Animator FPSNotif;
    public Animator TeleportNotif;
    public Animator UINotif;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("ToggleTarget"))
            switchMovingMode();
    }

    void OnEnable()
    {
        resumeMovingSystems();
    }

    public void enableAppropriateHUD(bool FPS_Control, bool Teleport_Control, bool UI_Control)
    {
        GameObjectManager.setGameObjectState(FPSControl, FPS_Control);
        GameObjectManager.setGameObjectState(TeleportControl, Teleport_Control);
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
        if (MovingSystem_FPSMode.instance != null && MovingSystem_TeleportMode.instance != null && MovingSystem_UIMode.instance != null)
        {
            switch (currentState)
            {
                case 0:
                    MovingSystem_FPSMode.instance.Pause = false;
                    MovingSystem_TeleportMode.instance.Pause = true;
                    MovingSystem_UIMode.instance.Pause = true;
                    enableAppropriateHUD(true, false, false);
                    if (FPSNotif != null)
                        FPSNotif.SetTrigger("flash");
                    break;
                case 1:
                    MovingSystem_FPSMode.instance.Pause = false;
                    MovingSystem_TeleportMode.instance.Pause = false;
                    MovingSystem_UIMode.instance.Pause = true;
                    enableAppropriateHUD(false, true, false);
                    if (TeleportNotif)
                        TeleportNotif.SetTrigger("flash");
                    break;
                case 2:
                    MovingSystem_FPSMode.instance.Pause = true;
                    MovingSystem_TeleportMode.instance.Pause = true;
                    MovingSystem_UIMode.instance.Pause = false;
                    enableAppropriateHUD(false, false, true);
                    if (UINotif)
                        UINotif.SetTrigger("flash");
                    break;
            }
        }
    }

    public void setCurrentState(int newState)
    {
        currentState = Mathf.Min(Mathf.Max(newState, 0), 2);
    }


    public void switchMovingMode()
    {
        currentState = (currentState + 1) % 3;
        resumeMovingSystems();
    }
}
