using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections.Generic;
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
    private Family f_closeBox = FamilyManager.getFamily(new AnyOfTags("Box", "Ball", "InventoryElements", "HUD_TransparentOnMove"), new AllOfComponents(typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));

    //information for animations
    private float speed;
    private float speedRotation = 200f;
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
    private GameObject ballSubTitles;
    private TextMeshProUGUI ballSubTitlesContent;
    private GameObject box;
    private GameObject boxTop;

    private bool unlocked = false;
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
            boxTop = box.transform.GetChild(2).gameObject;
            ballSubTitles = box.transform.GetChild(4).gameObject;
            ballSubTitlesContent = ballSubTitles.GetComponentInChildren<TextMeshProUGUI>();

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
            GameObjectManager.setGameObjectState(ballSubTitles, true);
            ballSubTitlesContent.text = b.text;

            focusedBall = go;
            GameObjectManager.addComponent<ActionPerformedForLRS>(focusedBall, new
            {
                verb = "read",
                objectType = "interactable",
                objectName = focusedBall.name,
                activityExtensions = new Dictionary<string, string>() { { "content", b.text } }
            });
        }
    }

    private void onExitBall(int instanceId)
    {
        GameObjectManager.setGameObjectState(ballSubTitles, false);
        ballSubTitlesContent.text = "";
        if (focusedBall)
        {
            GameObjectManager.addComponent<ActionPerformedForLRS>(focusedBall, new
            {
                verb = "exitedView",
                objectType = "interactable",
                objectName = focusedBall.name
            });
            focusedBall.GetComponent<Renderer>().material.color = focusedBall.GetComponent<Ball>().color;
            focusedBall = null;
        }
    }

    // return true if key is selected into inventory
    private GameObject keySelected()
    {
        foreach (GameObject go in f_itemSelected)
            if (go.name == "KeyBallBox")
                return go;
        return null;
    }

    private Vector3 ballPosOnGrid (Ball ball)
    {
        return Vector3.up * ((float)f_balls.Count / 11 - (float)(ball.id / 5) / 2) + Vector3.right * ((float)(ball.id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.6f;
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
            if (((f_closeBox.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && (!unlocked || (ballsout && !inFrontOfCamera && !selectedBall)))
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

            if (!unlocked)
            {
                if (keySelected())
                {
                    UnlockBallBox();
                    //remove key from inventory
                    GameObjectManager.setGameObjectState(keySelected(), false);
                    GameObjectManager.setGameObjectState(keySelected().GetComponent<HUDItemSelected>().hudGO, false);
                    // trace
                    GameObjectManager.addComponent<ActionPerformed>(boxPadlock.GetComponentInChildren<ComponentMonitoring>().gameObject, new { name = "perform", performedBy = "system" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBox, new { verb = "unlocked", objectType = "interactable", objectName = selectedBox.name });
                }
            }

            if (unlocked && !boxOpenned && boxPadlock.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8)
            {
                // open the cover of the box
                boxTop.GetComponent<Animator>().SetTrigger("turnOn");
                boxOpenned = true;
                ballCounter = 0;
            }

            if (boxOpenned && ballCounter < f_balls.Count && boxTop.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8)
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
                    if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel"))
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
                        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBox, new { verb = "skipped", objectType = "animation", objectName = string.Concat(selectedBox.name, "_opening") });
                    }
                    //when the ball reaches to its position
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
                if (focusedBall && !selectedBall && Input.GetButtonDown("Fire1") && !moveBall && !inFrontOfCamera)
                {
                    //calculate position and speeds for the animation
                    targetPos = new Vector3 (0f, 0.5f, 1.37f);
                    targetRotation = Quaternion.Euler(90, 0, 270);
                    moveBall = true;
                    selectedBall = focusedBall;
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBall, new
                    {
                        verb = "interacted",
                        objectType = "interactable",
                        objectName = selectedBall.name,
                        activityExtensions = new Dictionary<string, string>() {
                            { "content", focusedBall.GetComponent<Ball>().text },
                            { "value", focusedBall.GetComponent<Ball>().number.ToString() }
                        }
                    });
                    Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;

                    GameObjectManager.addComponent<ActionPerformed>(selectedBall, new { name = "activate", performedBy = "player" });
                }

                // Ask ball to move back on grid with other balls
                if (selectedBall && (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel")) && inFrontOfCamera)
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
                if (unlocked)
                    // close the cover of the box
                    boxTop.GetComponent<Animator>().SetTrigger("turnOff");
                ExitBox();
            }
        }
	}

    private void ExitBox()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedBox);

        ballCounter = 0;
        ballSubTitlesContent.text = "";
        GameObjectManager.setGameObjectState(ballSubTitles, false);
        boxOpenned = false;

        GameObjectManager.addComponent<ActionPerformed>(selectedBox, new { name = "turnOff", performedBy = "player" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBox, new { verb = "exited", objectType = "interactable", objectName = selectedBox.name });

        selectedBox = null;

        // Pause this system
        instance.Pause = true;
    }

    public void UnlockBallBox()
    {
        unlocked = true;
        // start animation
        boxPadlock.GetComponent<Animator>().enabled = true;
    }

    public bool IsLocked()
    {
        return !unlocked;
    }
}