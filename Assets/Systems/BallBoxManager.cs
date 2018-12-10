using UnityEngine;
using UnityEngine.PostProcessing;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;
using TMPro;

public class BallBoxManager : FSystem {

    // this system manage the box and balls

    //all selectable objects
    private Family f_box = FamilyManager.getFamily(new AnyOfTags("Box"));
    private Family f_selectedBox = FamilyManager.getFamily(new AnyOfTags("Box"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family f_focusedBalls = FamilyManager.getFamily(new AnyOfTags("Ball"), new AllOfComponents(typeof(PointerOver), typeof(MeshRenderer)));
    private Family f_closeBox = FamilyManager.getFamily(new AnyOfTags("Box", "Ball", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));

    //information for animations
    private float speed;
    private float speedRotation = 200f;
    private float coverSpeedRotation = 200f;
    private float dist = -1;
    private int ballCounter = -1;

    //box
    private bool ballsout = false;              //true when all balss are out
    private GameObject boxPadlock;
    //used during the selection of a ball
    private Vector3 ballPos = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    private Quaternion targetRotation;
    private bool inFrontOfCamera = false;       //true when a ball is in front of camera
    private bool moveBall = false;          //true during the animation of selection of a ball
    private GameObject focusedBall = null;  //store the focused ball
    private GameObject selectedBall = null;  //store the selected ball
    private TextMeshProUGUI ballSubTitles;
    private GameObject box;
    private GameObject boxTop;

    private float tmpRotationCount = 0;
    private bool boxOpenned = false;
    private bool closeBox = false;
    private bool depthOfFieldsEnabled = false;

    private GameObject selectedBox;

    public static BallBoxManager instance;

    public BallBoxManager()
    {
        if (Application.isPlaying)
        {
            box = f_box.First();
            boxPadlock = box.transform.GetChild(0).gameObject;
            boxTop = box.transform.GetChild(3).gameObject;
            ballSubTitles = box.transform.GetChild(4).gameObject.GetComponentInChildren<TextMeshProUGUI>();

            foreach (GameObject ball in f_balls)
                ball.GetComponent<Ball>().initialPosition = ball.transform.localPosition;

            f_selectedBox.addEntryCallback(onReadyToWorkOnBallBox);
            f_focusedBalls.addEntryCallback(onEnterBall);
            f_focusedBalls.addExitCallback(onExitBall);
        }
        instance = this;
    }

    private void onReadyToWorkOnBallBox(GameObject go)
    {
        selectedBox = go;
        // init flag for animations
        boxOpenned = false;
        closeBox = false;
        ballsout = false;
        moveBall = false;
        tmpRotationCount = 0;
        ballCounter = 0;

        // Launch this system
        instance.Pause = false;

        depthOfFieldsEnabled = Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled;

        GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
    }

    private void onEnterBall(GameObject go)
    {
        if (f_iarBackground.Count == 0)
        {
            Ball b = go.GetComponent<Ball>();
            go.GetComponent<Renderer>().material.color = Color.yellow + Color.white / 4;
            ballSubTitles.text = b.text;

            focusedBall = go;
            GameObjectManager.addComponent<ActionPerformedForLRS>(focusedBall, new { verb = "highlighted", objectType = "interactable", objectName = focusedBall.name });
        }
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
        foreach (GameObject go in f_itemSelected)
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
            if (((f_closeBox.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)) && (boxPadlock.activeSelf || (ballsout && !inFrontOfCamera && !selectedBall)))
            {
                // set balls to initial position
                foreach (GameObject ball in f_balls)
                {
                    ball.transform.localPosition = ball.GetComponent<Ball>().initialPosition;
                    ball.GetComponent<Ball>().outOfBox = false;
                }
                ballsout = false;
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
                        //remove key from inventory
                        GameObjectManager.setGameObjectState(keySelected(), false);

                        GameObjectManager.addComponent<ActionPerformed>(boxPadlock, new { name = "perform", performedBy = "system" });
                        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBox, new { verb = "unlocked", objectType = "interactable", objectName = selectedBox.name });
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
                        ballCounter++; //with this the next ball will start moving
                        if (ballCounter >= f_balls.Count)
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
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBall, new { verb = "interacted", objectType = "interactable", objectName = selectedBall.name });
                    Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;

                    GameObjectManager.addComponent<ActionPerformed>(selectedBall, new { name = "activate", performedBy = "player" });
                }

                // Ask ball to move back on grid with other balls
                if (selectedBall && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape)) && inFrontOfCamera)
                {
                    //calculate position and speeds for the animation
                    targetPos = ballPosOnGrid(selectedBall.GetComponent<Ball>());
                    targetRotation = Quaternion.Euler(90, 0, 90);
                    moveBall = true;
                    Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = depthOfFieldsEnabled;
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
                    }
                }
            }

            if (closeBox)
            {
                // Check if padlock is enabled => means the box is already closed
                if (!boxPadlock.activeSelf)
                {
                    // close the cover of the box
                    float step = coverSpeedRotation * Time.deltaTime;
                    boxTop.transform.Rotate(step, 0, 0);
                    tmpRotationCount += step;
                    if (tmpRotationCount > 120)
                    {
                        // correct rotation
                        boxTop.transform.Rotate(120 - tmpRotationCount, 0, 0);
                        ExitBox();
                    }
                }
                else
                    ExitBox();
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

        GameObjectManager.addComponent<ActionPerformed>(selectedBox, new { name = "turnOff", performedBy = "player" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBox, new { verb = "exited", objectType = "interactable", objectName = selectedBox.name });

        selectedBox = null;

        // Pause this system
        instance.Pause = true;
    }
}