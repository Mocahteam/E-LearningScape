using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using TMPro;
using UnityEngine.PostProcessing;
using FYFY_plugins.Monitoring;

public class ShowUI : FSystem {
    
    //all selectable objects
    private Family objects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)));
    
    private Family inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(InputField)));
    private Family ui = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family plank = FamilyManager.getFamily(new AnyOfTags("Plank"));
    private Family plankWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerSensitive))); //clickable words on the plank
    private Family box = FamilyManager.getFamily(new AnyOfTags("Box"));
    private Family boxTop = FamilyManager.getFamily(new AnyOfTags("BoxTop"));   //box lid
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family inventory = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family cGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family bag = FamilyManager.getFamily(new AnyOfTags("Bag"));
    private Family lockR2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"));
    private Family lockIntro = FamilyManager.getFamily(new AnyOfTags("LockIntro"));
    private Family lockR2Wheels = FamilyManager.getFamily(new AnyOfTags("LockR2Wheel"));
    private Family lockIntroWheels = FamilyManager.getFamily(new AnyOfTags("LockIntroWheel"));
    private Family closeLockR2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2", "LockR2Wheel", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family closeLockIntro = FamilyManager.getFamily(new AnyOfTags("LockIntro", "LockIntroWheel", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family closePlank = FamilyManager.getFamily (new AnyOfTags ("Plank", "PlankText", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family closeBox = FamilyManager.getFamily (new AnyOfTags ("Box", "Ball", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family overInventoryElem = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(PointerOver)));
	private Family e05Pieces = FamilyManager.getFamily(new AnyOfTags("E05UI"));
	private Family carillon = FamilyManager.getFamily(new AnyOfTags("Carillon"));
    private Family boardFamilly = FamilyManager.getFamily(new AnyOfTags("Board"));
    private Family closeBoard = FamilyManager.getFamily(new AnyOfTags("Board", "Eraser", "BoardTexture"), new AllOfComponents(typeof(PointerOver)));
    private Family boardRemovableWords = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));
    private Family collectableGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)));
    private Family hud = FamilyManager.getFamily(new AnyOfTags("HUDInputs"));

    private bool noSelection = true;    //true if all objects are unselected
    private GameObject uiGO;
    private GameObject cursorUI;
    public static bool askCloseWindow = false;

    //information for animations
    private float speed;
    private float speedRotation;
    private float speedRotation2;
    private float oldDT;
    private float dist = -1;
    private Vector3 objectPos = Vector3.zero;
    private int tmpCount = -1;
    private Vector3 camNewDir;
    private Vector3 newDir;

    //lock intro
    private bool onLockIntro = false;
    private bool moveToLockIntro = false;
    private Vector3 lockIntroPos;
    private GameObject lockIntroWheel1;
    private GameObject lockIntroWheel2;
    private GameObject lockIntroWheel3;
    private GameObject lockIntroUD;
    private GameObject lockIntroLR;
    private GameObject selectedWheelIntro;
    private bool lockIntroRotationUp = false;
    private bool lockIntroRotationDown = false;
    private Color lockIntroWheelColor;
    private Vector3 lockIntroNumbers = Vector3.zero;
    private float wheelIntroRotationCount = 0;

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
    private Vector3 lockNumbers = Vector3.zero;
    private float wheelRotationCount = 0;

    //carillon
    private bool onCarillon = false;
	private GameObject carillonImage;

    //board
    private GameObject board;
    private bool onBoard = false;
    private bool moveToBoard = false;
    private Vector3 boardPos;
    private GameObject eraser;
    public static bool eraserDragged = false;
    private float distToBoard;

	private GameObject forGO;
	private GameObject forGO2;

    public ShowUI()
    {
        if (Application.isPlaying)
        {
            //initialise vairables
            ballToCamera = Camera.main.transform.position + Camera.main.transform.forward;
            boxTopIniPos = boxTop.First().transform.localPosition;
            boxTopPos = boxTop.First().transform.localPosition + boxTop.First().transform.right - boxTop.First().transform.up / 2; //set lid position
            lr = plank.First().GetComponent<LineRenderer>();
            lrPositions = new List<Vector3>();
            ballSubTitles = box.First().GetComponentInChildren<Canvas>().gameObject.GetComponentInChildren<TextMeshProUGUI>();
            bagPaperInitialPos = bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition;
            board = boardFamilly.First();
            if (Camera.main.GetComponent<PostProcessingBehaviour>())
            {
                Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = true;
            }

            int nb = ui.Count;
            for (int i = 0; i < nb; i++)
            {
                forGO = ui.getAt(i);
                if (forGO.name == "UI")
                {
                    uiGO = forGO;
                    foreach (Transform child in uiGO.transform)
                    {
                        if (child.gameObject.name == "E05")
                        {
                            tableUI = child.gameObject;
                            break;
                        }
                    }
                }
                else if (forGO.name == "Cursor")
                {
                    GameObjectManager.setGameObjectState(forGO,true);  //display cursor
                    cursorUI = forGO;
                }
                else if (forGO.name == "Timer")
                {
                    GameObjectManager.setGameObjectState(forGO,true);  //display timer
                }
            }
            Ball b = null;
            int j = 0;
            nb = balls.Count;
            for (int i = 0; i < nb; i++)
            {
                forGO = balls.getAt(i);
                b = forGO.GetComponent<Ball>();
                b.initialPosition = forGO.transform.localPosition; //set initial position to current local position
                b.id = j;   //set ball id
                j++;
                forGO.GetComponent<Renderer>().material.color = Random.ColorHSV() + Color.white * 0.6f;  //set color to random color
                b.color = forGO.GetComponent<Renderer>().material.color;
                //init text and number
                foreach (Transform child in forGO.transform)
                {
                    if (child.gameObject.name == "Number")
                    {
                        child.gameObject.GetComponent<TextMeshPro>().text = b.number.ToString();
                    }
                }
            }

            nb = lockR2Wheels.Count;
            for (int i = 0; i < nb; i++)
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

            nb = lockIntroWheels.Count;
            for (int i = 0; i < nb; i++)
            {
                forGO = lockIntroWheels.getAt(i);
                if (forGO.name.Contains(1.ToString()))
                {
                    lockIntroWheel1 = forGO;
                }
                else if (forGO.name.Contains(2.ToString()))
                {
                    lockIntroWheel2 = forGO;
                }
                else if (forGO.name.Contains(3.ToString()))
                {
                    lockIntroWheel3 = forGO;
                }
            }
            lockIntroWheelColor = lockIntroWheel1.GetComponent<Renderer>().material.color;

            foreach (Transform child in plank.First().transform)
            {
                if (child.gameObject.name == "SubTitles")
                {
                    plankSubtitle = child.gameObject;
                }
            }

            foreach (Transform child in box.First().transform)
            {
                if (child.gameObject.name == "Padlock")
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

            foreach (Transform child in carillon.First().transform)
            {
                if (child.gameObject.GetComponent<Canvas>())
                {
                    carillonImage = child.gameObject;
                    break;
                }
            }

            foreach (Transform child in lockR2.First().transform)
            {
                if (child.gameObject.name == "LeftRight")
                {
                    lockLR = child.gameObject;
                    foreach (Transform c in child)
                    {
                        if (c.gameObject.name == "Left")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Left(ref selectedWheel, lockWheel1, lockWheel2, lockWheel3, lockUD, lockRotationUp, lockRotationDown);
                            });
                        }
                        else if (c.gameObject.name == "Right")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Right(ref selectedWheel, lockWheel1, lockWheel2, lockWheel3, lockUD, lockRotationUp, lockRotationDown);
                            });
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
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Up(selectedWheel, ref lockNumbers, lockWheel1, lockWheel2, lockWheel3, ref lockRotationUp, ref lockRotationDown);
                            });
                        }
                        else if (c.gameObject.name == "Down")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Down(selectedWheel, ref lockNumbers, lockWheel1, lockWheel2, lockWheel3, ref lockRotationUp, ref lockRotationDown);
                            });
                        }
                    }
                }
            }

            foreach (Transform child in lockIntro.First().transform)
            {
                if (child.gameObject.name == "LeftRight")
                {
                    lockIntroLR = child.gameObject;
                    foreach (Transform c in child)
                    {
                        if (c.gameObject.name == "Left")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Left(ref selectedWheelIntro, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, lockIntroUD, lockIntroRotationUp, lockIntroRotationDown);
                            });
                        }
                        else if (c.gameObject.name == "Right")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Right(ref selectedWheelIntro, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, lockIntroUD, lockIntroRotationUp, lockIntroRotationDown);
                            });
                        }
                    }
                }
                else if (child.gameObject.name == "UpDown")
                {
                    lockIntroUD = child.gameObject;
                    foreach (Transform c in child)
                    {
                        if (c.gameObject.name == "Up")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Up(selectedWheelIntro, ref lockIntroNumbers, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, ref lockIntroRotationUp, ref lockIntroRotationDown);
                            });
                        }
                        else if (c.gameObject.name == "Down")
                        {
                            c.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                                LockR2Down(selectedWheelIntro, ref lockIntroNumbers, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, ref lockIntroRotationUp, ref lockIntroRotationDown);
                            });
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
        speedRotation2 *= Time.deltaTime / oldDT;
        oldDT = Time.deltaTime;
        wasTakingballs = false;

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
            GameObjectManager.setGameObjectState(plankSubtitle,true);
        }
        else if(Time.time - plankSubtitlesTimer > 2 && plankSubtitle.activeSelf)
        {
            GameObjectManager.setGameObjectState(plankSubtitle,false);
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
							forGO.transform.localPosition = Vector3.MoveTowards(forGO.transform.localPosition, ballPos, speed/2);
                            //when the last ball arrives to its position
							if (forGO.transform.localPosition == ballPos && b.id == balls.Count - 1)
                            {
                                //stop animations
                                takingBalls = false;
                                moveBox = false;
                                ballsout = true;
                            }
                            else if (Input.GetMouseButtonDown(0))
                            {
                                //stop animations
                                takingBalls = false;
                                wasTakingballs = true;
                                moveBox = false;
                                ballsout = true;
                                for(int j = 0; j < nbBalls; j++)
                                {
                                    forGO = balls.getAt(j);
                                    b = forGO.GetComponent<Ball>();
                                    ballPos = Vector3.up * ((float)balls.Count / 10 - (float)(b.id / 5) / 3) + Vector3.right * ((float)(b.id % 5) * -2f / 4 + 1f) * 2 / 3 + Vector3.forward * 0.3f;
                                    forGO.transform.localPosition = ballPos;
                                    forGO.transform.localRotation = Quaternion.Euler(Vector3.up * -90 + Vector3.right * 90);
                                }
                                break;
                            }
                        }
                        else //if the ball is still in the box
                        {
                            //animation to move the ball to box's aperture
							forGO.transform.localPosition = Vector3.MoveTowards(forGO.transform.localPosition, Vector3.up, speed/2);
							forGO.transform.localRotation = Quaternion.Euler(Vector3.up*-90 + Vector3.right * 90);
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
                boxPadlock.transform.localPosition = Vector3.MoveTowards(boxPadlock.transform.localPosition, boxPadlock.transform.localPosition + Vector3.up * dist, (dist + 1) / 2 * Time.deltaTime);
                boxPadlock.transform.localRotation = Quaternion.Euler(boxPadlock.transform.localRotation.eulerAngles+Vector3.up* (boxPadlock.transform.localPosition.y - 0.2f) * 350 * 5 * Time.deltaTime);
                if(boxPadlock.transform.localPosition.y > 1.4f)
                {
                    //stop animation when the padlock reaches a certain height
                    GameObjectManager.setGameObjectState(boxPadlock,false);
                    unlockBox = false;
                    CollectableGO.usingKeyE03 = false;
                    SetAnswer.credits = false;
                    if(boxPadlock.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(boxPadlock.GetComponent<ComponentMonitoring>(), "perform");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                        HelpSystem.traces.Enqueue(trace);
                    }
                    //hide inventory's displayed elements
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Display" || child.gameObject.name == "Selected")
                        {
                            GameObjectManager.setGameObjectState(child.gameObject,false);
                        }
                    }
                    //remove key from inventory
					int nbCGO = cGO.Count;
					for(int i = 0; i < nbCGO; i++)
                    {
						forGO = cGO.getAt (i);
						if(forGO.name == "KeyE03")
                        {
                            GameObjectManager.setGameObjectState(forGO,false);
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
                            if (inventory.First().activeSelf)
                            {
                                CollectableGO.askCloseInventory = true;
                            }
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
                focusedBall.transform.localRotation = Quaternion.RotateTowards(focusedBall.transform.localRotation, Quaternion.Euler(90, 90, 0), speedRotation);
                //when the ball arrives
                if (focusedBall.transform.position == ballToCamera)
                {
                    if (focusedBall.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(focusedBall.GetComponent<ComponentMonitoring>(),"activate");
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
                        if(ball1Seen && ball2Seen && ball8Seen)
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
                            GameObjectManager.setGameObjectState(child.gameObject,false);
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
                GameObjectManager.setGameObjectState(inventory.First(),false);
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
                    GameObjectManager.setGameObjectState(tableUI,true);
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
                bagPadlock.transform.position = Vector3.MoveTowards(bagPadlock.transform.position, bagPadlock.transform.position + Vector3.up * dist, (dist + 1) / 5 * Time.deltaTime);
                bagPadlock.transform.localRotation = Quaternion.Euler(bagPadlock.transform.localRotation.eulerAngles + Vector3.up * (bagPadlock.transform.localPosition.y - 0.3f) * 50000 * Time.deltaTime);
                if (bagPadlock.transform.position.y > objectPos.y - 0.05f)
                {
                    Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                    v.Normalize();
                    objectPos = new Vector3(player.First().transform.position.x, 1.78f - 0.5f, player.First().transform.position.z) + v * 1.5f;
                    //stop animation when the padlock reaches a certain height
                    GameObjectManager.setGameObjectState(bagPadlock,false);
                    unlockBag = false;
                    CollectableGO.usingKeyE08 = false;
                    SetAnswer.credits = false;
                    //hide inventory's displayed elements
                    foreach (Transform child in inventory.First().transform)
                    {
                        if (child.gameObject.name == "Display" || child.gameObject.name == "Selected")
                        {
                            GameObjectManager.setGameObjectState(child.gameObject,false);
                        }
                    }
                    //remove key from inventory
					int nbCGO = cGO.Count;
					for(int i = 0; i < nbCGO; i++)
                    {
						forGO = cGO.getAt (i);
						if (forGO.name == "KeyE08")
                        {
                            GameObjectManager.setGameObjectState(forGO,false);
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
                        Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = false;
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
                        if (CollectableGO.usingKeyE08)
                        {
                            //unlock if key used
                            unlockBag = true;
                            objectPos = bagPadlock.transform.position + Vector3.up * 0.3f;
                            if (inventory.First().activeSelf)
                            {
                                CollectableGO.askCloseInventory = true;
                            }
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
                GameObjectManager.setGameObjectState(lockLR,true);
                GameObjectManager.setGameObjectState(lockUD,true);
                selectedWheel = lockWheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
                moveToLockR2 = false;
            }
        }
        else if (moveToLockIntro)
        {
            //animation to move the player in front of the lock in introduction
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, lockIntroPos, speed);
            camNewDir = Vector3.right;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            //when the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && player.First().transform.position == lockIntroPos)
            {
                //correct the rotation
                newDir = Vector3.RotateTowards(player.First().transform.forward, Vector3.right, 360, 0);
                player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                GameObjectManager.setGameObjectState(lockIntroLR,true);
                GameObjectManager.setGameObjectState(lockIntroUD,true);
                selectedWheelIntro = lockIntroWheel2;
                selectedWheelIntro.GetComponent<Renderer>().material.color = selectedWheelIntro.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockIntroUD.transform.localPosition += Vector3.right * (selectedWheelIntro.transform.localPosition.x - lockIntroUD.transform.localPosition.x);
                moveToLockIntro = false;
            }
        }
        else if (moveToBoard)
        {
            //animation to move the player in front of the board
            player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, boardPos, speed);
            camNewDir = board.transform.up;
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
            selectedWheel.transform.Rotate(Time.deltaTime * 200,0,0);
            wheelRotationCount += Time.deltaTime * 200;
            if (wheelRotationCount > 36)
            {
                selectedWheel.transform.Rotate(36 - wheelRotationCount, 0, 0);
                lockRotationUp = false;
                wheelRotationCount = 0;
                if (lockNumbers.x == 7 && lockNumbers.y == 0 && lockNumbers.z == 3 && !IARTab.room3Unlocked)
                {
                    lockR2.First().GetComponent<Selectable>().solved = true;
                    IARTab.room3Unlocked = true;
                }
            }
        }
        else if (lockRotationDown)
        {
            selectedWheel.transform.Rotate(-Time.deltaTime * 200, 0, 0);
            wheelRotationCount += Time.deltaTime * 200;
            if (wheelRotationCount > 36)
            {
                selectedWheel.transform.Rotate(-(36 - wheelRotationCount), 0, 0);
                lockRotationDown = false;
                wheelRotationCount = 0;
                if (lockNumbers.x == 7 && lockNumbers.y == 0 && lockNumbers.z == 3 && !IARTab.room3Unlocked)
                {
                    lockR2.First().GetComponent<Selectable>().solved = true;
                    IARTab.room3Unlocked = true;
                }
            }
        }
        if (lockIntroRotationUp)
        {
            selectedWheelIntro.transform.Rotate(Time.deltaTime * 200, 0, 0);
            wheelIntroRotationCount += Time.deltaTime * 200;
            if (wheelIntroRotationCount > 36)
            {
                selectedWheelIntro.transform.Rotate(36 - wheelIntroRotationCount, 0, 0);
                lockIntroRotationUp = false;
                wheelIntroRotationCount = 0;
                if (lockIntroNumbers.x == 2 && lockIntroNumbers.y == 6 && lockIntroNumbers.z == 7 && !IARTab.room1Unlocked)
                {
                    lockIntro.First().GetComponent<Selectable>().solved = true;
                    IARTab.room1Unlocked = true;
                    int nb = collectableGO.Count;
                    for(int i = 0; i < nb; i++)
                    {
                        forGO = collectableGO.getAt(i);
                        if (forGO.name.Contains("Intro"))
                        {
                            GameObjectManager.setGameObjectState(forGO,false);
                        }
                    }
                    foreach (Transform child in hud.First().transform)
                    {
                        if (child.gameObject.name == "Inventory")
                        {
                            GameObjectManager.setGameObjectState(child.gameObject,true);
                        }
                    }
                    if (HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(4),"perform");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                        HelpSystem.traces.Enqueue(trace);
                        trace = new MonitoringTrace(MonitoringManager.getMonitorById(21),"perform");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                        HelpSystem.traces.Enqueue(trace);
                    }
                }
            }
        }
        else if (lockIntroRotationDown)
        {
            selectedWheelIntro.transform.Rotate(-Time.deltaTime * 200, 0, 0);
            wheelIntroRotationCount += Time.deltaTime * 200;
            if (wheelIntroRotationCount > 36)
            {
                selectedWheelIntro.transform.Rotate(-(36 - wheelIntroRotationCount), 0, 0);
                lockIntroRotationDown = false;
                wheelIntroRotationCount = 0;
                if (lockIntroNumbers.x == 2 && lockIntroNumbers.y == 6 && lockIntroNumbers.z == 7 && !IARTab.room1Unlocked)
                {
                    lockIntro.First().GetComponent<Selectable>().solved = true;
                    IARTab.room1Unlocked = true;
                    int nb = collectableGO.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        forGO = collectableGO.getAt(i);
                        if (forGO.name.Contains("Intro"))
                        {
                            GameObjectManager.setGameObjectState(forGO,false);
                        }
                    }
                    foreach (Transform child in hud.First().transform)
                    {
                        if (child.gameObject.name == "Inventory")
                        {
                            GameObjectManager.setGameObjectState(child.gameObject,true);
                        }
                    }
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
                        plankPos = new Vector3(plank.First().transform.position.x, 1.1f, plank.First().transform.position.z) - plank.First().transform.up * 1.7f;
                        /*//the position in front of the plank is not the same depending on the scale of the player
                        if (player.First ().transform.localScale.x < 0.9f) {
							plankPos = new Vector3 (plank.First ().transform.position.x, 1.6f, plank.First ().transform.position.z) - plank.First().transform.up * 1.7f;
						} else {
							plankPos = new Vector3 (plank.First ().transform.position.x, 0.98f, plank.First ().transform.position.z) - plank.First().transform.up * 1.7f;
						}*/
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (plankPos - player.First ().transform.position).magnitude;
						foreach (Transform t in player.First().transform) {
							if (t.gameObject.tag == "MainCamera") {
								speedRotation = Vector3.Angle (t.gameObject.transform.forward, plank.First().transform.up) * speed / dist;
							}
						}
						moveToPlank = true; //start animation to move the player in front of the plank
						onPlank = true;
                        if (HelpSystem.monitoring)
                        {
                            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(53), "turnOn");
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                            HelpSystem.traces.Enqueue(trace);
                        }
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
                        if (HelpSystem.monitoring)
                        {
                            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(28), "turnOn");
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                            HelpSystem.traces.Enqueue(trace);
                        }
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
                                GameObjectManager.setGameObjectState(canvas.gameObject,true);
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
                        if (CollectableGO.usingGlasses1 && CollectableGO.usingGlasses2)
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
                    } else if (forGO.tag == "LockRoom2") {  //set lock room 2
                        lockR2Pos = new Vector3(lockWheel2.transform.position.x - 2.5f, 1.6f, lockWheel2.transform.position.z);
                        /*//the position in front of the lock is not the same depending on the scale of the player
                        if (player.First ().transform.localScale.x < 0.9f) {
							lockR2Pos = new Vector3 (lockWheel2.transform.position.x - 2.5f, 2.68f, lockWheel2.transform.position.z);
						} else {
							lockR2Pos = new Vector3 (lockWheel2.transform.position.x - 2.5f, 2f, lockWheel2.transform.position.z);
						}*/
						//calculate the correct speed so that the translation and the rotation finish at the same time
						dist = (lockR2Pos - player.First ().transform.position).magnitude;
						foreach (Transform t in player.First().transform) {
							if (t.gameObject.tag == "MainCamera") {
								speedRotation = Vector3.Angle (t.gameObject.transform.forward, Vector3.right) * speed / dist;
							}
						}
						moveToLockR2 = true; //start animation to move the player in front of the lock
						onLockR2 = true;
					}
                    else if (forGO.tag == "LockIntro")
                    {  //set lock intro
                        lockIntroPos = new Vector3(lockIntroWheel2.transform.position.x - 2.5f, -0.7f, lockIntroWheel2.transform.position.z);
                        /*//the position in front of the lock is not the same depending on the scale of the player
                        if (player.First().transform.localScale.x < 0.9f)
                        {
                            lockIntroPos = new Vector3(lockIntroWheel2.transform.position.x - 2.5f, 0.37f, lockIntroWheel2.transform.position.z);
                        }
                        else
                        {
                            lockIntroPos = new Vector3(lockIntroWheel2.transform.position.x - 2.5f, -0.31f, lockIntroWheel2.transform.position.z);
                        }*/
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (lockIntroPos - player.First().transform.position).magnitude;
                        foreach (Transform t in player.First().transform)
                        {
                            if (t.gameObject.tag == "MainCamera")
                            {
                                speedRotation = Vector3.Angle(t.gameObject.transform.forward, Vector3.right) * speed / dist;
                            }
                        }
                        moveToLockIntro = true; //start animation to move the player in front of the lock
                        onLockIntro = true;
                        if(HelpSystem.monitoring)
                        {
                            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(3), "turnOn");
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                            HelpSystem.traces.Enqueue(trace);
                        }
                    }
                    else if (forGO.name == "Carillon") {
                        GameObjectManager.setGameObjectState(carillonImage,true);
						onCarillon = true;
                    }
                    else if (forGO.tag == "Board")
                    {  //set board
                       //the position in front of the board is not the same depending on the scale of the player
                        if (player.First().transform.localScale.x < 0.9f)
                        {
                            boardPos = new Vector3(board.transform.position.x, 2f, board.transform.position.z) - board.transform.up * 3.5f;
                        }
                        else
                        {
                            boardPos = new Vector3(board.transform.position.x, 1.38f, board.transform.position.z) - board.transform.up * 3.5f;
                        }
                        //calculate the correct speed so that the translation and the rotation finish at the same time
                        dist = (boardPos - player.First().transform.position).magnitude;
                        speedRotation = Vector3.Angle(Camera.main.transform.forward, board.transform.up) * speed / dist;
                        moveToBoard = true; //start animation to move the player in front of the board
                        onBoard = true;
                    }
                    //hide the cursor when an object is selected
                    GameObjectManager.setGameObjectState(cursorUI,false);
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
				if (((closePlank.Count == 0 && Input.GetMouseButtonDown (0)) || Input.GetKeyDown(KeyCode.Escape)) && !moveToPlank) {
					CloseWindow ();
				} else{
					pointerOverWord = false;
					int nbPlankWords = plankWords.Count;
					for (int i = 0; i < nbPlankWords; i++) {
						forGO = plankWords.getAt (i);
						if (forGO.GetComponent<PointerOver> ()) {
							pointerOverWord = true;
							if (Input.GetMouseButtonDown (0) && CollectableGO.usingWire) {    //if a selectable word is clicked
                                Inventory.wireOn = false;
								//if the word is selected (color red)
								if (forGO.GetComponent<TextMeshPro> ().color == Color.red) {
									//unselect it
									forGO.GetComponent<TextMeshPro> ().color = Color.black;
                                    //remove the vertex from the linerenderer
                                    lrPositions.Remove(forGO.transform.TransformPoint(Vector3.up * -4));
                                    lr.positionCount--;
                                    //set the new positions
                                    lr.SetPositions (lrPositions.ToArray ());
                                    if (HelpSystem.monitoring && forGO.GetComponent<ComponentMonitoring>())
                                    {
                                        MonitoringTrace trace = new MonitoringTrace(forGO.GetComponent<ComponentMonitoring>(), "turnOff");
                                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                        HelpSystem.traces.Enqueue(trace);
                                    }
                                } else {    //if the word wasn't selected
									if (lr.positionCount > 2) {
                                        //if there is already 3 selected words, unselect them and select the new one
                                        foreach (GameObject w in plankWords)
                                        {
                                            if (w.GetComponent<TextMeshPro>().color == Color.red)
                                            {
                                                if (HelpSystem.monitoring && w.GetComponent<ComponentMonitoring>())
                                                {
                                                    MonitoringTrace trace = new MonitoringTrace(w.GetComponent<ComponentMonitoring>(), "turnOff");
                                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                                    HelpSystem.traces.Enqueue(trace);
                                                }
                                            }
                                            w.GetComponent<TextMeshPro>().color = Color.black;
                                        }
										lr.positionCount = 0;
										lrPositions.Clear ();
									}
									forGO.GetComponent<TextMeshPro> ().color = Color.red;
									//update the linerenderer
									lr.positionCount++;
									lrPositions.Add (forGO.transform.TransformPoint(Vector3.up*-4));
									lr.SetPositions (lrPositions.ToArray ());
									bool correct = true;
									for (int j = 0; j < nbPlankWords; j++) {
										forGO2 = plankWords.getAt (j);
										if ((forGO2.name == "Objectifs" || forGO2.name == "Methodes" || forGO2.name == "Evaluation") && forGO2.GetComponent<TextMeshPro> ().color != Color.red) {
											correct = false;
										}
                                    }
                                    if (HelpSystem.monitoring && forGO.GetComponent<ComponentMonitoring>())
                                    {
                                        MonitoringTrace trace = new MonitoringTrace(forGO.GetComponent<ComponentMonitoring>(), "turnOn");
                                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                        HelpSystem.traces.Enqueue(trace);
                                    }
                                    if (correct) {
										Selectable.askRight = true;
                                        CollectableGO.usingWire = false;
                                        if (HelpSystem.monitoring)
                                        {
                                            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(23), "perform");
                                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                            HelpSystem.traces.Enqueue(trace);
                                            trace = new MonitoringTrace(MonitoringManager.getMonitorById(54), "perform");
                                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                            HelpSystem.traces.Enqueue(trace);
                                        }
                                    }
                                }
							} else {    //if mouse over a word without click
								if (forGO.GetComponent<TextMeshPro> ().color != Color.red) {
									//if the word isn't selected change its color to yellow
									forGO.GetComponent<TextMeshPro> ().color = Color.yellow;
                                    if (Input.GetMouseButtonDown(0))
                                    {
                                        if (HelpSystem.monitoring && forGO.GetComponent<ComponentMonitoring>())
                                        {
                                            MonitoringTrace trace = new MonitoringTrace(forGO.GetComponent<ComponentMonitoring>(), "turnOn");
                                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                            HelpSystem.traces.Enqueue(trace);
                                        }
                                    }
								}
							}
						}
                        else {    //if mouse isn't over a word
							if (forGO.GetComponent<TextMeshPro> ().color != Color.red) {
								//if the word isn't selected change its color to black (initial)
								forGO.GetComponent<TextMeshPro> ().color = Color.black;
							}
						}
					}
					if (!pointerOverWord && Input.GetMouseButtonDown (0) && CollectableGO.usingWire)
                    {
                        Inventory.wireOn = true;
                        //if click over nothing unselect all
                        for (int i = 0; i < nbPlankWords; i++)
                        {
                            if (plankWords.getAt(i).GetComponent<TextMeshPro>().color == Color.red)
                            {
                                if (HelpSystem.monitoring && plankWords.getAt(i).GetComponent<ComponentMonitoring>())
                                {
                                    MonitoringTrace trace = new MonitoringTrace(plankWords.getAt(i).GetComponent<ComponentMonitoring>(), "turnOff");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                    HelpSystem.traces.Enqueue(trace);
                                }
                            }
                            plankWords.getAt (i).GetComponent<TextMeshPro> ().color = Color.black;
							lr.positionCount = 0;
							lrPositions.Clear ();
                        }
					}
				}
			} else if (onBox && (!moveBox || boxPadlock.activeSelf)) {
				if (((closeBox.Count == 0 && Input.GetMouseButtonDown(0) && !wasTakingballs) || Input.GetKeyDown(KeyCode.Escape)) && !ballFocused) {
					CloseWindow ();
				} else if (ballsout) {   //if all balls are out of the box
					Ball b = null;
					if (ballFocused) {    //if there is a selected ball
						if (Input.GetMouseButtonDown (0) && !moveBall) {
							//onclick if the ball isn't moving, move it back to its position with other balls
							ballFocused = false;
							moveBall = true;
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
                                            GameObjectManager.setGameObjectState(child.gameObject,true);
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
			} else if (onLockR2 && !moveToLockR2) {
                //"close" ui (give back control to the player) when the correct password is entered or when clicking on nothing
				if (lockR2.First ().GetComponent<Selectable> ().solved || ((closeLockR2.Count == 0 && Input.GetMouseButtonDown(0)) || Input.GetKeyDown(KeyCode.Escape))) {
					CloseWindow ();
				}
                else if (Input.GetMouseButtonDown(0))
                {
                    int nb = lockR2Wheels.Count;
                    for(int i = 0; i < nb; i++)
                    {
                        forGO = lockR2Wheels.getAt(i);
                        if (forGO.GetComponent<PointerOver>())
                        {
                            selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                            selectedWheel = forGO;
                            selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                            lockUD.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - lockUD.transform.localPosition.x);
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z))
                {
                    LockR2Up(selectedWheel, ref lockNumbers, lockWheel1, lockWheel2, lockWheel3, ref lockRotationUp, ref lockRotationDown);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    LockR2Down(selectedWheel, ref lockNumbers, lockWheel1, lockWheel2, lockWheel3, ref lockRotationUp, ref lockRotationDown);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
                {
                    LockR2Left(ref selectedWheel, lockWheel1, lockWheel2, lockWheel3, lockUD, lockRotationUp, lockRotationDown);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    LockR2Right(ref selectedWheel, lockWheel1, lockWheel2, lockWheel3, lockUD, lockRotationUp, lockRotationDown);
                }
            }
            else if (onLockIntro && !moveToLockIntro)
            {
                //"close" ui (give back control to the player) when the correct password is entered or when clicking on nothing
                if (lockIntro.First().GetComponent<Selectable>().solved || ((closeLockIntro.Count == 0 && Input.GetMouseButtonDown(0)) || Input.GetKeyDown(KeyCode.Escape)))
                {
                    CloseWindow();
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    int nb = lockIntroWheels.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        forGO = lockIntroWheels.getAt(i);
                        if (forGO.GetComponent<PointerOver>())
                        {
                            selectedWheelIntro.GetComponent<Renderer>().material.color = selectedWheelIntro.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                            selectedWheelIntro = forGO;
                            selectedWheelIntro.GetComponent<Renderer>().material.color = selectedWheelIntro.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                            lockIntroUD.transform.localPosition += Vector3.right * (selectedWheelIntro.transform.localPosition.x - lockIntroUD.transform.localPosition.x);
                        }
                    }
                }
                if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z))
                {
                    LockR2Up(selectedWheelIntro, ref lockIntroNumbers, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, ref lockIntroRotationUp, ref lockIntroRotationDown);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    LockR2Down(selectedWheelIntro, ref lockIntroNumbers, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, ref lockIntroRotationUp, ref lockIntroRotationDown);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
                {
                    LockR2Left(ref selectedWheelIntro, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, lockIntroUD, lockIntroRotationUp, lockIntroRotationDown);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    LockR2Right(ref selectedWheelIntro, lockIntroWheel1, lockIntroWheel2, lockIntroWheel3, lockIntroUD, lockIntroRotationUp, lockIntroRotationDown);
                }
            }
            else if (onBag && ((Input.GetMouseButtonDown(0) && overInventoryElem.Count == 0) || Input.GetKeyDown(KeyCode.Escape)) && !moveBag) {
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
					if (!onPiece && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))) {
						CloseWindow ();
					}
				}
			}
            else if (onBoard && !moveToBoard)
            {
                if ((closeBoard.Count == 0 && Input.GetMouseButtonDown(0)) || Input.GetKeyDown(KeyCode.Escape))
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
                            if (eraser.transform.localPosition.x > 0.021f)
                            {
                                eraser.transform.localPosition += Vector3.right * (0.021f - eraser.transform.localPosition.x);
                            }
                            else if (eraser.transform.localPosition.x < -0.021f)
                            {
                                eraser.transform.localPosition += Vector3.right * (-0.021f - eraser.transform.localPosition.x);
                            }
                            if (eraser.transform.localPosition.z > 0.016f)
                            {
                                eraser.transform.localPosition += Vector3.forward * (0.016f - eraser.transform.localPosition.z);
                            }
                            else if (eraser.transform.localPosition.z < -0.016f)
                            {
                                eraser.transform.localPosition += Vector3.forward * (-0.016f - eraser.transform.localPosition.z);
                            }
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
            if (HelpSystem.monitoring)
            {
                MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(53), "turnOff");
                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                HelpSystem.traces.Enqueue(trace);
            }
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
            ballSubTitles.text = "";
            moveBox = false;
            if (HelpSystem.monitoring)
            {
                MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(28), "turnOff");
                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                HelpSystem.traces.Enqueue(trace);
            }
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
            GameObjectManager.setGameObjectState(inventory.First(),true);
        } else if (onTable) {
			//start animation to move the player back to its position
			moveToTable = true;
            GameObjectManager.setGameObjectState(tableUI,false);
		} else if (onSheet)
        {
            Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = true;
            //set sheet to initial rotation and hide ui
            focusedSheet.transform.rotation = rotBeforeSheet;
			foreach (Canvas canvas in focusedSheet.GetComponentsInChildren<Canvas>()) {
				if (canvas.gameObject.name == "Display") {
                    GameObjectManager.setGameObjectState(canvas.gameObject,false);
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
                Camera.main.GetComponent<PostProcessingBehaviour>().profile.depthOfField.enabled = true;
            } else {
				//enable player
				player.First ().GetComponent<FirstPersonController> ().enabled = true;
				Cursor.lockState = CursorLockMode.None;
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		} else if (onCarillon) {
            GameObjectManager.setGameObjectState(carillonImage,false);
		}
        else if (onLockR2)
        {
            GameObjectManager.setGameObjectState(lockLR,false);
            GameObjectManager.setGameObjectState(lockUD,false);
            lockWheel1.GetComponent<Renderer>().material.color = lockWheelColor;
            lockWheel2.GetComponent<Renderer>().material.color = lockWheelColor;
            lockWheel3.GetComponent<Renderer>().material.color = lockWheelColor;
            lockR2.First().GetComponent<Selectable>().solved = false;
        }
        else if (onLockIntro)
        {
            GameObjectManager.setGameObjectState(lockIntroLR,false);
            GameObjectManager.setGameObjectState(lockIntroUD,false);
            lockIntroWheel1.GetComponent<Renderer>().material.color = lockIntroWheelColor;
            lockIntroWheel2.GetComponent<Renderer>().material.color = lockIntroWheelColor;
            lockIntroWheel3.GetComponent<Renderer>().material.color = lockIntroWheelColor;
            lockIntro.First().GetComponent<Selectable>().solved = false;
            if (HelpSystem.monitoring)
            {
                MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(3), "turnOff");
                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                HelpSystem.traces.Enqueue(trace);
            }
        }
        //show cursor
        GameObjectManager.setGameObjectState(cursorUI,true);
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
        onLockR2 = false;
        onLockIntro = false;
		onCarillon = false;
        onBoard = false;
    }
    
    private void LockR2Up(GameObject wheel, ref Vector3 numbers, GameObject wheel1, GameObject wheel2, GameObject wheel3, ref bool rotationUp, ref bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            rotationUp = true;
            if (Object.ReferenceEquals(wheel, wheel1))
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
            else if (Object.ReferenceEquals(wheel, wheel2))
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
            else if (Object.ReferenceEquals(wheel, wheel3))
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

    private void LockR2Down(GameObject wheel, ref Vector3 numbers, GameObject wheel1, GameObject wheel2, GameObject wheel3, ref bool rotationUp, ref bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            rotationDown = true;
            if (Object.ReferenceEquals(wheel, wheel1))
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
            else if (Object.ReferenceEquals(wheel, wheel2))
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
            else if (Object.ReferenceEquals(wheel, wheel3))
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

    private void LockR2Left(ref GameObject wheel, GameObject wheel1, GameObject wheel2, GameObject wheel3, GameObject lockUDUI, bool rotationUp, bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            if (Object.ReferenceEquals(wheel, wheel2))
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel1;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
            else if (Object.ReferenceEquals(wheel, wheel3))
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel2;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
        }
    }

    private void LockR2Right(ref GameObject wheel, GameObject wheel1, GameObject wheel2, GameObject wheel3, GameObject lockUDUI, bool rotationUp, bool rotationDown)
    {
        if (!rotationUp && !rotationDown)
        {
            if (Object.ReferenceEquals(wheel, wheel1))
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel2;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
            else if (Object.ReferenceEquals(wheel, wheel2))
            {
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                wheel = wheel3;
                wheel.GetComponent<Renderer>().material.color = wheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                lockUDUI.transform.localPosition += Vector3.right * (wheel.transform.localPosition.x - lockUDUI.transform.localPosition.x);
            }
        }
    }
}