using UnityEngine;
using UnityEngine.PostProcessing;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using FYFY_plugins.Monitoring;

public class BallBoxManager : FSystem {

    // this system manage the plank and the wire

    //all selectable objects
    private Family f_box = FamilyManager.getFamily(new AnyOfTags("Box"));
    private Family f_selectedBox = FamilyManager.getFamily(new AnyOfTags("Box"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family f_focusedBalls = FamilyManager.getFamily(new AnyOfTags("Ball"), new AllOfComponents(typeof(PointerOver), typeof(MeshRenderer)));
    private Family f_closeBox = FamilyManager.getFamily(new AnyOfTags("Box", "Ball", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));

    //information for animations
    private float speed;
    private float speedRotation = 200f;
    private float coverSpeedRotation = 200f;
    private float oldDT;
    private float dist = -1;
    private Vector3 objectPos = Vector3.zero;
    private int ballCounter = -1;
    private Vector3 camNewDir;
    private Vector3 newDir;
    private Vector3 playerLocalScale;

    //box
    private bool onBox = false;                 //true when the player is using the box
    private bool moveBox = false;               //true during the animation to move the box in front of the player
    private bool openBox = false;               //true during the animation to open the box
    private bool takingBalls = false;           //true during the animation to take out balls from the box
    private bool wasTakingballs = false;
    private Vector3 boxTopPos = Vector3.zero;   //position of the box lid when opened
    private Vector3 boxTopIniPos;               //position of the box lid when closed
    private bool ballsout = false;              //true when all balss are out
    private GameObject boxPadlock;
    private bool unlockBox = false;
    //used during the selection of a ball
    private Vector3 ballPos = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    private Quaternion targetRotation;
    private bool inFrontOfCamera = false;       //true when a ball is in front of camera
    private bool moveBall = false;          //true during the animation of selection of a ball
    private GameObject focusedBall = null;  //store the focused ball
    private GameObject selectedBall = null;  //store the selected ball
    private Vector3 ballToCamera;           //position of the ball when selected
    private TextMeshProUGUI ballSubTitles;
    private bool ball1Seen = false;
    private bool ball2Seen = false;
    private bool ball8Seen = false;
    private GameObject box;
    private GameObject boxTop;

    private float tmpRotationCount = 0;
    private bool boxOpenned = false;
    private bool closeBox = false;

    private GameObject selectedBox;

    public static BallBoxManager instance;

    public BallBoxManager()
    {
        if (Application.isPlaying)
        {
            ballToCamera = Camera.main.transform.position + Camera.main.transform.forward;
            box = f_box.First();
            boxPadlock = box.transform.GetChild(0).gameObject;
            boxTop = box.transform.GetChild(3).gameObject;
            boxTopIniPos = boxTop.transform.localPosition;
            boxTopPos = boxTop.transform.localPosition + boxTop.transform.right - boxTop.transform.up / 2; //set lid position
            ballSubTitles = box.transform.GetChild(4).gameObject.GetComponentInChildren<TextMeshProUGUI>();

            f_selectedBox.addEntryCallback(onReadyToWorkOnPlank);
            f_focusedBalls.addEntryCallback(onEnterBall);
            f_focusedBalls.addExitCallback(onExitBall);
        }
        instance = this;
    }

    private void onReadyToWorkOnPlank(GameObject go)
    {
        selectedBox = go;
        boxOpenned = false;
    }

    private void onEnterBall(GameObject go)
    {
        Ball b = go.GetComponent<Ball>();
        go.GetComponent<Renderer>().material.color = Color.yellow + Color.white / 4;
        ballSubTitles.text = b.text;

        focusedBall = go;
    }

    private void onExitBall(int instanceId)
    {
        ballSubTitles.text = "";
        if (focusedBall)
        {
            focusedBall.GetComponent<Renderer>().material.color = focusedBall.GetComponent<Ball>().color;
            focusedBall = null;
        }
    }

    // return true if key is selected into inventory
    private GameObject keySelected()
    {
        foreach (GameObject go in itemSelected)
            if (go.name == "KeyE03")
                return go;
        return null;
    }

    private Vector3 ballPosOnGrid (Ball ball)
    {
        return Vector3.up * ((float)f_balls.Count / 16 - (float)(ball.id / 5) / 3) + Vector3.right * ((float)(ball.id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.6f;
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

        if (selectedBox)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and balls are out of the box but none are in front of camera and IAR is closed (because Escape close IAR)
            if (((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape)) && ballsout && !inFrontOfCamera && !selectedBall && !focusedBall && (f_closeBox.Count == 0 || f_iarBackground.Count == 0)))
            {
                // set balls to initial position
                foreach (GameObject ball in f_balls)
                {
                    ball.transform.localPosition = ball.GetComponent<Ball>().initialPosition;
                    ball.GetComponent<Ball>().outOfBox = false;
                }
                // ask to close box
                closeBox = true;
            }

            if (boxPadlock.activeSelf)
            {
                if (keySelected())
                {
                    //move up and rotate the padlock
                    dist = 1.5f - boxPadlock.transform.localPosition.y;
                    boxPadlock.transform.localPosition = Vector3.MoveTowards(boxPadlock.transform.localPosition, boxPadlock.transform.localPosition + Vector3.up * dist, (dist + 1) / 2 * Time.deltaTime);
                    boxPadlock.transform.localRotation = Quaternion.Euler(boxPadlock.transform.localRotation.eulerAngles + Vector3.up * (boxPadlock.transform.localPosition.y - 0.2f) * 350 * 5 * Time.deltaTime);
                    if (boxPadlock.transform.localPosition.y > 1.4f)
                    {
                        //stop animation when the padlock reaches a certain height
                        GameObjectManager.setGameObjectState(boxPadlock, false);
                        if (boxPadlock.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                        {
                            MonitoringTrace trace = new MonitoringTrace(boxPadlock.GetComponent<ComponentMonitoring>(), "perform");
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                            HelpSystem.traces.Enqueue(trace);
                        }
                        //remove key from inventory
                        GameObjectManager.setGameObjectState(keySelected(), false);
                    }
                }
            }

            if (!boxPadlock.activeSelf && !boxOpenned)
            {
                // open the cover of the box
                float step = coverSpeedRotation * Time.deltaTime;
                tmpRotationCount += step;
                boxTop.transform.Rotate(-step, 0, 0);
                if (tmpRotationCount > 120)
                {
                    // correct rotation
                    boxTop.transform.Rotate(tmpRotationCount - 120, 0, 0);
                    tmpRotationCount = 0;
                    boxOpenned = true;
                    ballCounter = 0;
                }
            }

            if (boxOpenned && ballCounter < f_balls.Count)
            {
                //animation to take balls out of the box
                GameObject ballGo = f_balls.getAt(ballCounter);
                Ball b = ballGo.GetComponent<Ball>();
                if (!b.outOfBox) //if the ball is still in the box
                {
                    //move the ball to the position corresponding to its id
                    ballPos = ballPosOnGrid(b);
                    ballGo.transform.localPosition = Vector3.MoveTowards(ballGo.transform.localPosition, ballPos, speed / 2);
                    
                    // Check if we have to abort animation
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
                    {
                        // abort animation
                        for (int i = 0; i < f_balls.Count; i++)
                        {
                            ballGo = f_balls.getAt(i);
                            b = ballGo.GetComponent<Ball>();
                            b.outOfBox = true;
                            ballPos = ballPosOnGrid(b);
                            ballGo.transform.localPosition = ballPos;
                        }
                        ballCounter = f_balls.Count;
                    }
                    //when the last ball arrives to its position
                    if (ballGo.transform.localPosition == ballPos)
                    {
                        //stop animations
                        b.outOfBox = true;
                        if (ballCounter < f_balls.Count - 1)
                            ballCounter++; //with this the next ball will start moving
                        else
                        {
                            ballsout = true;
                            inFrontOfCamera = false;
                        }
                    }
                }
            }

            if (ballsout)
            {
                // balls interaction to see background digit

                // Ask a ball to move in front of the camera
                if (focusedBall && !selectedBall && Input.GetMouseButtonDown(0) && !moveBall && !inFrontOfCamera)
                {
                    //calculate position and speeds for the animation
                    targetPos = new Vector3 (0f, 0.5f, 1.37f);
                    targetRotation = Quaternion.Euler(90, 0, 270);
                    moveBall = true;
                    selectedBall = focusedBall;
                }

                // Ask ball to move back on grid with other balls
                if (selectedBall && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape)) && inFrontOfCamera)
                {
                    //calculate position and speeds for the animation
                    targetPos = ballPosOnGrid(selectedBall.GetComponent<Ball>());
                    targetRotation = Quaternion.Euler(90, 0, 90);
                    moveBall = true;
                }

                // Move the ball
                if (moveBall)
                {
                    dist = (selectedBall.transform.localPosition - targetPos).magnitude;
                    speedRotation = 180 * speed / dist;
                    selectedBall.transform.localPosition = Vector3.MoveTowards(selectedBall.transform.localPosition, targetPos, speed);
                    selectedBall.transform.localRotation = Quaternion.RotateTowards(selectedBall.transform.localRotation, targetRotation, speedRotation);
                    //when the ball arrives
                    if (selectedBall.transform.localPosition == targetPos)
                    {
                        moveBall = false;
                        inFrontOfCamera = targetPos != ballPosOnGrid(selectedBall.GetComponent<Ball>());
                        if (!inFrontOfCamera)
                            selectedBall = null;
                        /*else
                        {
                            if (selectedBall.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                            {
                                MonitoringTrace trace = new MonitoringTrace(selectedBall.GetComponent<ComponentMonitoring>(), "activate");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }
                            int id = selectedBall.GetComponent<Ball>().id;
                            switch (id)
                            {
                                case 0:
                                    ball1Seen = true;
                                    break;

                                case 1:
                                    ball2Seen = true;
                                    break;

                                case 7:
                                    ball8Seen = true;
                                    break;

                                default:
                                    break;
                            }
                            if (!box.GetComponent<Selectable>().solved)
                            {
                                if (ball1Seen && ball2Seen && ball8Seen)
                                {
                                    box.GetComponent<Selectable>().solved = true;
                                    if (HelpSystem.monitoring)
                                    {
                                        MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(22), "perform");
                                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                        HelpSystem.traces.Enqueue(trace);
                                        trace = new MonitoringTrace(MonitoringManager.getMonitorById(32), "perform");
                                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                        HelpSystem.traces.Enqueue(trace);
                                    }
                                }
                            }
                        }*/
                    }
                }
            }

            if (closeBox)
            {
                // close the cover of the box
                float step = coverSpeedRotation * Time.deltaTime;
                boxTop.transform.Rotate(step, 0, 0);
                tmpRotationCount += step;
                if (tmpRotationCount > 120)
                {
                    // correct rotation
                    boxTop.transform.Rotate(120 - tmpRotationCount, 0, 0);
                    tmpRotationCount = 0;
                    closeBox = false;
                    ExitBox();
                }
            }
        }
	}

    private void ExitBox()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedBox);

        ballCounter = 0;
        ballSubTitles.text = "";
        boxOpenned = false;
        if (HelpSystem.monitoring)
        {
            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(28), "turnOff");
            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
            HelpSystem.traces.Enqueue(trace);
        }

        selectedBox = null;

        if (HelpSystem.monitoring)
        {
            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(28), "turnOff");
            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
            HelpSystem.traces.Enqueue(trace);
        }
    }
}