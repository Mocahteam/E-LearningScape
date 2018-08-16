using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;

public class LockResolver : FSystem {

    // all selectable locker
    private Family lockers = FamilyManager.getFamily(new AllOfComponents(typeof(Locker)));
    private Family focusedLocker = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Highlighted), typeof(Locker)));

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family closeLock = FamilyManager.getFamily(new AnyOfTags("LockIntro", "LockIntroWheel", "LockRoom2", "LockR2Wheel", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));

    private Family audioSourceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)), new AnyOfTags("GameRooms"));
    private Family wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));
    private Family fences = FamilyManager.getFamily(new AnyOfTags("Fence"));

    //information for animations
    private float speed;
    private float speedRotation;
    private float oldDT;
    private float dist = -1;
    private Vector3 camNewDir;
    private Vector3 newDir;
    private Vector3 playerLocalScale;
    private Vector3 tmpTarget;
    private float angleCount = 0;

    //locker
    private Locker selectedLocker;
    private bool onLock = false;
    private bool moveToLock = false;
    private Vector3 lockPos;
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

    private int nb;
    private GameObject lockerGO;

    public static LockResolver instance;

    public LockResolver()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject go in lockers)
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
            gameAudioSource = audioSourceFamily.First().GetComponent<AudioSource>();
        }
        instance = this;
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

        if (Input.GetMouseButtonDown(0))
        {
            lockerGO = focusedLocker.First();
            if (lockerGO)
            {
                // pause unuse systems
                MovingSystem.instance.Pause = true;
                Highlighter.instance.Pause = true;
                DreamFragmentCollecting.instance.Pause = true;
                IARTabNavigation.instance.Pause = true;
                IARNewItemAvailable.instance.Pause = true;
                ToggleObject.instance.Pause = true;
                CollectObject.instance.Pause = true;

                // save player scale (crouch or not) in order to reset it when player exit the locker
                playerLocalScale = player.First().transform.localScale;
                // be sure the player standing in front of the locker
                player.First().transform.localScale = Vector3.one;
                // compute position in front of the second wheel
                selectedLocker = lockerGO.GetComponent<Locker>();
                lockPos = new Vector3(selectedLocker.Wheel2.transform.position.x - 2.5f, selectedLocker.Wheel2.transform.position.y - 0.9f, selectedLocker.Wheel2.transform.position.z);
                // compute distance between the player and the locker and compute speed rotation
                speedRotation = Vector3.Angle(Camera.main.transform.forward, Vector3.right) * speed / dist;
                moveToLock = true; // start animation to move the player in front of the lock
                onLock = true; // the player is captured by the lock
                if (HelpSystem.monitoring)
                {
                    MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(3), "turnOn");
                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                    HelpSystem.traces.Enqueue(trace);
                }
            }
        }

        // Do we have to process animation to move the player in front of the lock ?
        if (moveToLock)
        {
            // move the player in front of the lock
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, lockPos, speed);
            camNewDir = new Vector3(-1, 0, 0);// Vector3.right;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            // Check if the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, newDir) < 0.5f && player.First().transform.position == lockPos)
            {
                // Correct the rotation
                newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.right, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, Vector3.right, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                // Enable UI arrows
                GameObjectManager.setGameObjectState(selectedLocker.LeftRightControl, true);
                GameObjectManager.setGameObjectState(selectedLocker.UpDownControl, true);
                // Change selected wheel color and move Up/Down UI over the selected wheel
                selectedWheel = selectedLocker.Wheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
                // animation is over
                moveToLock = false;
            }
        }

        // De we are in front of the locker
        if (onLock && !moveToLock)
        {
            //"close" ui (give back control to the player) when the correct password is entered or when clicking on nothing
            if ((closeLock.Count == 0 && Input.GetMouseButtonDown(0)) || Input.GetKeyDown(KeyCode.Escape))
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
            Debug.Log("wheelRotationCount: " + wheelRotationCount);
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
                Debug.Log(lockNumbers.x + " " + lockNumbers.y + " " + lockNumbers.z + " / " + selectedLocker.wheel1Solution + " " + selectedLocker.wheel2Solution + " " + selectedLocker.wheel3Solution);
                // Check if the solution is found
                if (lockNumbers.x == selectedLocker.wheel1Solution && lockNumbers.y == selectedLocker.wheel2Solution && lockNumbers.z == selectedLocker.wheel3Solution)
                {
                    selectedLocker.GetComponent<Selectable>().solved = true;
                    // depending of locker => unlock the right room
                    if (selectedLocker.tag == "LockIntro")
                    {
                        Debug.Log("Unlock room 1");
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
                    {
                        Debug.Log("Unlock room 3");
                        room3Unlocked = true;
                    }
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
                // if not => process animation
                tmpTarget = wallIntro.First().transform.position + Vector3.up - Camera.main.transform.position;
                Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180 * speed, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                // check if animation is over
                if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                {
                    // correct the rotation
                    //++++
                    newDir = Vector3.RotateTowards(player.First().transform.forward, tmpTarget, 360, 0);
                    player.First().transform.rotation = Quaternion.LookRotation(newDir);
                    newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, 360, 0);
                    Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                    //++++
                    //Camera.main.transform.forward = tmpTarget;
                    //player.First().transform.rotation = Quaternion.LookRotation(newDir);
                    // Play sound
                    gameAudioSource.clip = selectedLocker.GetComponent<Selectable>().right;
                    gameAudioSource.PlayDelayed(0);
                    gameAudioSource.loop = true;
                    playerLookingAtDoor = true;
                    tmpTarget = wallIntro.First().transform.position + Vector3.up * (-4f - wallIntro.First().transform.position.y);
                }













                /*// move the player in front of the lock
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, lockPos, speed);
                camNewDir = Vector3.right;
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                // Check if the animation is finished
                if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && player.First().transform.position == lockPos)
                {
                    // Correct the rotation
                    newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.right, 360, 0);
                    player.First().transform.rotation = Quaternion.LookRotation(newDir);
                    newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                    Camera.main.transform.rotation = Quaternion.LookRotation(newDir);



                }*/
            }
            else
            {
                // animate the wall
                wallIntro.First().transform.position = Vector3.MoveTowards(wallIntro.First().transform.position, tmpTarget, 0.1f * speed);
                // Check if animation is over
                if (wallIntro.First().transform.position == tmpTarget)
                {
                    // disable the wall
                    GameObjectManager.setGameObjectState(wallIntro.First(), false);
                    gameAudioSource.loop = false; // stop sound
                    // update IAR
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(0).gameObject, false); // first child is locked tab
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(1).gameObject, true); // second child is unlocked tab
                    // update flags
                    playerLookingAtDoor = false;
                    IARScreenRoom1Unlocked = true;
                    // Exit the locker
                    ExitLocker();
                }
            }
        }

        if (room3Unlocked && !IARScreenRoom3Unlocked)
        {
            if (!playerLookingAtDoor)
            {
                tmpTarget = (fences.getAt(0).transform.position + fences.getAt(1).transform.position) / 2 - Camera.main.transform.position;
                Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180 * speed, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                {
                    Camera.main.transform.forward = tmpTarget;
                    gameAudioSource.clip = selectedLocker.GetComponent<Selectable>().right;
                    gameAudioSource.PlayDelayed(0);
                    gameAudioSource.loop = true;
                    playerLookingAtDoor = true;
                    playerLookingAtDoor = true;
                }
            }
            else
            {
                fences.getAt(0).transform.Rotate(0, 0, -Time.deltaTime * 100);
                fences.getAt(1).transform.Rotate(0, 0, Time.deltaTime * 100);
                angleCount += Time.deltaTime * 100;
                if (angleCount > 103)
                {
                    fences.getAt(0).transform.Rotate(0, 0, -(103 - angleCount));
                    fences.getAt(1).transform.Rotate(0, 0, 103 - angleCount);
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
        CollectableGO.askCloseInventory = true;
        if (onLock)
        {
            GameObjectManager.setGameObjectState(selectedLocker.LeftRightControl, false);
            GameObjectManager.setGameObjectState(selectedLocker.UpDownControl, false);
            selectedLocker.Wheel1.GetComponent<Renderer>().material.color = lockWheelColor;
            selectedLocker.Wheel2.GetComponent<Renderer>().material.color = lockWheelColor;
            selectedLocker.Wheel3.GetComponent<Renderer>().material.color = lockWheelColor;
            selectedLocker.GetComponent<Selectable>().solved = false;
            if (HelpSystem.monitoring)
            {
                MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(3), "turnOff");
                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                HelpSystem.traces.Enqueue(trace);
            }
        }
        onLock = false;
        // reset intial player scale
        player.First().transform.localScale = playerLocalScale;
        // enable systems
        MovingSystem.instance.Pause = false;
        Highlighter.instance.Pause = false;
        DreamFragmentCollecting.instance.Pause = false;
        IARTabNavigation.instance.Pause = false;
        IARNewItemAvailable.instance.Pause = false;
        ToggleObject.instance.Pause = false;
        CollectObject.instance.Pause = false;
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