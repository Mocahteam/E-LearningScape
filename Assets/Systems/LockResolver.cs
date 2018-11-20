using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

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

    private Family f_mainloop = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));

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
    private float wheelRotationCount = 0;

    private bool room1Unlocked = false;
    private bool room3Unlocked = false;
    private bool IARScreenRoom1Unlocked = false;
    private bool IARScreenRoom3Unlocked = false;
    private bool playerLookingAtDoor = false;
    private AudioSource gameAudioSource;

    private string closedBy = "";

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
                                    SelectLeftWheel(ref selectedWheel, locker.Wheel1, locker.Wheel2, locker.Wheel3, locker.UpDownControl);
                                });
                            }
                            else if (c.gameObject.name == "Right")
                            {
                                c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                    SelectRightWheel(ref selectedWheel, locker.Wheel1, locker.Wheel2, locker.Wheel3, locker.UpDownControl);
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
                                    moveWheelUp(selectedWheel);
                                });
                            }
                            else if (c.gameObject.name == "Down")
                            {
                                c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                    moveWheelDown(selectedWheel);
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

        GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
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

        // Are we in front of the locker
        if (selectedLocker)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed
            if (((f_closeLock.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)) && (!room1Unlocked || IARScreenRoom1Unlocked) && (!room3Unlocked || IARScreenRoom3Unlocked))
            {
                closedBy = "player";
                ExitLocker();
            }
            else
            {
                // avoid to rotate wheel during unlock animation
                if ((!room1Unlocked || IARScreenRoom1Unlocked) && (!room3Unlocked || IARScreenRoom3Unlocked))
                {
                    if (Input.GetMouseButtonDown(0))
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
                        moveWheelUp(selectedWheel);
                    else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                        moveWheelDown(selectedWheel);
                    else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
                        SelectLeftWheel(ref selectedWheel, selectedLocker.Wheel1, selectedLocker.Wheel2, selectedLocker.Wheel3, selectedLocker.UpDownControl);
                    else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                        SelectRightWheel(ref selectedWheel, selectedLocker.Wheel1, selectedLocker.Wheel2, selectedLocker.Wheel3, selectedLocker.UpDownControl);
                }
            }
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
                if (selectedLocker.Wheel1.GetComponent<WheelFrontFace>().faceNumber == selectedLocker.wheel1Solution && selectedLocker.Wheel2.GetComponent<WheelFrontFace>().faceNumber == selectedLocker.wheel2Solution && selectedLocker.Wheel3.GetComponent<WheelFrontFace>().faceNumber == selectedLocker.wheel3Solution)
                {
                    tmpTargetPosition = f_player.First().transform.position + Vector3.back * 3;
                    // depending of locker => unlock the right room
                    if (selectedLocker.tag == "LockIntro")
                    {
                        room1Unlocked = true;
                    }
                    else
                        room3Unlocked = true;
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
                    GameObjectManager.addComponent<ActionPerformed>(f_wallIntro.First(), new { name = "perform", performedBy = "player" });

                    GameObjectManager.addComponent<ActionPerformed>(f_wallIntro.First(), new { name = "perform2", performedBy = "system" });

                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLocker.gameObject, new { verb = "completed", objectType = "interactable", objectName = selectedLocker.gameObject.name });

                    // disable the wall
                    GameObjectManager.setGameObjectState(f_wallIntro.First(), false);
                    gameAudioSource.loop = false; // stop sound
                    // update IAR
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(0).gameObject, false); // first child is locked tab
                    GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock.transform.GetChild(1).gameObject, true); // second child is unlocked tab
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLocker.IARScreenUnlock, new { verb = "unlocked", objectType = "menu", objectName = selectedLocker.IARScreenUnlock.name });
                    // update flags
                    playerLookingAtDoor = false;
                    IARScreenRoom1Unlocked = true;
                    // disable UI items usable for this enigm
                    if (selectedLocker.GetComponent<LinkedWith>())
                        GameObjectManager.setGameObjectState(selectedLocker.GetComponent<LinkedWith>().link, false);
                    // Exit the locker
                    closedBy = "system";
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
                    GameObjectManager.addComponent<ActionPerformed>(selectedLocker.gameObject, new { name = "perform", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLocker.gameObject, new { verb = "completed",
                        objectType = "interactable", objectName = selectedLocker.gameObject.name });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLocker.IARScreenUnlock, new { verb = "unlocked", objectType = "menu",
                        objectName = selectedLocker.IARScreenUnlock.name });
                    closedBy = "system";
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

        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLocker.gameObject);

        GameObjectManager.addComponent<ActionPerformed>(selectedLocker.gameObject, new { name = "turnOff", performedBy = closedBy });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLocker.gameObject, new { verb = "exited", objectType = "interactable", objectName = selectedLocker.gameObject.name });

        selectedLocker = null;

        // disable this system
        instance.Pause = true;
    }

    private void moveWheelUp(GameObject wheel)
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            lockRotationUp = true;
            WheelFrontFace wff = wheel.GetComponent<WheelFrontFace>();
            if(wff.faceNumber == 9)
                wff.faceNumber = 0;
            else
                wff.faceNumber++;
        }
    }

    private void moveWheelDown(GameObject wheel)
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            lockRotationDown = true;
            WheelFrontFace wff = wheel.GetComponent<WheelFrontFace>();
            if (wff.faceNumber == 0)
                wff.faceNumber = 9;
            else
                wff.faceNumber--;
        }
    }

    private void SelectLeftWheel(ref GameObject wheel, GameObject wheel1, GameObject wheel2, GameObject wheel3, GameObject lockUDUI)
    {
        if (!lockRotationUp && !lockRotationDown)
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

    private void SelectRightWheel(ref GameObject wheel, GameObject wheel1, GameObject wheel2, GameObject wheel3, GameObject lockUDUI)
    {
        if (!lockRotationUp && !lockRotationDown)
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