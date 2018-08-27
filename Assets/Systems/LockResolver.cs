using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;

public class LockResolver : FSystem {

    // THis system manages lockers with three wheels

    // all selectable locker
    private Family f_lockers = FamilyManager.getFamily(new AllOfComponents(typeof(Locker)));
    private Family f_focusedLocker = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(ReadyToWork), typeof(Locker), typeof(AudioBank)));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_closeLock = FamilyManager.getFamily(new AnyOfTags("LockIntro", "LockIntroWheel", "LockRoom2", "LockR2Wheel", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));

    private Family f_audioSourceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)), new AnyOfTags("GameRooms"));
    private Family f_wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));
    private Family f_fences = FamilyManager.getFamily(new AnyOfTags("Fence"));

    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    //information for animations
    private float speed;
    private float speedRotation;
    private float oldDT;
    private Vector3 tmpTargetPosition;
    private float angleCount = 0;

    //locker
    private Locker selectedLocker = null;
    private GameObject selectedWheel;
    private bool lockRotationUp = false;
    private bool lockRotationDown = false;
    private Color lockWheelColor;
    private Vector3 lockNumbers = Vector3.zero;
    private float wheelRotationCount = 0;

    private bool room1Unlocked = false;
    private bool room3Unlocked = false;
    private bool IARScreenRoom1Unlocked = false;
    private bool IARScreenRoom3Unlocked = false;
    private bool playerLookingAtDoor = false;
    private AudioSource gameAudioSource;

    public static LockResolver instance;

    public LockResolver()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject go in f_lockers)
            {
                Locker locker = go.GetComponent<Locker>();
                lockWheelColor = locker.Wheel1.GetComponent<Renderer>().material.color;
                // Add listener on UI arrows to control the lock 
                foreach (Transform child in go.transform)
                {
                    if (child.gameObject.name == "LeftRight")
                    {
                        foreach (Transform c in child)
                        {
                            if (c.gameObject.name == "Left")
                            {
                                c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                    SelectLeftWheel(ref selectedWheel, locker.Wheel1, locker.Wheel2, locker.Wheel3, locker.UpDownControl, lockRotationUp, lockRotationDown);
                                });
                            }
                            else if (c.gameObject.name == "Right")
                            {
                                c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                    SelectRightWheel(ref selectedWheel, locker.Wheel1, locker.Wheel2, locker.Wheel3, locker.UpDownControl, lockRotationUp, lockRotationDown);
                                });
                            }
                        }
                    }
                    else if (child.gameObject.name == "UpDown")
                    {
                        foreach (Transform c in child)
                        {
                            if (c.gameObject.name == "Up")
                            {
                                c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                    moveWheelUp(selectedWheel, ref lockNumbers, locker.Wheel1, locker.Wheel2, locker.Wheel3, ref lockRotationUp, ref lockRotationDown);
                                });
                            }
                            else if (c.gameObject.name == "Down")
                            {
                                c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                    moveWheelDown(selectedWheel, ref lockNumbers, locker.Wheel1, locker.Wheel2, locker.Wheel3, ref lockRotationUp, ref lockRotationDown);
                                });
                            }
                        }
                    }
                }
            }
            gameAudioSource = f_audioSourceFamily.First().GetComponent<AudioSource>();

            f_focusedLocker.addEntryCallback(onReadyToWorkOnLocker);
        }
        instance = this;
    }

    private void onReadyToWorkOnLocker(GameObject go)
    {
        selectedLocker = go.GetComponent<Locker>();
        // Enable UI arrows
        GameObjectManager.setGameObjectState(selectedLocker.LeftRightControl, true);
        GameObjectManager.setGameObjectState(selectedLocker.UpDownControl, true);
        // Change selected wheel color and move Up/Down UI over the selected wheel
        selectedWheel = selectedLocker.Wheel2;
        selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
        selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
        // activate this system
        instance.Pause = false;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        speed = 8f * Time.deltaTime;
        speedRotation *= Time.deltaTime / oldDT;
        oldDT = Time.deltaTime;

        // Do we are in front of the locker
        if (selectedLocker)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed
            if (((f_closeLock.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)) && (!room1Unlocked || IARScreenRoom1Unlocked) && (!room3Unlocked || IARScreenRoom3Unlocked))
                ExitLocker();
            else if (Input.GetMouseButtonDown(0))
            {
                // Select the clicked wheel
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                if (selectedLocker.Wheel1.GetComponent<PointerOver>())
                    selectedWheel = selectedLocker.Wheel1;
                else if (selectedLocker.Wheel2.GetComponent<PointerOver>())
                    selectedWheel = selectedLocker.Wheel2;
                else if (selectedLocker.Wheel3.GetComponent<PointerOver>())
                    selectedWheel = selectedLocker.Wheel3;
                // Change selected wheel color and move Up/Down UI over the selected wheel
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
            }

            // process hotkeys to move the wheels
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z))
                moveWheelUp(selectedWheel, ref lockNumbers, selectedLocker.Wheel1, selectedLocker.Wheel2, selectedLocker.Wheel3, ref lockRotationUp, ref lockRotationDown);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                moveWheelDown(selectedWheel, ref lockNumbers, selectedLocker.Wheel1, selectedLocker.Wheel2, selectedLocker.Wheel3, ref lockRotationUp, ref lockRotationDown);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
                SelectLeftWheel(ref selectedWheel, selectedLocker.Wheel1, selectedLocker.Wheel2, selectedLocker.Wheel3, selectedLocker.UpDownControl, lockRotationUp, lockRotationDown);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                SelectRightWheel(ref selectedWheel, selectedLocker.Wheel1, selectedLocker.Wheel2, selectedLocker.Wheel3, selectedLocker.UpDownControl, lockRotationUp, lockRotationDown);
        }

        // Do we have to rotate wheel up or down?
        if (lockRotationUp || lockRotationDown)
        {
            if (lockRotationUp)
                selectedWheel.transform.Rotate(Time.deltaTime * 200, 0, 0);
            else
                selectedWheel.transform.Rotate(-Time.deltaTime * 200, 0, 0);
            wheelRotationCount += Time.deltaTime * 200;

            // is rotation finished ?
            if (wheelRotationCount > 36)
            {
                if (lockRotationUp)
                {
                    selectedWheel.transform.Rotate(36 - wheelRotationCount, 0, 0);
                    lockRotationUp = false;
                }
                else
                {
                    selectedWheel.transform.Rotate(-(36 - wheelRotationCount), 0, 0);
                    lockRotationDown = false;
                }
                wheelRotationCount = 0;
                // Check if the solution is found
                if (lockNumbers.x == selectedLocker.wheel1Solution && lockNumbers.y == selectedLocker.wheel2Solution && lockNumbers.z == selectedLocker.wheel3Solution)
                {
                    tmpTargetPosition = f_player.First().transform.position + Vector3.back * 3;
                    // depending of locker => unlock the right room
                    if (selectedLocker.tag == "LockIntro")
                    {
                        room1Unlocked = true;
                        if (HelpSystem.monitoring)
                        {
                            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(4), "perform");
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                            HelpSystem.traces.Enqueue(trace);
                            trace = new MonitoringTrace(MonitoringManager.getMonitorById(21), "perform");
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                            HelpSystem.traces.Enqueue(trace);
                        }
                    }
                    else
                        room3Unlocked = true;
                    lockNumbers = Vector3.zero;
                }
            }
        }

        speed = 50 * Time.deltaTime;

        // Do we have to unlock first room and associated IAR screen
        if (room1Unlocked && !IARScreenRoom1Unlocked)
        {
            // the player has to look at animated wall
            if (!playerLookingAtDoor)
            {
                // straf right
                f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, tmpTargetPosition, 4 * Time.deltaTime);
                // check if animation is over
                if (f_player.First().transform.position == tmpTargetPosition)
                {
                    gameAudioSource.clip = selectedLocker.GetComponent<AudioBank>().audioBank[0];
                    gameAudioSource.PlayDelayed(0);
                    gameAudioSource.loop = true;
                    playerLookingAtDoor = true;
                    tmpTargetPosition = f_wallIntro.First().transform.position + Vector3.up * (-4f - f_wallIntro.First().transform.position.y);
                }
            }
            else
            {
                // animate the wall
                f_wallIntro.First().transform.position = Vector3.MoveTowards(f_wallIntro.First().transform.position, tmpTargetPosition, 0.1f * speed);
                // Check if animation is over
                if (f_wallIntro.First().transform.position == tmpTargetPosition)
                {
                    // disable the wall
                    GameObjectManager.setGameObjectState(f_wallIntro.First(), false);
                    gameAudioSource.loop = false; // stop sound
                    // update IAR
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(0).gameObject, false); // first child is locked tab
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(1).gameObject, true); // second child is unlocked tab
                    // update flags
                    playerLookingAtDoor = false;
                    IARScreenRoom1Unlocked = true;
                    // disable UI items usable for this enigm
                    if (selectedLocker.GetComponent<LinkedWith>())
                        GameObjectManager.setGameObjectState(selectedLocker.GetComponent<LinkedWith>().link, false);
                    // Exit the locker
                    ExitLocker();
                }
            }
        }

        if (room3Unlocked && !IARScreenRoom3Unlocked)
        {
            if (!playerLookingAtDoor)
            {
                f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, tmpTargetPosition, 4 * Time.deltaTime);
                if (f_player.First().transform.position == tmpTargetPosition)
                {
                    gameAudioSource.clip = selectedLocker.GetComponent<AudioBank>().audioBank[0];
                    gameAudioSource.PlayDelayed(0);
                    gameAudioSource.loop = true;
                    playerLookingAtDoor = true;
                }
            }
            else
            {
                f_fences.getAt(0).transform.Rotate(0, 0, -Time.deltaTime * 100);
                f_fences.getAt(1).transform.Rotate(0, 0, Time.deltaTime * 100);
                angleCount += Time.deltaTime * 100;
                if (angleCount > 103)
                {
                    f_fences.getAt(0).transform.Rotate(0, 0, -(103 - angleCount));
                    f_fences.getAt(1).transform.Rotate(0, 0, 103 - angleCount);
                    angleCount = 0;
                    playerLookingAtDoor = false;
                    gameAudioSource.loop = false;
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(0).gameObject, false); // first child is locked tab
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(1).gameObject, true); // second child is unlocked tab
                    IARScreenRoom3Unlocked = true;
                    ExitLocker();
                }
            }
        }
    }

    private void ExitLocker()
    {
        // Hide UI
        GameObjectManager.setGameObjectState(selectedLocker.LeftRightControl, false);
        GameObjectManager.setGameObjectState(selectedLocker.UpDownControl, false);
        // Reset default wheel color
        selectedLocker.Wheel1.GetComponent<Renderer>().material.color = lockWheelColor;
        selectedLocker.Wheel2.GetComponent<Renderer>().material.color = lockWheelColor;
        selectedLocker.Wheel3.GetComponent<Renderer>().material.color = lockWheelColor;

        if (HelpSystem.monitoring)
        {
            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(3), "turnOff");
            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
            HelpSystem.traces.Enqueue(trace);
        }

        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLocker.gameObject);

        selectedLocker = null;

        // disable this system
        instance.Pause = true;
    }

    private void moveWheelUp(GameObject wheel, ref Vector3 numbers, GameObject wheel1, GameObject wheel2, GameObject wheel3, ref bool rotationUp, ref bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            rotationUp = true;
            if (wheel.GetInstanceID() == wheel1.GetInstanceID())
            {
                if(numbers.x == 9)
                {
                    numbers = new Vector3(0, numbers.y, numbers.z);
                }
                else
                {
                    numbers += Vector3.right;
                }
            }
            else if (wheel.GetInstanceID() == wheel2.GetInstanceID())
            {
                if (numbers.y == 9)
                {
                    numbers = new Vector3(numbers.x, 0, numbers.z);
                }
                else
                {
                    numbers += Vector3.up;
                }
            }
            else if (wheel.GetInstanceID() == wheel3.GetInstanceID())
            {
                if (numbers.z == 9)
                {
                    numbers = new Vector3(numbers.x, numbers.y, 0);
                }
                else
                {
                    numbers += Vector3.forward;
                }
            }
        }
    }

    private void moveWheelDown(GameObject wheel, ref Vector3 numbers, GameObject wheel1, GameObject wheel2, GameObject wheel3, ref bool rotationUp, ref bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            rotationDown = true;
            if (wheel.GetInstanceID() == wheel1.GetInstanceID())
            {
                if (numbers.x == 0)
                {
                    numbers = new Vector3(9, numbers.y, numbers.z);
                }
                else
                {
                    numbers += Vector3.left;
                }
            }
            else if (wheel.GetInstanceID() == wheel2.GetInstanceID())
            {
                if (numbers.y == 0)
                {
                    numbers = new Vector3(numbers.x, 9, numbers.z);
                }
                else
                {
                    numbers += Vector3.down;
                }
            }
            else if (wheel.GetInstanceID() == wheel3.GetInstanceID())
            {
                if (numbers.z == 0)
                {
                    numbers = new Vector3(numbers.x, numbers.y, 9);
                }
                else
                {
                    numbers += Vector3.back;
                }
            }
        }
    }

    private void SelectLeftWheel(ref GameObject wheel, GameObject wheel1, GameObject wheel2, GameObject wheel3, GameObject lockUDUI, bool rotationUp, bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            if (wheel.GetInstanceID() == wheel2.GetInstanceID())
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel1;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
            else if (wheel.GetInstanceID() == wheel3.GetInstanceID())
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel2;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
        }
    }

    private void SelectRightWheel(ref GameObject wheel, GameObject wheel1, GameObject wheel2, GameObject wheel3, GameObject lockUDUI, bool rotationUp, bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            if (wheel.GetInstanceID() == wheel1.GetInstanceID())
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel2;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
            else if (wheel.GetInstanceID() == wheel2.GetInstanceID())
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel3;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
        }
    }
}