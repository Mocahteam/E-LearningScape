using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
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
    private bool ballFocused = false;       //true when a ball is focused
    private bool moveBall = false;          //true during the animation of selection of a ball
    private GameObject focusedBall = null;  //store the focused ball
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
            focusedBall.GetComponent<Renderer>().material.color = focusedBall.GetComponent<Ball>().color;
    }

    // return true if key is selected into inventory
    private GameObject keySelected()
    {
        foreach (GameObject go in itemSelected)
            if (go.name == "KeyE03")
                return go;
        return null;
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
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closeBox.Count == 0 && Input.GetMouseButtonDown(0) && ballsout) || (Input.GetKeyDown(KeyCode.Escape) && !ballFocused && f_iarBackground.Count == 0 && ballsout)))
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

                        ballToCamera = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0.5f));  //set position of balls when in front of the screen
                    }
                }
            }

            if (!boxPadlock.activeSelf && !boxOpenned)
            {
                // open the cover of the box
                float step = speedRotation * Time.deltaTime;
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
                    ballPos = Vector3.up * ((float)f_balls.Count / 16 - (float)(b.id / 5) / 3) + Vector3.right * ((float)(b.id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.6f;
                    ballGo.transform.localPosition = Vector3.MoveTowards(ballGo.transform.localPosition, ballPos, speed / 2);
                    //when the last ball arrives to its position
                    if (ballGo.transform.localPosition == ballPos)
                    {
                        //stop animations
                        b.outOfBox = true;
                        ballCounter++; //with this the next ball will start moving
                        if (ballCounter == f_balls.Count)
                            ballsout = true;
                    }
                    else if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
                    {
                        //stop animations
                        for (int i = 0; i < f_balls.Count; i++)
                        {
                            ballGo = f_balls.getAt(i);
                            b = ballGo.GetComponent<Ball>();
                            b.outOfBox = true;
                            ballPos = Vector3.up * ((float)f_balls.Count / 16 - (float)(b.id / 5) / 3) + Vector3.right * ((float)(b.id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.6f;
                            ballGo.transform.localPosition = ballPos;
                            ballGo.transform.localRotation = Quaternion.Euler(Vector3.up * -90 + Vector3.right * 90);
                        }
                        ballCounter = f_balls.Count;
                        ballsout = true;
                    }
                }
            }

            if (ballsout)
            {
                // balls interaction to see background digit

            }

            if (closeBox)
            {
                // close the cover of the box
                float step = speedRotation * Time.deltaTime;
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

            /*            else
                        {
                            else if (moveBall)
                            {
                                //animation to move the selected ball
                                if (ballFocused)
                                {
                                    //if the ball is selected, move it in front of the camera and rotate it to make its back visible
                                    focusedBall.transform.position = Vector3.MoveTowards(focusedBall.transform.position, ballToCamera, speed);
                                    focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(90, 90, 0), speedRotation);
                                    //when the ball arrives
                                    if (focusedBall.transform.position == ballToCamera)
                                    {
                                        if (focusedBall.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                                        {
                                            MonitoringTrace trace = new MonitoringTrace(focusedBall.GetComponent<ComponentMonitoring>(), "activate");
                                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                            HelpSystem.traces.Enqueue(trace);
                                        }
                                        int id = focusedBall.GetComponent<Ball>().id;
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
                                        if (!box.First().GetComponent<Selectable>().solved)
                                        {
                                            if (ball1Seen && ball2Seen && ball8Seen)
                                            {
                                                box.First().GetComponent<Selectable>().solved = true;
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
                                        moveBall = false;
                                    }
                                }
                                else
                                {
                                    //if the ball is not selected, move it back with the other balls and rotate it back
                                    ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
                                    focusedBall.transform.localPosition = Vector3.MoveTowards(focusedBall.transform.localPosition, ballPos, speed);
                                    focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(90, -90, 0), speedRotation * 2);
                                    //when the ball arrives
                                    if (focusedBall.transform.localPosition == ballPos)
                                    {
                                        moveBall = false;
                                        Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = true;
                                        //hide the number behind
                                        foreach (Transform child in focusedBall.transform)
                                        {
                                            if (child.gameObject.name == "Number")
                                            {
                                                GameObjectManager.setGameObjectState(child.gameObject, false);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (onBox && (!moveBox || boxPadlock.activeSelf))
                            {
                                if (((f_closeBox.Count == 0 && Input.GetMouseButtonDown(0) && !wasTakingballs) || Input.GetKeyDown(KeyCode.Escape)) && !ballFocused)
                                {
                                    CloseWindow();
                                }
                                else if (ballsout)
                                {   //if all balls are out of the box
                                    Ball b = null;
                                    if (ballFocused)
                                    {    //if there is a selected ball
                                        if (Input.GetMouseButtonDown(0) && !moveBall)
                                        {
                                            //onclick if the ball isn't moving, move it back to its position with other balls
                                            ballFocused = false;
                                            moveBall = true;
                                            //calculate position and speeds for the animation
                                            ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
                                            dist = (box.First().transform.TransformPoint(ballPos) - ballToCamera).magnitude;
                                            speedRotation = 180 * speed / dist / 2;
                                        }
                                    }
                                    else if (!moveBall)
                                    {  //if there isn't animations and selected ball
                                        int nbBalls = balls.Count;
                                        bool overNothing = true;
                                        for (int i = 0; i < nbBalls; i++)
                                        {
                                            forGO = balls.getAt(i);
                                            b = forGO.GetComponent<Ball>();
                                            if (forGO.GetComponent<PointerOver>())
                                            {
                                                overNothing = false;
                                                //if pointer over a ball change its color to yellow
                                                forGO.GetComponent<Renderer>().material.color = Color.yellow + Color.white / 4;
                                                ballSubTitles.text = b.text;
                                                if (Input.GetMouseButtonDown(0))
                                                {    //if a ball is clicked
                                                     //move it in front of the camera
                                                    ballFocused = true;
                                                    moveBall = true;
                                                    focusedBall = forGO;
                                                    foreach (Transform child in focusedBall.transform)
                                                    {
                                                        if (child.gameObject.name == "Number")
                                                        {
                                                            GameObjectManager.setGameObjectState(child.gameObject, true);
                                                        }
                                                    }
                                                    forGO.GetComponent<Renderer>().material.color = b.color; //initial color
                                                    ballSubTitles.text = "";
                                                    //calculate position and speeds for the animation
                                                    ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
                                                    dist = (box.First().transform.TransformPoint(ballPos) - ballToCamera).magnitude;
                                                    speedRotation = 180 * speed / dist;
                                                    Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;
                                                }
                                            }
                                            else
                                            {
                                                //if there isn't animations, mouse over or click, set to initial color
                                                forGO.GetComponent<Renderer>().material.color = b.color;
                                            }
                                        }
                                        if (overNothing)
                                        {
                                            ballSubTitles.text = "";
                                        }
                                    }
                                }
                            }
                        }*/
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