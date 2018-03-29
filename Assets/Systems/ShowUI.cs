using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using TMPro;

public class ShowUI : FSystem {
    
    //all selectable objects
    private Family objects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)));

    private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family plank = FamilyManager.getFamily(new AnyOfTags("Plank"));
    private Family plankWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerSensitive))); //clickable words on the plank
    private Family box = FamilyManager.getFamily(new AnyOfTags("Box"));
    private Family boxTop = FamilyManager.getFamily(new AnyOfTags("BoxTop"));   //box lid
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family inventory = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family cGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family bag = FamilyManager.getFamily(new AnyOfTags("Bag"));
    private Family lockR2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"));

    private bool noSelection = true;    //true if all objects are unselected
    private GameObject closeButton;
    private GameObject uiGO;
    private GameObject cursorUI;

    //information for animations
    private float speed = 0.1f;
    private float speedRotation = 0;
    private float speedRotation2 = 0;
    private float dist = -1;
    private Vector3 objectPos = Vector3.zero;
    private int tmpCount = -1;
    private Vector3 camNewDir;
    private Vector3 newDir;

    //plank
    private bool onPlank = false;           //true when the player is using the plank
    private bool moveToPlank = false;       //true during the animation to move the player in front of the plank
    private Vector3 plankPos;               //position of the player when using the plank
    private Vector3 plankTopRight;          //position of "close" button when using plank
    private bool pointerOverWord = false;   //true when the pointer is over a selectable word
    private LineRenderer lr;                //used to link words
    private List<Vector3> lrPositions;
    private Vector3 lrFirstPosition = Vector3.zero;

    //box
    private bool onBox = false;                 //true when the player is using the box
    private bool moveBox = false;               //true during the animation to move the box in front of the player
    private bool openBox = false;               //true during the animation to open the box
    private bool takingBalls = false;           //true during the animation to take out balls from the box
    private Vector3 boxTopPos = Vector3.zero;   //position of the box lid when opened
    private Vector3 boxTopIniPos;               //position of the box lid when closed
    private bool ballsout = false;              //true when all balss are out
    private Vector3 boxTopRight = Vector3.zero; //position of "close" button when using box
    private GameObject boxPadlock;
    private bool unlockBox = false;
    //used during the selection of a ball
    private Vector3 ballPos = Vector3.zero;
    private bool ballFocused = false;       //true when a ball is focused
    private bool moveBall = false;          //true during the animation of selection of a ball
    private GameObject focusedBall = null;  //store the focused ball
    private Vector3 ballToCamera;           //position of the ball when selected

    //tablet
    private GameObject selectedTablet;
    private GameObject tabletScreen;
    private bool onTablet = false;                  //true when the player is using the tablet
    private bool moveTablet = false;                //true during the animation to move the tablet in front of the player
    private Vector3 tabletTopRight = Vector3.zero;  //position of "close" button when using tablet
    //values used to set the position of the tablet according to screen size
    //(not necessary anymore since the ui is on the screen and not on the tablet now)
    private float aTabletDist = 1.42857143f;
    private float bTabletDist = -0.58571429f;

    //tables
    private bool onTable = false;                   //true when the player selected a table
    private bool moveToTable = false;               //true during the animation to move the player above the table
    private Vector3 posBeforeTable = Vector3.zero;  //store player's position before moving him above the table
    private Quaternion rotBeforeTable;              //store camera's rotation before moving the player above the table
    private Quaternion rot2BeforeTable;             //store player's rotation before moving him above the table
    private Vector3 onTablePoint = Vector3.zero;    //position above the table
    private GameObject focusedTable = null;         //store the focused table

    //sheets
    private bool onSheet = false;       //true when the player selected a sheet
    private GameObject focusedSheet;    //store the focused sheet
    private Quaternion rotBeforeSheet;  //rotation of the sheet before selection

    //bag
    private bool onBag = false;                   //true when the player selected a table
    private bool moveBag = false;               //true during the animation to move the player above the table
    private Vector3 bagTopRight = Vector3.zero;  //store player's position before moving him above the table
    private bool unlockBag = false;
    private GameObject bagPadlock;
    private bool showBagPaper = false;
    private GameObject bagAnswer;
    private bool usingGlassesTmp = false;

    //lock room 2
    private bool onLockR2 = false;
    private bool moveToLockR2 = false;
    private Vector3 lockR2Pos;
    private Vector3 lockR2TopRight;

    private bool onObject = false;


    public ShowUI()
    {
        //initialise vairables
        ballToCamera = Camera.main.transform.position + Camera.main.transform.forward;
        boxTopIniPos = new Vector3(0, 0.2625f, 0);
        plankTopRight = plank.First().transform.position + plank.First().transform.localScale/2;
        lockR2TopRight = lockR2.First().transform.position + lockR2.First().transform.localScale / 2;
        lr = plank.First().GetComponent<LineRenderer>();
        lrPositions = new List<Vector3>();

        foreach (GameObject button in buttons)
        {
            if(button.name == "Close")
            {
                //add listener to ui's close button
                button.GetComponent<Button>().onClick.AddListener(CloseWindow);
                closeButton = button;
            }
        }

        foreach (GameObject c in ui)
        {
            if (c.name == "UI")
            {
                uiGO = c;
                c.SetActive(false); //hide ui
            }
            else if (c.name == "Cursor")
            {
                c.SetActive(true);  //display cursor
                cursorUI = c;
            }
            else if (c.name == "Timer")
            {
                c.SetActive(true);  //display timer
            }
        }
        Ball b = null;
        int i = 0;
        foreach(GameObject go in balls)
        {
            b = go.GetComponent<Ball>();
            b.initialPosition = go.transform.localPosition; //set initial position to current local position
            b.id = i;   //set ball id
            i++;
            go.GetComponent<Renderer>().material.color = Random.ColorHSV() + Color.white/2.5f;  //set color to random color
            b.color = go.GetComponent<Renderer>().material.color;
            //init text and number
        }

        foreach(Transform child in box.First().transform)
        {
            if(child.gameObject.name == "Padlock")
            {
                boxPadlock = child.gameObject;
            }
        }
        foreach (Transform child in bag.First().transform)
        {
            if (child.gameObject.name == "Padlock")
            {
                bagPadlock = child.gameObject;
            }
        }
        foreach(Transform child in inventory.First().transform) {
            if(child.gameObject.name == "GlassesBackground")
            {
                foreach(Transform c in child)
                {
                    bagAnswer = c.gameObject;
                }
            }
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
        if (onBag)
        {
            if (usingGlassesTmp && !CollectableGO.usingGlasses)
            {
                bagAnswer.SetActive(false);
            }
            else if (!usingGlassesTmp && CollectableGO.usingGlasses)
            {
                bagAnswer.SetActive(true);
            }
        }
        usingGlassesTmp = CollectableGO.usingGlasses;

        if (moveToPlank)
        {
            //animation to move the player in front of the plank
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, plankPos, speed);
            camNewDir = Vector3.left;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            //when the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && player.First().transform.position == plankPos)
            {
                //correct the rotation
                newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.left, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                //show ui and set close button position
                closeButton.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(plankTopRight) - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                uiGO.SetActive(true);
                moveToPlank = false;
            }
        }
        else if (moveBox)
        {
            //box animations//
            if (openBox)
            {
                //animation to open the box
                boxTop.First().transform.position = Vector3.MoveTowards(boxTop.First().transform.position, boxTopPos, speed / 10);
                //when the animation if finished
                if(boxTop.First().transform.position == boxTopPos)
                {
                    openBox = false;
                    takingBalls = true; //start taking out balls
                    tmpCount = 1;
                }
            }
            else if (takingBalls)
            {
                //animation to take balls out of the box
                Ball b = null;
                foreach(GameObject ball in balls)
                {
                    b = ball.GetComponent<Ball>();
                    ball.GetComponent<Rigidbody>().isKinematic = true;
                    if (b.id < tmpCount) //the animation doesn't take out all balls at the same time
                    {
                        if (b.outOfBox) //if the ball is out of the box
                        {
                            //move the ball to the position corresponding to its id
                            ballPos = Vector3.up * ((float)balls.Count/10 - (float)(b.id/5)/3) + Vector3.right * ((float)(b.id%5) * -2f / 4+1f)+Vector3.forward*0.2f;
                            ball.transform.localPosition = Vector3.MoveTowards(ball.transform.localPosition, ballPos, speed);
                            //when the last ball arrives to its position
                            if (ball.transform.localPosition == ballPos && b.id == balls.Count - 1)
                            {
                                //stop animations
                                takingBalls = false;
                                moveBox = false;
                                ballsout = true;
                                //show ui and set "close" button position
                                boxTopRight = Vector3.up * ((float)balls.Count / 10 + 0.3f) + Vector3.right * -1.3f + Vector3.forward * 0.2f;
                                closeButton.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(box.First().transform.TransformPoint(boxTopRight)) - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                                uiGO.SetActive(true);
                            }
                        }
                        else //if the ball is still in the box
                        {
                            //animation to move the ball to box's aperture
                            ball.transform.localPosition = Vector3.MoveTowards(ball.transform.localPosition, Vector3.up, speed);
                            ball.transform.localRotation = Quaternion.Euler(Vector3.up*-90);
                            //when the ball arrives
                            if(ball.transform.localPosition == Vector3.up)
                            {
                                b.outOfBox = true;
                                tmpCount++; //with this the next ball will start moving
                            }
                        }
                    }
                }
            }
            else if (unlockBox)
            {
                dist = 1.5f - boxPadlock.transform.localPosition.y;
                boxPadlock.transform.localPosition = Vector3.MoveTowards(boxPadlock.transform.localPosition, boxPadlock.transform.localPosition + Vector3.up * dist, (dist + 1)/100);
                boxPadlock.transform.localRotation = Quaternion.Euler(boxPadlock.transform.localRotation.eulerAngles+Vector3.up* (boxPadlock.transform.localPosition.y - 0.2f) *35);
                if(boxPadlock.transform.localPosition.y > 1.4f)
                {
                    boxPadlock.SetActive(false);
                    unlockBox = false;
                    CollectableGO.usingKeyE03 = false;
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Display" || child.gameObject.name == "Selected")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    foreach(GameObject go in cGO)
                    {
                        if(go.name == "KeyE03")
                        {
                            go.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                if (player.First().transform.localScale.x < 0.9f)
                {
                    //if the player is crouching, move him to the standing height 
                    player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, player.First().transform.position + Vector3.up * (1.6f - player.First().transform.position.y), (1.6f - 0.26f) * speed / dist);
                }
                //animation to move the box in front of the player
                box.First().transform.position = Vector3.MoveTowards(box.First().transform.position, objectPos, speed);
                newDir = Vector3.RotateTowards(box.First().transform.forward, -player.First().transform.forward, Mathf.Deg2Rad * speedRotation, 0);
                box.First().transform.rotation = Quaternion.LookRotation(newDir);
                Camera.main.transform.localRotation = Quaternion.Euler(Vector3.MoveTowards(Camera.main.transform.localRotation.eulerAngles, Vector3.zero, speedRotation2));
                //when the box arrives
                if (box.First().transform.position == objectPos && Vector3.Angle(box.First().transform.forward, -player.First().transform.forward)<1)
                {
                    if (!boxPadlock.activeSelf)
                    {
                        objectPos += Vector3.up * (1.78f - 1 - objectPos.y);
                        if(box.First().transform.position == objectPos)
                        {
                            openBox = true; //start opening box
                        }
                    }
                    else
                    {
                        boxTopRight = Vector3.up * ((float)balls.Count / 10 + 0.3f) + Vector3.right * -1.3f + Vector3.forward * 0.2f;
                        closeButton.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(box.First().transform.TransformPoint(boxTopRight)) - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                        uiGO.SetActive(true);
                        objectPos += Vector3.up*(1.78f - 0.5f -objectPos.y);
                        if (CollectableGO.usingKeyE03)
                        {
                            unlockBox = true;
                        }
                    }
                    boxTopPos = boxTop.First().transform.position + box.First().transform.right * boxTop.First().transform.localScale.x; //set lid position
                    ballToCamera = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0.5f));  //set position of balls when in front of the screen
                }
            }
        }
        else if (moveBall)
        {
            //animation to move the selected ball
            if (ballFocused)
            {
                //if the ball is selected, move it in front of the camera and rotate it to make its back visible
                focusedBall.transform.position = Vector3.MoveTowards(focusedBall.transform.position, ballToCamera, speed / 2);
                focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(0, 90, 0), speedRotation);
                //when the ball arrives
                if (focusedBall.transform.position == ballToCamera)
                {
                    moveBall = false;
                    //hide the text behind
                    foreach (Transform child in focusedBall.transform)
                    {
                        if (child.gameObject.name == "Text")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                //if the ball is not selected, move it back with the other balls and rotate it back
                ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) + Vector3.forward * 0.2f;
                focusedBall.transform.localPosition = Vector3.MoveTowards(focusedBall.transform.localPosition, ballPos, speed / 2);
                focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(0, -90, 0), speedRotation);
                //when the ball arrives
                if (focusedBall.transform.localPosition == ballPos)
                {
                    moveBall = false;
                    //hide the number behind
                    foreach (Transform child in focusedBall.transform)
                    {
                        if (child.gameObject.name == "Number")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        else if (moveTablet)
        {
            float d = 0.33f;
            //calculate the distance of the tablet from the camera depending on the screen size
            if ((float)Camera.main.pixelWidth / 900 < (float)Camera.main.pixelHeight / 600)
            {
                d = aTabletDist * (float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth + bTabletDist;
            }
            else
            {
                d = 0.56f;
            }
            //animation to move the tablet in front of the player
            Vector3 vec = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            vec.Normalize();
            objectPos = new Vector3(player.First().transform.position.x, 1.78f, player.First().transform.position.z) + vec * (d);
            selectedTablet.transform.position = Vector3.MoveTowards(selectedTablet.transform.position, objectPos, speed);
            newDir = Vector3.RotateTowards(selectedTablet.transform.forward, player.First().transform.forward, Mathf.Deg2Rad * speedRotation, 0);
            selectedTablet.transform.rotation = Quaternion.LookRotation(newDir);
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.MoveTowards(Camera.main.transform.localRotation.eulerAngles, Vector3.zero, speedRotation2));
            //when the tablet arrives
            if (selectedTablet.transform.position == objectPos)
            {
                moveTablet = false;
                //show ui and put the "close" button at the top right of the screen
                tabletTopRight = Vector3.up * Camera.main.pixelHeight + Vector3.right * Camera.main.pixelWidth;
                closeButton.GetComponent<RectTransform>().localPosition = tabletTopRight - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                uiGO.SetActive(true);
                //put the ui "screen" on the screen (rather than on the tablet)
                tabletScreen.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            }
        }
        else if (moveToTable)
        {
            player.First().GetComponent<Rigidbody>().detectCollisions = false; //disable player collision to avoid collision with tables during animation
            if (onTable)
            {
                //animation to move the player above the table
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, onTablePoint, speed);
                Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation, Quaternion.Euler(90,0,0), speedRotation);
                player.First().transform.rotation = Quaternion.RotateTowards(player.First().transform.rotation, Quaternion.Euler(0, focusedTable.transform.rotation.eulerAngles.y, 0), speedRotation2);
                //when the player reaches the position
                if (player.First().transform.position == onTablePoint)
                {
                    //show ui and set "close" button's position
                    closeButton.GetComponent<RectTransform>().localPosition = Vector3.up * Camera.main.pixelHeight + Vector3.right * Camera.main.pixelWidth - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                    uiGO.SetActive(true);
                    moveToTable = false;
                }
            }
            else
            {
                //animation to move the player back at its position
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, posBeforeTable, speed);
                Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation, rotBeforeTable, speedRotation);
                player.First().transform.rotation = Quaternion.RotateTowards(player.First().transform.rotation, rot2BeforeTable, speedRotation2);
                //when player reaches its position
                if (player.First().transform.position == posBeforeTable)
                {
                    //enable moving mode and collisions
                    moveToTable = false;
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    player.First().GetComponent<Rigidbody>().detectCollisions = true;
                }
            }
        }
        else if (moveBag)
        {
            if (unlockBag)
            {
                dist = 1.7f - boxPadlock.transform.localPosition.y;
                bagPadlock.transform.localPosition = Vector3.MoveTowards(bagPadlock.transform.localPosition, bagPadlock.transform.localPosition + Vector3.up * dist, (dist + 1) / 100);
                bagPadlock.transform.localRotation = Quaternion.Euler(bagPadlock.transform.localRotation.eulerAngles + Vector3.up * (bagPadlock.transform.localPosition.y - 0.3f) * 35);
                if (bagPadlock.transform.localPosition.y > 1.6f)
                {
                    bagPadlock.SetActive(false);
                    unlockBag = false;
                    CollectableGO.usingKeyE08 = false;
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Display" || child.gameObject.name == "Selected")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    foreach (GameObject go in cGO)
                    {
                        if (go.name == "KeyE08")
                        {
                            go.SetActive(false);
                        }
                    }
                }
            }
            else if (showBagPaper)
            {
                if(onBag)
                {
                    bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition = Vector3.MoveTowards(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition, Vector3.up*0.9f, speed / 10);
                    if(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.y == 0.9f)
                    {
                        showBagPaper = false;
                        moveBag = false;
                        bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition += Vector3.up * (bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.InverseTransformPoint(Camera.main.transform.position).y - bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.y);
                        bag.First().GetComponentInChildren<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                        bagTopRight = Vector3.up * Camera.main.pixelHeight/2 + Vector3.right * Camera.main.pixelWidth/2 + (Vector3.up + Vector3.right) * bag.First().GetComponentInChildren<Image>().gameObject.GetComponent<RectTransform>().rect.width*0.225f;
                        //closeButton.GetComponent<RectTransform>().localPosition = bagTopRight - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0)/2;
                        closeButton.GetComponent<RectTransform>().localPosition = Vector3.up * Camera.main.pixelHeight + Vector3.right * Camera.main.pixelWidth - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                        if (CollectableGO.usingGlasses)
                        {
                            bagAnswer.SetActive(true);
                        }
                    }
                }
                else
                {
                    bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition = Vector3.MoveTowards(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition, Vector3.zero, speed / 5);
                    if (bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.y == 0f)
                    {
                        player.First().GetComponent<FirstPersonController>().enabled = true;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        showBagPaper = false;
                        moveBag = false;
                        bag.First().GetComponent<Rigidbody>().isKinematic = false;
                    }
                }
            }
            else
            {
                if (player.First().transform.localScale.x < 0.9f)
                {
                    //if the player is crouching, move him to the standing height 
                    player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, player.First().transform.position + Vector3.up * (1.6f - player.First().transform.position.y), (1.6f - 0.26f) * speed / dist);
                }
                //animation to move the bag in front of the player
                bag.First().transform.position = Vector3.MoveTowards(bag.First().transform.position, objectPos, speed);
                newDir = Vector3.RotateTowards(bag.First().transform.forward, player.First().transform.forward, Mathf.Deg2Rad * speedRotation, 0);
                bag.First().transform.rotation = Quaternion.LookRotation(newDir);
                Camera.main.transform.localRotation = Quaternion.Euler(Vector3.MoveTowards(Camera.main.transform.localRotation.eulerAngles, Vector3.zero, speedRotation2));
                //when the box arrives
                if (bag.First().transform.position == objectPos && Vector3.Angle(bag.First().transform.forward, player.First().transform.forward)<1)
                {
                    //show ui and set close button position
                    if (bagPadlock.activeSelf)
                    {
                        bagTopRight = bag.First().transform.position + bag.First().transform.up * bag.First().transform.localScale.y + bag.First().transform.right * bag.First().transform.localScale.x;
                    }
                    closeButton.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(bagTopRight) - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                    uiGO.SetActive(true);
                    if (!bagPadlock.activeSelf)
                    {
                        showBagPaper = true;
                    }
                    else
                    {
                        objectPos += Vector3.up * (1.78f - 0.8f - objectPos.y);
                        if (CollectableGO.usingKeyE08)
                        {
                            unlockBag = true;
                        }
                    }
                }
            }
        }
        else if (moveToLockR2)
        {
            //animation to move the player in front of the lock room 2
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, lockR2Pos, speed);
            camNewDir = Vector3.right;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            //when the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && player.First().transform.position == lockR2Pos)
            {
                //correct the rotation
                newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.right, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                //show ui and set close button position
                closeButton.GetComponent<RectTransform>().localPosition = Camera.main.WorldToScreenPoint(lockR2TopRight) - new Vector3(closeButton.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, closeButton.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                uiGO.SetActive(true);
                lockR2.First().GetComponentInChildren<InputField>().ActivateInputField();
                moveToLockR2 = false;
            }
        }

        if (noSelection)
        {
            foreach (GameObject go in objects)
            {
                if (go.GetComponent<Selectable>().isSelected)   //if a gameobject is selected
                {
                    noSelection = false;
                    //set and show ui
                    uiGO.SetActive(true);
                    if (go.tag == "Object") //set ui for gameobjects with tag "object"
                    {
                        foreach (Transform child in uiGO.transform)
                        {
                            if (child.gameObject.name == "Window")
                            {
                                //show a window (used in the first prototype)
                                child.gameObject.SetActive(true);
                                onObject = true;
                            }
                        }
                    }
                    else if(go.tag == "Plank")  //set plank
                    {
                        //the position in front of the plank is not the same depending on the scale of the player
                        if(player.First().transform.localScale.x < 0.9f)
                        {
                            plankPos = new Vector3(plank.First().transform.position.x + 1.5f, 1.6f, plank.First().transform.position.z);
                        }
                        else
                        {
                            plankPos = new Vector3(plank.First().transform.position.x + 1.5f, 0.98f, plank.First().transform.position.z);
                        }
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (plankPos - player.First().transform.position).magnitude;
                        foreach (Transform t in player.First().transform)
                        {
                            if (t.gameObject.tag == "MainCamera")
                            {
                                speedRotation = Vector3.Angle(t.gameObject.transform.forward, Vector3.left) * speed / dist;
                            }
                        }
                        moveToPlank = true; //start animation to move the player in front of the plank
                        uiGO.SetActive(false); //hide ui during animation
                        onPlank = true;
                    }
                    else if(go.tag == "Box")    //set box
                    {
                        go.GetComponent<Rigidbody>().isKinematic = true;
                        //calculate the position in front of the player
                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                        v.Normalize();
                        objectPos = new Vector3(player.First().transform.position.x, 1.78f-1, player.First().transform.position.z) + v * (go.transform.localScale.y + 1.5f);
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (objectPos - go.transform.position).magnitude;
                        speedRotation = Vector3.Angle(box.First().transform.forward, -player.First().transform.forward) * speed / dist;
                        speedRotation2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
                        moveBox = true; //start animation to move the box in front of the player
                        uiGO.SetActive(false); //hide ui during animation
                        onBox = true;
                    }
                    else if(go.tag == "Tablet") //set tablet
                    {
                        selectedTablet = go;
                        tabletScreen = selectedTablet.GetComponentInChildren<Canvas>().gameObject;
                        selectedTablet.GetComponent<Rigidbody>().isKinematic = true;
                        //calculate the distance between the tablet and the player depending on the screen size
                        //(not really necessary since the tablet screen is displayed on the screen and not in the world)
                        float d = 0.33f;
                        if ((float)Camera.main.pixelWidth / 900 < (float)Camera.main.pixelHeight / 600)
                        {
                            d = aTabletDist * (float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth + bTabletDist;
                        }
                        else
                        {
                            d = 0.56f;
                        }
                        //calculate the position in front of the player
                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                        v.Normalize();
                        objectPos = new Vector3(player.First().transform.position.x, 1.78f, player.First().transform.position.z) + v * (d);
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (objectPos - go.transform.position).magnitude;
                        speedRotation = Vector3.Angle(selectedTablet.transform.forward, player.First().transform.forward) * speed / dist;
                        speedRotation2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
                        uiGO.SetActive(false); //hide ui during animation
                        onTablet = true;
                        moveTablet = true;  //start animation to move the tablet in front of the player
                    }
                    else if (go.tag == "TableE05")  //set tables of enigma 5
                    {
                        /* set the position where the player will be moved depending on the selected table
                            * the position is chosen so that when the tables are assembled, the formed picture is in the center of the screen
                            */
                        if (go.name.Contains(1.ToString()))
                        {
                            //front left of the table
                            onTablePoint = go.transform.position - go.transform.right * 0.75f + go.transform.forward * 0.75f + Vector3.up * (go.transform.localScale.y * 0.55f + 1);
                            dist = (onTablePoint - player.First().transform.position).magnitude;
                        }
                        else if (go.name.Contains(2.ToString()))
                        {
                            //front right of the table
                            onTablePoint = go.transform.position + go.transform.right * 0.75f + go.transform.forward * 0.75f + Vector3.up * (go.transform.localScale.y * 0.55f + 1);
                            dist = (onTablePoint - player.First().transform.position).magnitude;
                        }
                        else if (go.name.Contains(3.ToString()))
                        {
                            //back of the table (back according to unity axes)
                            onTablePoint = go.transform.position - go.transform.forward * 0.75f + Vector3.up * (go.transform.localScale.y * 0.55f + 1);
                            dist = (onTablePoint - player.First().transform.position).magnitude;
                        }
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        speedRotation = Vector3.Angle(Camera.main.transform.forward, Vector3.down) * speed / dist;
                        speedRotation2 = Vector3.Angle(player.First().transform.forward, go.transform.forward) * speed / dist;
                        //save rotation and position of the player before moving
                        posBeforeTable = player.First().transform.position;
                        rotBeforeTable = Camera.main.transform.localRotation;
                        rot2BeforeTable = player.First().transform.rotation;
                        focusedTable = go;  //store the selected table
                        uiGO.SetActive(false); //hide ui during animation
                        onTable = true;
                        moveToTable = true; //start animation to move the playre above the table
                    }
                    else if(go.tag == "Sheet")  //set sheets of enigma 6 and 7
                    {
                        focusedSheet = go;  //store the selected sheet
                        onSheet = true;
                        foreach (Transform ch in uiGO.transform)
                        {
                            if (ch.gameObject.name == "Close")
                            {
                                //set the position of the "close" button
                                //if (go.GetComponentsInChildren<Image>().Length == 3)
                                ch.gameObject.GetComponent<RectTransform>().localPosition = Vector3.up * Camera.main.pixelHeight + Vector3.right * Camera.main.pixelWidth;
                                ch.gameObject.GetComponent<RectTransform>().localPosition = ch.gameObject.GetComponent<RectTransform>().localPosition - new Vector3(ch.gameObject.GetComponent<RectTransform>().rect.width + Camera.main.pixelWidth, ch.gameObject.GetComponent<RectTransform>().rect.height + Camera.main.pixelHeight, 0) / 2;
                            }
                        }
                        Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0); //set the x rotation of the player to 0 (make an animation rather than this)
                        rotBeforeSheet = focusedSheet.transform.rotation;   //the the rotation of the sheet before selection
                        //rotate the sheet toward the player before displaying it (else it would be distorted in the ui)
                        focusedSheet.transform.rotation = Quaternion.RotateTowards(focusedSheet.transform.rotation,Quaternion.Euler(0, player.First().transform.rotation.eulerAngles.y-90, 0), 360);
                        foreach(Transform canvas in focusedSheet.transform)
                        {
                            if(canvas.gameObject.name == "Display")
                            {
                                //show the sheet in an ui
                                canvas.GetComponent<Canvas>().planeDistance = 0.33f;
                                canvas.gameObject.SetActive(true);
                            }
                        }
                    }
                    else if(go.tag == "Bag")
                    {
                        go.GetComponent<Rigidbody>().isKinematic = true;
                        //calculate the position in front of the player
                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                        v.Normalize();
                        objectPos = new Vector3(player.First().transform.position.x, 1.78f - 0.5f, player.First().transform.position.z) + v * (go.transform.localScale.y + 1.5f);
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (objectPos - go.transform.position).magnitude;
                        speedRotation = Vector3.Angle(bag.First().transform.forward, player.First().transform.forward) * speed / dist;
                        speedRotation2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
                        moveBag = true; //start animation to move the bag in front of the player
                        uiGO.SetActive(false); //hide ui during animation
                        onBag = true;
                    }
                    else if (go.tag == "LockRoom2")  //set lock room 2
                    {
                        //the position in front of the lock is not the same depending on the scale of the player
                        if (player.First().transform.localScale.x < 0.9f)
                        {
                            lockR2Pos = new Vector3(lockR2.First().transform.position.x - 1.5f, 1.6f, lockR2.First().transform.position.z);
                        }
                        else
                        {
                            lockR2Pos = new Vector3(lockR2.First().transform.position.x - 1.5f, 0.98f, lockR2.First().transform.position.z);
                        }
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (lockR2Pos - player.First().transform.position).magnitude;
                        foreach (Transform t in player.First().transform)
                        {
                            if (t.gameObject.tag == "MainCamera")
                            {
                                speedRotation = Vector3.Angle(t.gameObject.transform.forward, Vector3.right) * speed / dist;
                            }
                        }
                        moveToLockR2 = true; //start animation to move the player in front of the lock
                        uiGO.SetActive(false); //hide ui during animation
                        onLockR2 = true;
                    }
                    //hide the cursor when an object is selected
                    cursorUI.SetActive(false);
                    //disable player moves
                    player.First().GetComponent<FirstPersonController>().enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
            }
        }
        else    //if "noselection" is false
        {
            if(onPlank && CollectableGO.usingWire)
            {
                pointerOverWord = false;
                foreach(GameObject word in plankWords)
                {
                    if (word.GetComponent<PointerOver>())
                    {
                        pointerOverWord = true;
                        if (Input.GetMouseButtonDown(0))    //if a selectable word is clicked
                        {
                            //if the word is selected (color red)
                            if (word.GetComponent<TextMeshPro>().color == Color.red)
                            {
                                //unselect it
                                word.GetComponent<TextMeshPro>().color = Color.black;
                                //remove the vertex from the linerenderer
                                lr.positionCount--;
                                //if there were 4 points, remove a second one (should use linerenderer loop rather than this)
                                if (lr.positionCount == 3)
                                {
                                    lr.positionCount--;
                                    lrPositions.RemoveAt(lrPositions.LastIndexOf(lrFirstPosition));
                                }
                                if(word.transform.position == lrFirstPosition)
                                {
                                    lrPositions.Remove(word.transform.position);
                                    lrFirstPosition = lrPositions.Find(ReturnFirst);
                                }
                                else
                                {
                                    lrPositions.Remove(word.transform.position);
                                }
                                //set the new positions
                                lr.SetPositions(lrPositions.ToArray());
                            }
                            else    //if the word wasn't selected
                            {
                                if (lr.positionCount > 2)
                                {
                                    //if there is already 3 selected words, unselect them and select the new one
                                    foreach (GameObject w in plankWords)
                                    {
                                        w.GetComponent<TextMeshPro>().color = Color.black;
                                    }
                                    lr.positionCount = 0;
                                    lrPositions.Clear();
                                }
                                word.GetComponent<TextMeshPro>().color = Color.red;
                                //update the linerenderer (will be changed with linrenderer loop)
                                lr.positionCount++;
                                lrPositions.Add(word.transform.position);
                                if(lr.positionCount == 1)
                                {
                                    lrFirstPosition = word.transform.position;
                                }
                                if (lr.positionCount == 3)
                                {
                                    lr.positionCount++;
                                    lrPositions.Add(lrFirstPosition);
                                }
                                lr.SetPositions(lrPositions.ToArray());
                            }
                        }
                        else    //if mouse over a word without click
                        {
                            if(word.GetComponent<TextMeshPro>().color != Color.red)
                            {
                                //if the word isn't selected change its color to yellow
                                word.GetComponent<TextMeshPro>().color = Color.yellow;
                            }
                        }
                    }
                    else    //if mouse isn't over a word
                    {
                        if (word.GetComponent<TextMeshPro>().color != Color.red)
                        {
                            //if the word isn't selected change its color to black (initial)
                            word.GetComponent<TextMeshPro>().color = Color.black;
                        }
                    }
                }
                if (!pointerOverWord && Input.GetMouseButtonDown(0))
                {
                    //if click over nothing unselect all
                    foreach (GameObject word in plankWords)
                    {
                        word.GetComponent<TextMeshPro>().color = Color.black;
                        lr.positionCount = 0;
                        lrPositions.Clear();
                    }
                }
            }
            else if (onBox)
            {
                if (ballsout)   //if all balls are out of the box
                {
                    Ball b = null;
                    if (ballFocused)    //if there is a selected ball
                    {
                        if(Input.GetMouseButtonDown(0) && !moveBall)
                        {
                            //onclick if the ball isn't moving, move it back to its position with other balls
                            ballFocused = false;
                            moveBall = true;
                            foreach (Transform child in focusedBall.transform)
                            {
                                if (child.gameObject.name == "Text")
                                {
                                    child.gameObject.SetActive(true);
                                }
                            }
                            //calculate position and speeds for the animation
                            ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) + Vector3.forward * 0.2f;
                            dist = (box.First().transform.TransformPoint(ballPos) - ballToCamera).magnitude;
                            speedRotation = 180 * speed / dist/2;
                        }
                    }
                    else if(!moveBall)  //if there isn't animations and selected ball
                    {
                        foreach (GameObject ballGO in balls)
                        {
                            b = ballGO.GetComponent<Ball>();
                            if (ballGO.GetComponent<PointerOver>())
                            {
                                //if pointer over a ball change its color to yellow
                                ballGO.GetComponent<Renderer>().material.color = Color.yellow + Color.white / 4;
                                if (Input.GetMouseButtonDown(0))    //if a ball is clicked
                                {
                                    //move it in front of the camera
                                    ballFocused = true;
                                    moveBall = true;
                                    focusedBall = ballGO;
                                    foreach(Transform child in focusedBall.transform)
                                    {
                                        if(child.gameObject.name == "Number")
                                        {
                                            child.gameObject.SetActive(true);
                                        }
                                    }
                                    ballGO.GetComponent<Renderer>().material.color = b.color; //initial color
                                    //calculate position and speeds for the animation
                                    ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) + Vector3.forward * 0.2f;
                                    dist = (box.First().transform.TransformPoint(ballPos) - ballToCamera).magnitude;
                                    speedRotation = 180 * speed / dist/2;
                                }
                            }
                            else
                            {
                                //if there isn't animations, mouse over or click, set to initial color
                                ballGO.GetComponent<Renderer>().material.color = b.color;
                            }
                        }
                    }
                }
            }
            else if (onLockR2)
            {
                if (lockR2.First().GetComponent<Selectable>().solved)
                {
                    CloseWindow();
                }
            }
        }
	}

    private void CloseWindow()
    {
        if (!onTable && !onBag)
        {
            //enable player
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (onObject)
        {
            foreach (Transform child in uiGO.transform)
            {
                if (child.gameObject.name == "Window")
                {
                    //hide window
                    child.gameObject.SetActive(false);
                }
            }
        }
        else if (onPlank)
        {
            player.First().transform.forward = -plank.First().transform.right;
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        else if(onBox)
        {
            //close box, set balls to initial position and everything to not kinematic
            boxTop.First().transform.localPosition = boxTopIniPos;
            box.First().GetComponent<Rigidbody>().isKinematic = false;
            foreach(GameObject go in balls)
            {
                go.transform.localPosition = go.GetComponent<Ball>().initialPosition;
                go.GetComponent<Ball>().outOfBox = false;
                go.GetComponent<Rigidbody>().isKinematic = false;
            }
            tmpCount = 1;
            moveBox = false;
        }
        else if (onTablet)
        {
            selectedTablet.GetComponent<Rigidbody>().isKinematic = false;
            //put the screen on the tablet
            tabletScreen.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            tabletScreen.GetComponent<RectTransform>().localPosition = Vector3.forward * -0.026f;
            tabletScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(900,600);
            tabletScreen.GetComponent<RectTransform>().localScale = Vector3.one * -0.0008327437f;
            tabletScreen.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 180);
        }
        else if (onTable)
        {
            //start animation to move the player back to its position
            moveToTable = true;
        }
        else if (onSheet)
        {
            //set sheet to initial rotation and hide ui
            focusedSheet.transform.rotation = rotBeforeSheet;
            foreach (Canvas canvas in focusedSheet.GetComponentsInChildren<Canvas>())
            {
                if (canvas.gameObject.name == "Display")
                {
                    canvas.gameObject.SetActive(false);
                }
            }
        }
        else if (onBag)
        {
            bag.First().GetComponent<Rigidbody>().isKinematic = false;
            moveBag = false;
            if(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.y != 0)
            {
                bagAnswer.SetActive(false);
                showBagPaper = true;
                moveBag = true;
                bag.First().GetComponent<Rigidbody>().isKinematic = true;
                bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition += Vector3.up * (0.9f - bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.y);
                bag.First().GetComponentInChildren<Canvas>().renderMode = RenderMode.WorldSpace;
                bag.First().GetComponentInChildren<Canvas>().gameObject.GetComponent<RectTransform>().localPosition = Vector3.forward * (-0.51f - bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.z);
                bag.First().GetComponentInChildren<Canvas>().gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 0.000535786f;
            }
            else
            {
                //enable player
                player.First().GetComponent<FirstPersonController>().enabled = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        uiGO.SetActive(false);
        //show cursor
        cursorUI.SetActive(true);
        foreach (GameObject go in objects)
        {
            if (go.GetComponent<Selectable>().isSelected)
            {
                //unselect the gameobject
                go.GetComponent<Selectable>().isSelected = false;
                Selectable.selected = false;
                noSelection = true;
                break;
            }
        }
        onObject = false;
        onSheet = false;
        onPlank = false;
        onBox = false;
        onTablet = false;
        onTable = false;
        onBag = false;
        onLockR2 = false;
    }

    //predicate to return the first vector of the list
    private bool ReturnFirst(Vector3 v)
    {
        return true;
    }
}