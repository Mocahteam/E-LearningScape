using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using TMPro;
using UnityEngine.PostProcessing;

public class ShowUI : FSystem {
    
    //all selectable objects
    private Family objects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)));

    private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
    private Family inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(InputField)));
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
    private Family lockR2Wheels = FamilyManager.getFamily(new AnyOfTags("LockR2Wheel"));
    private Family closePlank = FamilyManager.getFamily (new AnyOfTags ("Plank", "PlankText", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family closeBox = FamilyManager.getFamily (new AnyOfTags ("Box", "Ball", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family overInventoryElem = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(PointerOver)));
	private Family e05Pieces = FamilyManager.getFamily(new AnyOfTags("E05UI"));
	private Family carillon = FamilyManager.getFamily(new AnyOfTags("Carillon"));
    private Family boardFamilly = FamilyManager.getFamily(new AnyOfTags("Board"));
    private Family closeBoard = FamilyManager.getFamily(new AnyOfTags("Board", "Eraser"), new AllOfComponents(typeof(PointerOver)));
    private Family boardRemovableWords = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));

    private bool noSelection = true;    //true if all objects are unselected
    private GameObject uiGO;
    private GameObject cursorUI;
    public static bool askCloseWindow = false;

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
    private bool pointerOverWord = false;   //true when the pointer is over a selectable word
    private LineRenderer lr;                //used to link words
    private List<Vector3> lrPositions;
    private float plankSubtitlesTimer = -Mathf.Infinity;
    private GameObject plankSubtitle;

    //box
    private bool onBox = false;                 //true when the player is using the box
    private bool moveBox = false;               //true during the animation to move the box in front of the player
    private bool openBox = false;               //true during the animation to open the box
    private bool takingBalls = false;           //true during the animation to take out balls from the box
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

    //tablet
    private GameObject selectedTablet;
    private GameObject tabletScreen;
    private bool onTablet = false;                  //true when the player is using the tablet
    private bool moveTablet = false;                //true during the animation to move the tablet in front of the player
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
	private GameObject tableUI;
	private GameObject draggedE05 = null;
	private Vector3 posBeforeDragE05;
	private Vector3 posFromMouseE05;

    //sheets
    private bool onSheet = false;       //true when the player selected a sheet
    private GameObject focusedSheet;    //store the focused sheet
    private Quaternion rotBeforeSheet;  //rotation of the sheet before selection

    //bag
    private bool onBag = false;                   //true when the player selected a table
    private bool moveBag = false;               //true during the animation to move the player above the table
    private bool unlockBag = false;
    private GameObject bagPadlock;
    private bool showBagPaper = false;
    private bool usingGlassesTmp1 = false;
    private bool usingGlassesTmp2 = false;
    private bool onBagPaper = false;
    private Vector3 bagPaperInitialPos;

    //lock room 2
    private bool onLockR2 = false;
    private bool moveToLockR2 = false;
    private Vector3 lockR2Pos;
    private GameObject lockWheel1;
    private GameObject lockWheel2;
    private GameObject lockWheel3;
    private GameObject lockUD;
    private GameObject lockLR;
    private GameObject selectedWheel;
    private bool lockRotationUp = false;
    private bool lockRotationDown = false;
    private Color lockWheelColor;
    private int lockNumber1 = 0;
    private int lockNumber2 = 0;
    private int lockNumber3 = 0;
    private int wheelRotationCount = 0;

    //carillon
    private bool onCarillon = false;
	private GameObject carillonImage;

    //board
    private GameObject board;
    private bool onBoard = false;
    private bool moveToBoard = false;
    private Vector3 boardPos;
    private GameObject eraser;
    private bool eraserDragged = false;
    private float distToBoard;

	private GameObject forGO;
	private GameObject forGO2;

    public ShowUI()
    {
        //initialise vairables
        ballToCamera = Camera.main.transform.position + Camera.main.transform.forward;
        boxTopIniPos = boxTop.First().transform.localPosition;
        boxTopPos = boxTop.First().transform.localPosition + boxTop.First().transform.right - boxTop.First().transform.up/2; //set lid position
        lr = plank.First().GetComponent<LineRenderer>();
        lrPositions = new List<Vector3>();
        ballSubTitles = box.First().GetComponentInChildren<Canvas>().gameObject.GetComponentInChildren<TextMeshProUGUI>();
        bagPaperInitialPos = bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition;
        board = boardFamilly.First();

		int nb = ui.Count;
		for(int i = 0; i < nb; i++)
        {
			forGO = ui.getAt (i);
			if (forGO.name == "UI")
            {
				uiGO = forGO;
				foreach (Transform child in uiGO.transform) {
					if (child.gameObject.name == "E05") {
						tableUI = child.gameObject;
						break;
					}
				}
            }
			else if (forGO.name == "Cursor")
            {
				forGO.SetActive(true);  //display cursor
				cursorUI = forGO;
            }
			else if (forGO.name == "Timer")
            {
				forGO.SetActive(true);  //display timer
            }
        }
        Ball b = null;
        int j = 0;
		nb = balls.Count;
		for(int i = 0; i < nb; i++)
        {
			forGO = balls.getAt (i);
			b = forGO.GetComponent<Ball>();
			b.initialPosition = forGO.transform.localPosition; //set initial position to current local position
            b.id = j;   //set ball id
            j++;
			forGO.GetComponent<Renderer>().material.color = Random.ColorHSV() + Color.white*0.6f;  //set color to random color
			b.color = forGO.GetComponent<Renderer>().material.color;
            //init text and number
            foreach(Transform child in forGO.transform)
            {
                if(child.gameObject.name == "Text")
                {
                    b.text = child.gameObject.GetComponent<TextMeshPro>().text.Replace("\n", " ");
                }
                else if(child.gameObject.name == "Number")
                {
                    int.TryParse(child.gameObject.GetComponent<TextMeshPro>().text, out b.number);
                }
            }
        }
        nb = lockR2Wheels.Count;
        for(int i = 0; i <nb; i++)
        {
            forGO = lockR2Wheels.getAt(i);
            if (forGO.name.Contains(1.ToString()))
            {
                lockWheel1 = forGO;
            }
            else if (forGO.name.Contains(2.ToString()))
            {
                lockWheel2 = forGO;
            }
            else if (forGO.name.Contains(3.ToString()))
            {
                lockWheel3 = forGO;
            }
        }
        lockWheelColor = lockWheel1.GetComponent<Renderer>().material.color;

        foreach(Transform child in plank.First().transform)
        {
            if(child.gameObject.name == "SubTitles")
            {
                plankSubtitle = child.gameObject;
            }
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

		foreach (Transform child in carillon.First().transform) {
			if (child.gameObject.GetComponent<Canvas> ()) {
				carillonImage = child.gameObject;
				break;
			}
        }

        foreach(Transform child in lockR2.First().transform)
        {
            if(child.gameObject.name == "LeftRight")
            {
                lockLR = child.gameObject;
                foreach(Transform c in child)
                {
                    if(c.gameObject.name == "Left")
                    {
                        c.gameObject.GetComponent<Button>().onClick.AddListener(LockR2Left);
                    }
                    else if (c.gameObject.name == "Right")
                    {
                        c.gameObject.GetComponent<Button>().onClick.AddListener(LockR2Right);
                    }
                }
            }
            else if (child.gameObject.name == "UpDown")
            {
                lockUD = child.gameObject;
                foreach (Transform c in child)
                {
                    if (c.gameObject.name == "Up")
                    {
                        c.gameObject.GetComponent<Button>().onClick.AddListener(LockR2Up);
                    }
                    else if (c.gameObject.name == "Down")
                    {
                        c.gameObject.GetComponent<Button>().onClick.AddListener(LockR2Down);
                    }
                }
            }
        }

        foreach (Transform child in board.transform)
        {
            if (child.gameObject.name == "Eraser")
            {
                eraser = child.gameObject;
            }
        }

        nb = boardRemovableWords.Count;
        for (int i = 0; i < nb; i++)
        {
            boardRemovableWords.getAt(i).GetComponent<Renderer>().material.renderQueue = 2001;
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
        if (askCloseWindow)
        {
            CloseWindow();
            askCloseWindow = false;
        }
        
        if (onBag)
        {
            if((usingGlassesTmp1 != CollectableGO.usingGlasses1) || (usingGlassesTmp2 != CollectableGO.usingGlasses2))
            {
                if(CollectableGO.usingGlasses1 && CollectableGO.usingGlasses2)
                {
                    bag.First().GetComponentInChildren<Image>().sprite = bag.First().GetComponentInChildren<BagImage>().image4;
                }
                else if (CollectableGO.usingGlasses1)
                {
                    bag.First().GetComponentInChildren<Image>().sprite = bag.First().GetComponentInChildren<BagImage>().image3;
                }
                else if (CollectableGO.usingGlasses2)
                {
                    bag.First().GetComponentInChildren<Image>().sprite = bag.First().GetComponentInChildren<BagImage>().image2;
                }
                else
                {
                    bag.First().GetComponentInChildren<Image>().sprite = bag.First().GetComponentInChildren<BagImage>().image1;
                }
            }
        }
        usingGlassesTmp1 = CollectableGO.usingGlasses1;
        usingGlassesTmp2 = CollectableGO.usingGlasses2;

        if (Time.time - plankSubtitlesTimer < 2 && !plankSubtitle.activeSelf)
        {
            plankSubtitle.SetActive(true);
        }
        else if(Time.time - plankSubtitlesTimer > 2 && plankSubtitle.activeSelf)
        {
            plankSubtitle.SetActive(false);
        }

        if (moveToPlank)
        {
            //animation to move the player in front of the plank
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, plankPos, speed);
            camNewDir = plank.First().transform.up;
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
                moveToPlank = false;
            }
        }
        else if (moveBox)
        {
            //box animations//
            if (openBox)
            {
                //animation to open the box
                boxTop.First().transform.localPosition = Vector3.MoveTowards(boxTop.First().transform.localPosition, boxTopPos, speed);
                //when the animation if finished
                if(boxTop.First().transform.localPosition == boxTopPos)
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
				int nbBalls = balls.Count;
				for(int i = 0; i < nbBalls; i++)
                {
					forGO = balls.getAt (i);
					b = forGO.GetComponent<Ball>();
					//forGO.GetComponent<Rigidbody>().isKinematic = true;
                    if (b.id < tmpCount) //the animation doesn't take out all balls at the same time
                    {
                        if (b.outOfBox) //if the ball is out of the box
                        {
                            //move the ball to the position corresponding to its id
                            ballPos = Vector3.up * ((float)balls.Count/10 - (float)(b.id/5)/3) + Vector3.right * ((float)(b.id%5) * -2f / 4+1f)*2/3+Vector3.forward* 0.3f;
							forGO.transform.localPosition = Vector3.MoveTowards(forGO.transform.localPosition, ballPos, speed);
                            //when the last ball arrives to its position
							if (forGO.transform.localPosition == ballPos && b.id == balls.Count - 1)
                            {
                                //stop animations
                                takingBalls = false;
                                moveBox = false;
                                ballsout = true;
                            }
                        }
                        else //if the ball is still in the box
                        {
                            //animation to move the ball to box's aperture
							forGO.transform.localPosition = Vector3.MoveTowards(forGO.transform.localPosition, Vector3.up, speed);
							forGO.transform.localRotation = Quaternion.Euler(Vector3.up*-90);
                            //when the ball arrives
							if(forGO.transform.localPosition == Vector3.up)
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
                //move up and rotate the padlock
                dist = 1.5f - boxPadlock.transform.localPosition.y;
                boxPadlock.transform.localPosition = Vector3.MoveTowards(boxPadlock.transform.localPosition, boxPadlock.transform.localPosition + Vector3.up * dist, (dist + 1)/100);
                boxPadlock.transform.localRotation = Quaternion.Euler(boxPadlock.transform.localRotation.eulerAngles+Vector3.up* (boxPadlock.transform.localPosition.y - 0.2f) *35);
                if(boxPadlock.transform.localPosition.y > 1.4f)
                {
                    //stop animation when the padlock reaches a certain height
                    boxPadlock.SetActive(false);
                    unlockBox = false;
                    CollectableGO.usingKeyE03 = false;
                    //hide inventory's displayed elements
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Display" || child.gameObject.name == "Selected")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    //remove key from inventory
					int nbCGO = cGO.Count;
					for(int i = 0; i < nbCGO; i++)
                    {
						forGO = cGO.getAt (i);
						if(forGO.name == "KeyE03")
                        {
							forGO.SetActive(false);
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
                        objectPos += Vector3.up*(1.78f - 0.5f -objectPos.y);
                        //start animation to unlock box if using key on box
                        if (CollectableGO.usingKeyE03)
                        {
                            unlockBox = true;
                        }
                    }
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
                focusedBall.transform.position = Vector3.MoveTowards(focusedBall.transform.position, ballToCamera, speed);
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
                ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball>().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball>().id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
                focusedBall.transform.localPosition = Vector3.MoveTowards(focusedBall.transform.localPosition, ballPos, speed);
                focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(0, -90, 0), speedRotation * 2);
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
            objectPos = new Vector3(player.First().transform.position.x, Camera.main.transform.position.y, player.First().transform.position.z) + vec * (d);
            selectedTablet.transform.position = Vector3.MoveTowards(selectedTablet.transform.position, objectPos, speed);
            newDir = Vector3.RotateTowards(selectedTablet.transform.forward, player.First().transform.forward, Mathf.Deg2Rad * speedRotation, 0);
            selectedTablet.transform.rotation = Quaternion.LookRotation(newDir);
            Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation, Quaternion.Euler(Vector3.zero), speedRotation2);
            //when the tablet arrives
            if (selectedTablet.transform.position == objectPos)
            {
                moveTablet = false;
                //put the ui "screen" on the screen (rather than on the tablet)
                tabletScreen.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;
                Camera.main.GetComponent<PostProcessingBehaviour>().profile.vignette.enabled = false;
                Camera.main.GetComponent<PostProcessingBehaviour>().profile.chromaticAberration.enabled = false;
                inventory.First().SetActive(false);
            }
        }
        else if (moveToTable)
        {
            player.First().GetComponent<Rigidbody>().detectCollisions = false; //disable player collision to avoid collision with tables during animation
            if (onTable)
            {
                //animation to move the player above the table
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, onTablePoint, speed);
                //Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation, Quaternion.Euler(90,0,0), speedRotation);
                //player.First().transform.rotation = Quaternion.RotateTowards(player.First().transform.rotation, Quaternion.Euler(0, focusedTable.transform.rotation.eulerAngles.y, 0), speedRotation2);
                //when the player reaches the position
                if (player.First().transform.position == onTablePoint)
                {
                    //show ui
					tableUI.SetActive (true);
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
                //move up and rotate padlock
                dist = objectPos.y - boxPadlock.transform.position.y;
                bagPadlock.transform.position = Vector3.MoveTowards(bagPadlock.transform.position, bagPadlock.transform.position + Vector3.up * dist, (dist + 1) / 500);
                bagPadlock.transform.localRotation = Quaternion.Euler(bagPadlock.transform.localRotation.eulerAngles + Vector3.up * (bagPadlock.transform.localPosition.y - 0.3f) * 500);
                if (bagPadlock.transform.position.y > objectPos.y - 0.05f)
                {
                    Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                    v.Normalize();
                    objectPos = new Vector3(player.First().transform.position.x, 1.78f - 0.5f, player.First().transform.position.z) + v * 1.5f;
                    //stop animation when the padlock reaches a certain height
                    bagPadlock.SetActive(false);
                    unlockBag = false;
                    CollectableGO.usingKeyE08 = false;
                    //hide inventory's displayed elements
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Display" || child.gameObject.name == "Selected")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    //remove key from inventory
					int nbCGO = cGO.Count;
					for(int i = 0; i < nbCGO; i++)
                    {
						forGO = cGO.getAt (i);
						if (forGO.name == "KeyE08")
                        {
							forGO.SetActive(false);
                        }
                    }
                }
            }
            else if (showBagPaper)
            {
                if(onBag)
                {
                    //take off the paper from the bag
                    bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.position = Vector3.MoveTowards(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.position, bag.First().transform.TransformPoint(bagPaperInitialPos) + Vector3.up*0.9f, speed / 10);
                    if(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.position.y == bag.First().transform.TransformPoint(bagPaperInitialPos).y + 0.9f)
                    {
                        //show the paper on the camera screen when it reaches the final position
                        showBagPaper = false;
                        moveBag = false;
                        bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition += Vector3.up * (bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.InverseTransformPoint(Camera.main.transform.position).y - bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition.y);
                        bag.First().GetComponentInChildren<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
						onBagPaper = true;
                    }
                }
                else
				{
                    //put the paper back in the bag
                    bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.position = Vector3.MoveTowards(bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.position, bag.First().transform.TransformPoint(bagPaperInitialPos), speed / 5);
                    if (bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.position.y == bag.First().transform.TransformPoint(bagPaperInitialPos).y)
                    {
                        //when final position is reached, stop animation and give back control to the player
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
                //when the bag arrives
                if (bag.First().transform.position == objectPos && Vector3.Angle(bag.First().transform.forward, player.First().transform.forward)<1)
                {
                    if (!bagPadlock.activeSelf)
                    {
                        //if unlocked, show the paper
                        showBagPaper = true;
                    }
                    else
                    {
                        objectPos += Vector3.up * (1.78f + 0.4f - objectPos.y);
                        Debug.Log(objectPos);
                        if (CollectableGO.usingKeyE08)
                        {
                            //unlock if key used
                            unlockBag = true;
                            objectPos = bagPadlock.transform.position + Vector3.up * 0.3f;
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
                lockLR.SetActive(true);
                lockUD.SetActive(true);
                selectedWheel = lockWheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
                moveToLockR2 = false;
            }
        }
        else if (moveToBoard)
        {
            //animation to move the player in front of the board
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, boardPos, speed);
            camNewDir = -board.transform.up + Vector3.up * 0.05f;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            //when the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && player.First().transform.position == boardPos)
            {
                //correct the rotation
                newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.right, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                moveToBoard = false;
                distToBoard = Mathf.Abs(Vector3.Dot((Camera.main.transform.position - eraser.transform.position), board.transform.up));
            }
        }

        if (lockRotationUp)
        {
            selectedWheel.transform.Rotate(1,0,0);
            wheelRotationCount++;
            if (wheelRotationCount == 36)
            {
                lockRotationUp = false;
                wheelRotationCount = 0;
                if (lockNumber1 == 7 && lockNumber2 == 0 && lockNumber3 == 3 && !IARTab.room3Unlocked)
                {
                    lockR2.First().GetComponent<Selectable>().solved = true;
                    IARTab.room3Unlocked = true;
                }
            }
        }
        else if (lockRotationDown)
        {
            selectedWheel.transform.Rotate(-1, 0, 0);
            wheelRotationCount++;
            if (wheelRotationCount == 36)
            {
                lockRotationDown = false;
                wheelRotationCount = 0;
                if (lockNumber1 == 7 && lockNumber2 == 0 && lockNumber3 == 3 && !IARTab.room3Unlocked)
                {
                    lockR2.First().GetComponent<Selectable>().solved = true;
                    IARTab.room3Unlocked = true;
                }
            }
        }

        if (noSelection)
        {
			int nbObjects = objects.Count;
			for(int i = 0; i < nbObjects; i++)
            {
				forGO = objects.getAt (i);
				if (forGO.GetComponent<Selectable>().isSelected)   //if a gameobject is selected
                {
                    noSelection = false;
                    //set and show ui
					if (forGO.tag == "Plank") {  //set plank
						//the position in front of the plank is not the same depending on the scale of the player
						if (player.First ().transform.localScale.x < 0.9f) {
							plankPos = new Vector3 (plank.First ().transform.position.x, 1.6f, plank.First ().transform.position.z) - plank.First().transform.up * 1.7f;
						} else {
							plankPos = new Vector3 (plank.First ().transform.position.x, 0.98f, plank.First ().transform.position.z) - plank.First().transform.up * 1.7f;
						}
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (plankPos - player.First ().transform.position).magnitude;
						foreach (Transform t in player.First().transform) {
							if (t.gameObject.tag == "MainCamera") {
								speedRotation = Vector3.Angle (t.gameObject.transform.forward, plank.First().transform.up) * speed / dist;
							}
						}
						moveToPlank = true; //start animation to move the player in front of the plank
						onPlank = true;
					} else if (forGO.tag == "Box") {    //set box
						forGO.GetComponent<Rigidbody> ().isKinematic = true;
						//calculate the position in front of the player
						Vector3 v = new Vector3 (Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
						v.Normalize ();
						objectPos = new Vector3 (player.First ().transform.position.x, 1.78f - 1, player.First ().transform.position.z) + v * (forGO.transform.localScale.y + 1f);
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (objectPos - forGO.transform.position).magnitude;
						speedRotation = Vector3.Angle (box.First ().transform.forward, -player.First ().transform.forward) * speed / dist;
						speedRotation2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
						moveBox = true; //start animation to move the box in front of the player
						onBox = true;
					} else if (forGO.tag == "Tablet") { //set tablet
						selectedTablet = forGO;
						tabletScreen = selectedTablet.GetComponentInChildren<Canvas> ().gameObject;
						selectedTablet.GetComponent<Rigidbody> ().isKinematic = true;
						//calculate the distance between the tablet and the player depending on the screen size
						//(not really necessary since the tablet screen is displayed on the screen and not in the world)
						float d = 0.33f;
						if ((float)Camera.main.pixelWidth / 900 < (float)Camera.main.pixelHeight / 600) {
							d = aTabletDist * (float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth + bTabletDist;
						} else {
							d = 0.56f;
						}
						//calculate the position in front of the player
						Vector3 v = new Vector3 (Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
						v.Normalize ();
						objectPos = new Vector3 (player.First ().transform.position.x, Camera.main.transform.position.y, player.First ().transform.position.z) + v * (d);
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (objectPos - forGO.transform.position).magnitude;
						speedRotation = Vector3.Angle (selectedTablet.transform.forward, player.First ().transform.forward) * speed / dist;
						speedRotation2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
						onTablet = true;
						moveTablet = true;  //start animation to move the tablet in front of the player
					} else if (forGO.tag == "TableE05") {  //set tables of enigma 5
						/* set the position where the player will be moved depending on the selected table
                            * the position is chosen so that when the tables are assembled, the formed picture is in the center of the screen
                            */
						/*if (forGO.name.Contains (1.ToString ())) {
							//front left of the table
							onTablePoint = forGO.transform.position - forGO.transform.right * 0.75f + forGO.transform.forward * 0.75f + Vector3.up * (forGO.transform.localScale.y * 0.55f + 1);
							dist = (onTablePoint - player.First ().transform.position).magnitude;
						} else if (forGO.name.Contains (2.ToString ())) {
							//front right of the table
							onTablePoint = forGO.transform.position + forGO.transform.right * 0.75f + forGO.transform.forward * 0.75f + Vector3.up * (forGO.transform.localScale.y * 0.55f + 1);
							dist = (onTablePoint - player.First ().transform.position).magnitude;
						} else if (forGO.name.Contains (3.ToString ())) {
							//back of the table (back according to unity axes)
							onTablePoint = forGO.transform.position - forGO.transform.forward * 0.75f + Vector3.up * (forGO.transform.localScale.y * 0.55f + 1);
							dist = (onTablePoint - player.First ().transform.position).magnitude;
                        }*/
                        onTablePoint = player.First().transform.position;
                        dist = 1;
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        speedRotation = Vector3.Angle (Camera.main.transform.forward, Vector3.down) * speed / dist;
						speedRotation2 = Vector3.Angle (player.First ().transform.forward, forGO.transform.forward) * speed / dist;
						//save rotation and position of the player before moving
						posBeforeTable = player.First ().transform.position;
						rotBeforeTable = Camera.main.transform.localRotation;
						rot2BeforeTable = player.First ().transform.rotation;
						onTable = true;
						moveToTable = true; //start animation to move the playre above the table
					} else if (forGO.tag == "Sheet") {  //set sheets of enigma 6 and 7
						focusedSheet = forGO;  //store the selected sheet
						onSheet = true;
                        Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;
                        Camera.main.transform.localRotation = Quaternion.Euler (0, 0, 0); //set the x rotation of the player to 0 (make an animation rather than this)
						rotBeforeSheet = focusedSheet.transform.rotation;   //the the rotation of the sheet before selection
						//rotate the sheet toward the player before displaying it (else it would be distorted in the ui)
						focusedSheet.transform.rotation = Quaternion.RotateTowards (focusedSheet.transform.rotation, Quaternion.Euler (0, player.First ().transform.rotation.eulerAngles.y - 90, 0), 360);
						foreach (Transform canvas in focusedSheet.transform) {
							if (canvas.gameObject.name == "Display") {
								//show the sheet in an ui
								canvas.GetComponent<Canvas> ().planeDistance = 0.33f;
								canvas.gameObject.SetActive (true);
							}
						}
					} else if (forGO.tag == "Bag") {
						forGO.GetComponent<Rigidbody> ().isKinematic = true;
						//calculate the position in front of the player
						Vector3 v = new Vector3 (Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
						v.Normalize ();
						objectPos = new Vector3 (player.First ().transform.position.x, 1.78f - 0.5f, player.First ().transform.position.z) + v * 1.5f;
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (objectPos - forGO.transform.position).magnitude;
						speedRotation = Vector3.Angle (bag.First ().transform.forward, player.First ().transform.forward) * speed / dist;
						speedRotation2 = Camera.main.transform.localRotation.eulerAngles.magnitude * speed / dist;
						moveBag = true; //start animation to move the bag in front of the player
						onBag = true;
					} else if (forGO.tag == "LockRoom2") {  //set lock room 2
						//the position in front of the lock is not the same depending on the scale of the player
						if (player.First ().transform.localScale.x < 0.9f) {
							lockR2Pos = new Vector3 (lockWheel2.transform.position.x - 2.5f, 2.68f, lockWheel2.transform.position.z);
						} else {
							lockR2Pos = new Vector3 (lockWheel2.transform.position.x - 2.5f, 2f, lockWheel2.transform.position.z);
						}
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (lockR2Pos - player.First ().transform.position).magnitude;
						foreach (Transform t in player.First().transform) {
							if (t.gameObject.tag == "MainCamera") {
								speedRotation = Vector3.Angle (t.gameObject.transform.forward, Vector3.right) * speed / dist;
							}
						}
						moveToLockR2 = true; //start animation to move the player in front of the lock
						onLockR2 = true;
					} else if (forGO.name == "Carillon") {
						carillonImage.SetActive (true);
						onCarillon = true;
                    }
                    else if (forGO.tag == "Board")
                    {  //set board
                       //the position in front of the board is not the same depending on the scale of the player
                        if (player.First().transform.localScale.x < 0.9f)
                        {
                            boardPos = new Vector3(board.transform.position.x, 1.6f, board.transform.position.z) + board.transform.up * 3.5f;
                        }
                        else
                        {
                            boardPos = new Vector3(board.transform.position.x, 0.98f, board.transform.position.z) + board.transform.up * 3.5f;
                        }
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (boardPos - player.First().transform.position).magnitude;
                        speedRotation = Vector3.Angle(Camera.main.transform.forward, -board.transform.up + Vector3.up * 0.05f) * speed / dist;
                        moveToBoard = true; //start animation to move the player in front of the board
                        onBoard = true;
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
		else if(!IARTab.onIAR)    //if "noselection" is false
        {
			if (onPlank) {
				if (closePlank.Count == 0 && Input.GetMouseButtonDown (0)) {
					CloseWindow ();
				} else if (CollectableGO.usingWire) {
					pointerOverWord = false;
					int nbPlankWords = plankWords.Count;
					for (int i = 0; i < nbPlankWords; i++) {
						forGO = plankWords.getAt (i);
						if (forGO.GetComponent<PointerOver> ()) {
							pointerOverWord = true;
							if (Input.GetMouseButtonDown (0)) {    //if a selectable word is clicked
								//if the word is selected (color red)
								if (forGO.GetComponent<TextMeshPro> ().color == Color.red) {
									//unselect it
									forGO.GetComponent<TextMeshPro> ().color = Color.black;
                                    //remove the vertex from the linerenderer
                                    lrPositions.Remove(forGO.transform.position);
                                    lr.positionCount--;
                                    //set the new positions
                                    lr.SetPositions (lrPositions.ToArray ());
								} else {    //if the word wasn't selected
									if (lr.positionCount > 2) {
										//if there is already 3 selected words, unselect them and select the new one
										foreach (GameObject w in plankWords) {
											w.GetComponent<TextMeshPro> ().color = Color.black;
										}
										lr.positionCount = 0;
										lrPositions.Clear ();
									}
									forGO.GetComponent<TextMeshPro> ().color = Color.red;
									//update the linerenderer
									lr.positionCount++;
									lrPositions.Add (forGO.transform.position);
									lr.SetPositions (lrPositions.ToArray ());
									bool correct = true;
									for (int j = 0; j < nbPlankWords; j++) {
										forGO2 = plankWords.getAt (j);
										if ((forGO2.name == "Objectifs" || forGO2.name == "Methodes" || forGO2.name == "Evaluation") && forGO2.GetComponent<TextMeshPro> ().color != Color.red) {
											correct = false;
										}
									}
									if (correct) {
										Selectable.askRight = true;
									}
								}
							} else {    //if mouse over a word without click
								if (forGO.GetComponent<TextMeshPro> ().color != Color.red) {
									//if the word isn't selected change its color to yellow
									forGO.GetComponent<TextMeshPro> ().color = Color.yellow;
								}
							}
						} else {    //if mouse isn't over a word
							if (forGO.GetComponent<TextMeshPro> ().color != Color.red) {
								//if the word isn't selected change its color to black (initial)
								forGO.GetComponent<TextMeshPro> ().color = Color.black;
							}
						}
					}
					if (!pointerOverWord && Input.GetMouseButtonDown (0)) {
						//if click over nothing unselect all
						for (int i = 0; i < nbPlankWords; i++) {
							plankWords.getAt (i).GetComponent<TextMeshPro> ().color = Color.black;
							lr.positionCount = 0;
							lrPositions.Clear ();
						}
					}
				}
                else if(Input.GetMouseButtonDown(0))
                {
                    plankSubtitlesTimer = Time.time;
                }
			} else if (onBox) {
				if (closeBox.Count == 0 && Input.GetMouseButtonDown (0) && !ballFocused) {
					CloseWindow ();
				} else if (ballsout) {   //if all balls are out of the box
					Ball b = null;
					if (ballFocused) {    //if there is a selected ball
						if (Input.GetMouseButtonDown (0) && !moveBall) {
							//onclick if the ball isn't moving, move it back to its position with other balls
							ballFocused = false;
							moveBall = true;
							foreach (Transform child in focusedBall.transform) {
								if (child.gameObject.name == "Text") {
									child.gameObject.SetActive (true);
								}
							}
							//calculate position and speeds for the animation
							ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball> ().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball> ().id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
							dist = (box.First ().transform.TransformPoint (ballPos) - ballToCamera).magnitude;
							speedRotation = 180 * speed / dist / 2;
                        }
					} else if (!moveBall) {  //if there isn't animations and selected ball
						int nbBalls = balls.Count;
                        bool overNothing = true;
						for (int i = 0; i < nbBalls; i++) {
							forGO = balls.getAt (i);
							b = forGO.GetComponent<Ball> ();
							if (forGO.GetComponent<PointerOver> ()) {
                                overNothing = false;
								//if pointer over a ball change its color to yellow
								forGO.GetComponent<Renderer> ().material.color = Color.yellow + Color.white / 4;
                                ballSubTitles.text = b.text;
								if (Input.GetMouseButtonDown (0)) {    //if a ball is clicked
									//move it in front of the camera
									ballFocused = true;
									moveBall = true;
									focusedBall = forGO;
									foreach (Transform child in focusedBall.transform) {
										if (child.gameObject.name == "Number") {
											child.gameObject.SetActive (true);
										}
									}
									forGO.GetComponent<Renderer> ().material.color = b.color; //initial color
                                    ballSubTitles.text = "";
                                    //calculate position and speeds for the animation
                                    ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(focusedBall.GetComponent<Ball> ().id / 5) / 3) + Vector3.right * ((float)(focusedBall.GetComponent<Ball> ().id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
									dist = (box.First ().transform.TransformPoint (ballPos) - ballToCamera).magnitude;
									speedRotation = 180 * speed / dist;
                                    Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;
                                }
							} else {
								//if there isn't animations, mouse over or click, set to initial color
								forGO.GetComponent<Renderer> ().material.color = b.color;
                            }
						}
                        if (overNothing)
                        {
                            ballSubTitles.text = "";
                        }
					}
				}
			} else if (onLockR2) {
                //"close" ui (give back control to the player) when the correct password is entered or when clicking on nothing
				if (lockR2.First ().GetComponent<Selectable> ().solved || (!lockR2.First ().GetComponent<PointerOver> () && overInventoryElem.Count == 0 && Input.GetMouseButtonDown (0))) {
					CloseWindow ();
				}
			} else if ((onSheet || onCarillon || onBag) && Input.GetMouseButtonDown (0) && overInventoryElem.Count == 0) {
                //close ui on click 
				CloseWindow ();
			} else if (onTable) {
				if (draggedE05) {
					if (Input.GetMouseButtonUp(0))
					{
						if(!(draggedE05.GetComponent<RectTransform>().position.x >0 && draggedE05.GetComponent<RectTransform>().position.x < Camera.main.pixelWidth && draggedE05.GetComponent<RectTransform>().position.y > 0 && draggedE05.GetComponent<RectTransform>().position.y < Camera.main.pixelHeight))
						{
							draggedE05.GetComponent<RectTransform>().position = posBeforeDragE05;
						}
						draggedE05 = null;
					}
					else
					{
						if (Input.GetKeyDown(KeyCode.Q))
						{
							draggedE05.GetComponent<RectTransform>().localRotation = Quaternion.Euler(draggedE05.GetComponent<RectTransform>().localRotation.eulerAngles.x, draggedE05.GetComponent<RectTransform>().localRotation.eulerAngles.y, draggedE05.GetComponent<RectTransform>().localRotation.eulerAngles.z + 90);
						}
						if (Input.GetKeyDown(KeyCode.D))
						{
							draggedE05.GetComponent<RectTransform>().localRotation = Quaternion.Euler(draggedE05.GetComponent<RectTransform>().localRotation.eulerAngles.x, draggedE05.GetComponent<RectTransform>().localRotation.eulerAngles.y, draggedE05.GetComponent<RectTransform>().localRotation.eulerAngles.z - 90);
						}
						draggedE05.GetComponent<RectTransform>().position = Input.mousePosition - posFromMouseE05;
					}
				} else {
					bool onPiece = false;
					int nbPieces = e05Pieces.Count;
					for (int i = 0; i < nbPieces; i++) {
						forGO = e05Pieces.getAt (i);
						if (forGO.GetComponent<PointerOver> ()) {
							onPiece = true;
							if (Input.GetMouseButtonDown (0)) {
								draggedE05 = forGO;
								posBeforeDragE05 = draggedE05.GetComponent<RectTransform> ().position;
								posFromMouseE05 = Input.mousePosition - posBeforeDragE05;
							}
							break;
						}
					}
					if (!onPiece && Input.GetMouseButtonDown (0)) {
						CloseWindow ();
					}
				}
			}
            else if (onBoard)
            {
                if (closeBoard.Count == 0 && Input.GetMouseButtonDown(0))
                {
                    //close ui when clicking on nothing
                    CloseWindow();
                }
                else
                {
                    if (eraser.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
                    {
                        //start dragging eraser when it s clicked
                        eraserDragged = true;
                    }
                    if (eraserDragged)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            //stopo dragging eraser when the click is released
                            eraserDragged = false;
                        }
                        else
                        {
                            //move eraser to mouse position
                            Vector3 mousePos = board.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distToBoard)));
                            eraser.transform.localPosition = new Vector3(mousePos.x, eraser.transform.localPosition.y, mousePos.z);
                            //prevent eraser from going out of the board
                            if (eraser.transform.localPosition.x > 0.5f)
                            {
                                eraser.transform.localPosition += Vector3.right * (0.5f - eraser.transform.localPosition.x);
                            }
                            else if (eraser.transform.localPosition.x < -0.5f)
                            {
                                eraser.transform.localPosition += Vector3.right * (-0.5f - eraser.transform.localPosition.x);
                            }
                            if (eraser.transform.localPosition.z > 0.5f)
                            {
                                eraser.transform.localPosition += Vector3.forward * (0.5f - eraser.transform.localPosition.z);
                            }
                            else if (eraser.transform.localPosition.z < -0.5f)
                            {
                                eraser.transform.localPosition += Vector3.forward * (-0.5f - eraser.transform.localPosition.z);
                            }
                            TextureFromCamera.draw = true; //draw on the texture to erase words on board
                        }
                    }
                }
            }
        }
	}

    private void CloseWindow()
    {
        CollectableGO.askCloseInventory = true;
        if (!onTable && !onBag)
        {
            //enable player
            player.First().GetComponent<FirstPersonController>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
		if (onPlank) {
			player.First ().transform.forward = -plank.First ().transform.right;
			Camera.main.transform.localRotation = Quaternion.Euler (Vector3.zero);
		} else if (onBox) {
			//close box, set balls to initial position and everything to not kinematic
			boxTop.First ().transform.localPosition = boxTopIniPos;
			box.First ().GetComponent<Rigidbody> ().isKinematic = false;
			int nbBalls = balls.Count;
			for (int i = 0; i < nbBalls; i++) {
				forGO = balls.getAt (i);
				forGO.transform.localPosition = forGO.GetComponent<Ball> ().initialPosition;
				forGO.GetComponent<Ball> ().outOfBox = false;
				//forGO.GetComponent<Rigidbody> ().isKinematic = false;
			}
			tmpCount = 1;
			moveBox = false;
		} else if (onTablet) {
			selectedTablet.GetComponent<Rigidbody> ().isKinematic = false;
			//put the screen on the tablet
			tabletScreen.GetComponent<Canvas> ().renderMode = RenderMode.WorldSpace;
			tabletScreen.GetComponent<RectTransform> ().localPosition = Vector3.forward * -0.026f;
			tabletScreen.GetComponent<RectTransform> ().sizeDelta = new Vector2 (900, 600);
			tabletScreen.GetComponent<RectTransform> ().localScale = Vector3.one * -0.0008327437f;
			tabletScreen.GetComponent<RectTransform> ().localRotation = Quaternion.Euler (0, 0, 180);
            Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = true;
            Camera.main.GetComponent<PostProcessingBehaviour>().profile.vignette.enabled = true;
            Camera.main.GetComponent<PostProcessingBehaviour>().profile.chromaticAberration.enabled = true;
            int nb = inputFields.Count;
            inventory.First().SetActive(true);
        } else if (onTable) {
			//start animation to move the player back to its position
			moveToTable = true;
			tableUI.SetActive (false);
		} else if (onSheet)
        {
            Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = true;
            //set sheet to initial rotation and hide ui
            focusedSheet.transform.rotation = rotBeforeSheet;
			foreach (Canvas canvas in focusedSheet.GetComponentsInChildren<Canvas>()) {
				if (canvas.gameObject.name == "Display") {
					canvas.gameObject.SetActive (false);
				}
			}
		} else if (onBag) {
			bag.First ().GetComponent<Rigidbody> ().isKinematic = false;
			moveBag = false;
			if (bag.First ().GetComponentInChildren<Canvas> ().gameObject.transform.parent.localPosition.y != bagPaperInitialPos.y) {
				showBagPaper = true;
				moveBag = true;
				bag.First ().GetComponent<Rigidbody> ().isKinematic = true;
				bag.First ().GetComponentInChildren<Canvas> ().gameObject.transform.parent.position += Vector3.up * (bag.First().transform.TransformPoint(bagPaperInitialPos).y + 0.9f - bag.First ().GetComponentInChildren<Canvas> ().gameObject.transform.parent.position.y);
				bag.First ().GetComponentInChildren<Canvas> ().renderMode = RenderMode.WorldSpace;
				bag.First ().GetComponentInChildren<Canvas> ().gameObject.GetComponent<RectTransform> ().localPosition = Vector3.forward * (-0.51f - bag.First ().GetComponentInChildren<Canvas> ().gameObject.transform.parent.localPosition.z);
				bag.First ().GetComponentInChildren<Canvas> ().gameObject.GetComponent<RectTransform> ().localScale = Vector3.one * 0.000535786f;
			} else {
				//enable player
				player.First ().GetComponent<FirstPersonController> ().enabled = true;
				Cursor.lockState = CursorLockMode.None;
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		} else if (onCarillon) {
			carillonImage.SetActive (false);
		}
        else if (onLockR2)
        {
            lockLR.SetActive(false);
            lockUD.SetActive(false);
            lockWheel1.GetComponent<Renderer>().material.color = lockWheelColor;
            lockWheel2.GetComponent<Renderer>().material.color = lockWheelColor;
            lockWheel3.GetComponent<Renderer>().material.color = lockWheelColor;
            lockR2.First().GetComponent<Selectable>().solved = false;
        }
        //show cursor
        cursorUI.SetActive(true);
		int nbObjects = objects.Count;
		for(int i = 0; i < nbObjects; i++)
        {
			forGO = objects.getAt (i);
			if (forGO.GetComponent<Selectable>().isSelected)
            {
                //unselect the gameobject
				forGO.GetComponent<Selectable>().isSelected = false;
                Selectable.selected = false;
                noSelection = true;
                break;
            }
        }
        onSheet = false;
        onPlank = false;
        onBox = false;
        onTablet = false;
        onTable = false;
        onBag = false;
        onBagPaper = false;
        onLockR2 = false;
		onCarillon = false;
        onBoard = false;
    }
    
    private void LockR2Up()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            lockRotationUp = true;
            if (Object.ReferenceEquals(selectedWheel, lockWheel1))
            {
                if(lockNumber1 == 9)
                {
                    lockNumber1 = 0;
                }
                else
                {
                    lockNumber1++;
                }
            }
            else if (Object.ReferenceEquals(selectedWheel, lockWheel2))
            {
                if (lockNumber2 == 9)
                {
                    lockNumber2 = 0;
                }
                else
                {
                    lockNumber2++;
                }
            }
            else if (Object.ReferenceEquals(selectedWheel, lockWheel3))
            {
                if (lockNumber3 == 9)
                {
                    lockNumber3 = 0;
                }
                else
                {
                    lockNumber3++;
                }
            }
        }
    }

    private void LockR2Down()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            lockRotationDown = true;
            if (Object.ReferenceEquals(selectedWheel, lockWheel1))
            {
                if (lockNumber1 == 0)
                {
                    lockNumber1 = 9;
                }
                else
                {
                    lockNumber1--;
                }
            }
            else if (Object.ReferenceEquals(selectedWheel, lockWheel2))
            {
                if (lockNumber2 == 0)
                {
                    lockNumber2 = 9;
                }
                else
                {
                    lockNumber2--;
                }
            }
            else if (Object.ReferenceEquals(selectedWheel, lockWheel3))
            {
                if (lockNumber3 == 0)
                {
                    lockNumber3 = 9;
                }
                else
                {
                    lockNumber3--;
                }
            }
        }
    }

    private void LockR2Left()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            if (Object.ReferenceEquals(selectedWheel, lockWheel2))
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = lockWheel1;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
            }
            else if (Object.ReferenceEquals(selectedWheel, lockWheel3))
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = lockWheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
            }
        }
    }

    private void LockR2Right()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            if (Object.ReferenceEquals(selectedWheel, lockWheel1))
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = lockWheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
            }
            else if (Object.ReferenceEquals(selectedWheel, lockWheel2))
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = lockWheel3;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
            }
        }
    }
}