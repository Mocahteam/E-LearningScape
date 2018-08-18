using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;

public class MoveInFrontOf : FSystem {

    // all focused GO (only once)
    private Family f_focused = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Highlighted)));
    // ReadyToWork component is added dynamically by this system and removed by systems interested by it (LockResolver, PlankManager...)
    private Family f_readyToWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    //information for animations
    private float speed;
    private float speedRotation;
    private float angleRotation;
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
    private bool moveInFrontOf = false;
    private Vector3 targetPos;
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
    private GameObject focusedGO;

    public static MoveInFrontOf instance;

    public MoveInFrontOf()
    {
        if (Application.isPlaying)
        {
            f_readyToWork.addExitCallback(onWorkFinished);
        }
        instance = this;
    }

    private void onWorkFinished(int instanceId)
    {
        // if family is empty we can resume stopped systems
        if (f_readyToWork.Count == 0)
        {
            // reset intial player scale
            player.First().transform.localScale = playerLocalScale;
            // enable systems
            MovingSystem.instance.Pause = false;
            Highlighter.instance.Pause = false;
            DreamFragmentCollecting.instance.Pause = false;
            ToggleObject.instance.Pause = false;
            CollectObject.instance.Pause = false;
            // pause unused system
            LockResolver.instance.Pause = true;
            PlankManager.instance.Pause = true;
            BallBoxManager.instance.Pause = true;
        }
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

        if (Input.GetMouseButtonDown(0))
        {
            focusedGO = f_focused.First();
            if (focusedGO)
            {
                // pause unuse systems
                MovingSystem.instance.Pause = true;
                Highlighter.instance.Pause = true;
                DreamFragmentCollecting.instance.Pause = true;
                ToggleObject.instance.Pause = true;
                CollectObject.instance.Pause = true;
                // launch required systems
                LockResolver.instance.Pause = false;
                PlankManager.instance.Pause = false;
                BallBoxManager.instance.Pause = false;

                // save player scale (crouch or not) in order to reset it when player exit the focused GameObject
                playerLocalScale = player.First().transform.localScale;
                // be sure the player standing in front of the locker
                player.First().transform.localScale = Vector3.one;
                // compute target position and orientation in front of the focused GameObject
                Selectable selectable = focusedGO.GetComponent<Selectable>();
                targetPos = new Vector3(focusedGO.transform.position.x + selectable.standingPosDelta.x, focusedGO.transform.position.y + selectable.standingPosDelta.y, focusedGO.transform.position.z + selectable.standingPosDelta.z);
                // compute distance between the player and the focused GO and compute speed rotation
                dist = (targetPos - player.First().transform.position).magnitude;
                camNewDir = selectable.standingOrientation;
                angleRotation = Vector3.Angle(Camera.main.transform.forward, camNewDir);
                moveInFrontOf = true; // start animation to move the player in front of the lock
                if (HelpSystem.monitoring)
                {
                    MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(3), "turnOn");
                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                    HelpSystem.traces.Enqueue(trace);
                }
            }
        }

        // Do we have to process animation to move the player in front of the selected GO ?
        if (moveInFrontOf)
        {
            // move the player in front of the selected GO
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, targetPos, speed);
            speedRotation = angleRotation * speed / dist;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            // Check if the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, newDir) < 0.5f && player.First().transform.position == targetPos)
            {
                // Correct position
                player.First().transform.position = targetPos;
                // Correct the rotation
                newDir = Vector3.RotateTowards(player.First().transform.forward, camNewDir, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                // animation is over
                moveInFrontOf = false;
                // Add ReadyToWork component
                GameObjectManager.addComponent<ReadyToWork>(focusedGO);
            }
        }
    }
}