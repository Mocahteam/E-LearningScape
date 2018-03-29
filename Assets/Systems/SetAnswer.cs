using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using TMPro;

public class SetAnswer : FSystem {

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family objects = FamilyManager.getFamily(new AnyOfTags("Object"), new AllOfComponents(typeof(Selectable)));
    private Family audioSource = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)));
    private Family images = FamilyManager.getFamily(new AllOfComponents(typeof(Image)));
    private Family qRoom1 = FamilyManager.getFamily(new AnyOfTags("Q-R1")); //questions of the room 1 (tablet)
    private Family tablet = FamilyManager.getFamily(new AnyOfTags("Tablet"));
    private Family aRoom1 = FamilyManager.getFamily(new AnyOfTags("A-R1")); //answers of the room 1 (tablet)
    private Family gears = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));
    private Family rotatingGears = FamilyManager.getFamily(new AnyOfTags("RotateGear")); //gears that can rotate (middle top, middle bot, and the solution gear)
    private Family wTimerText = FamilyManager.getFamily(new AnyOfTags("WrongTimerText"));
    private Family qRoom2 = FamilyManager.getFamily(new AnyOfTags("Q-R2")); //questions of the room 2 (tablet)
    private Family aRoom2 = FamilyManager.getFamily(new AnyOfTags("A-R2")); //answers of the room 2 (tablet)
    private Family door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));
    private Family lockR2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"));

    //used for the first prototype (not used anymore)
    private Family answers = FamilyManager.getFamily(new AnyOfTags("Answer"), new AllOfComponents(typeof(Button)));
    private Family displayAnswer = FamilyManager.getFamily(new AnyOfTags("Answer"), new NoneOfComponents(typeof(Button)));
    private bool initialized = false;
    private string vrai = "VRAI";
    private string faux = "FAUX";

    //elements used for visual and audio feedback when answering
    private GameObject rightBG;
    private float timeR = 0;
    private GameObject wrongBG;
    private float timeW = 0;
    private AudioSource source;

    string answer;

    private GameObject tablet1;
    private GameObject screen1;
    private int aq1r1 = 128;    //answer question 1 room 1
    private int aq2r1 = 459;    //answer question 2 room 1
    private string aq3r1 = "il faut savoir changer de posture"; //answer question 3 room 1
    private GameObject answersRoom1; //ui empty containing inputfields to answer
    private GameObject enigma4; //ui empty containing enigma04
    private GameObject whiteBG; // white ui image used for transition between answers and enigma04 on tablet
    private float timerWhite = Mathf.Infinity;  //timer used for the fading
    private bool fadingToEnigma4 = false;
    private GameObject gearDragged = null;
    private bool rotateGear = false;
    private float wTimerE04 = Mathf.Infinity;   //timer used when the player gives a wrong answer to enigma04
    private TextMeshProUGUI wtt;    //text displaying wTimerE04
    private bool wrongAnswerE04 = false;
    private bool doorSoundPlayed = false;

    private GameObject tablet2;
    private GameObject screen2;
    private GameObject connectionR2;
    private InputField ifConnectionR2;
    private string ifcR2Text;
    private TextMeshProUGUI connectionAnswerCheck1;
    private TextMeshProUGUI connectionAnswerCheck2;
    private TextMeshProUGUI connectionAnswerCheck3;
    private Color cacGreen;
    private Color cacOrange;
    private Color cacRed;
    private int connectionPassword = 789;
    private bool fadingToAnswersRoom2 = false;
    private GameObject answersRoom2; //ui empty containing inputfields to answer
    private string aq1r2 = "contrat pedagogique";
    private int aq2r2 = 914;
    private string aq3r2 = "smart";
    private int aq4r2 = 1956;
    private string aq5r2 = "grille criteriee";
    private string aq6r2 = "collaboration";
    
    private string previousTryPassword = "";
    private string password = 703.ToString();
    private GameObject wallRoom2;
    private bool moveWall = false;

    public SetAnswer()
    {
        door.First().transform.rotation = Quaternion.Euler(0, -135, 0); //opened
        //door.First().transform.rotation = Quaternion.Euler(0, 0, 0);    //closed
        wallRoom2 = lockR2.First().transform.parent.gameObject;

        foreach (GameObject go in tablet)
        {
            if (go.name.Contains(1.ToString()))
            {
                tablet1 = go;
                screen1 = tablet1.GetComponentInChildren<Canvas>().gameObject;
            }
            else if (go.name.Contains(2.ToString()))
            {
                tablet2 = go;
                screen2 = tablet2.GetComponentInChildren<Canvas>().gameObject;
            }
        }

        foreach (Transform child in screen1.transform)
        {
            if (child.gameObject.name == "AnswersInput")
            {
                answersRoom1 = child.gameObject;
            }
            else if (child.gameObject.name == "Enigma4")
            {
                enigma4 = child.gameObject;
            }
        }
        wtt = wTimerText.First().GetComponent<TextMeshProUGUI>();
        foreach (GameObject go in gears)
        {
            //set the initial position of each gear to their local position at the beginning of the game
            go.GetComponent<Gear>().initialPosition = go.transform.localPosition;
        }

        foreach (Transform child in screen2.transform)
        {
            if (child.gameObject.name == "ConnectionScreen")
            {
                connectionR2 = child.gameObject;
                ifConnectionR2 = connectionR2.GetComponentInChildren<InputField>();
                foreach(Transform c in child)
                {
                    if(c.gameObject.name == "AnswerCheck")
                    {
                        foreach (Transform cac in c)
                        {
                            if (cac.gameObject.name.Contains(1.ToString()))
                            {
                                connectionAnswerCheck1 = cac.gameObject.GetComponent<TextMeshProUGUI>();
                                cacGreen = connectionAnswerCheck1.color;
                            }
                            else if (cac.gameObject.name.Contains(2.ToString()))
                            {
                                connectionAnswerCheck2 = cac.gameObject.GetComponent<TextMeshProUGUI>();
                                cacOrange = connectionAnswerCheck2.color;
                            }
                            else if (cac.gameObject.name.Contains(3.ToString()))
                            {
                                connectionAnswerCheck3 = cac.gameObject.GetComponent<TextMeshProUGUI>();
                                cacRed = connectionAnswerCheck3.color;
                            }
                        }
                        break;
                    }
                }
            }
            else if (child.gameObject.name == "AnswersInput")
            {
                answersRoom2 = child.gameObject;
            }
        }

        //initialise buttons with listener
        foreach (GameObject b in answers)
        {
            b.GetComponent<Button>().onClick.AddListener(delegate { Answer(b.GetComponent<Button>()); });
        }
        foreach(GameObject g in displayAnswer)
        {
            if(g.name == "Result")
            {
                g.GetComponentInChildren<Button>().onClick.AddListener(CloseResult);
            }
        }
        foreach (GameObject go in qRoom1)
        {
            if (go.name.Contains(1.ToString()))
            {
                go.GetComponentInChildren<Button>().onClick.AddListener(CheckT1Answer1);
                go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckT1Answer1();
                    }
                });
            }
            else if (go.name.Contains(2.ToString()))
            {
                go.GetComponentInChildren<Button>().onClick.AddListener(CheckT1Answer2);
                go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckT1Answer2();
                    }
                });
            }
            else if (go.name.Contains(3.ToString()))
            {
                go.GetComponentInChildren<Button>().onClick.AddListener(CheckT1Answer3);
                go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckT1Answer3();
                    }
                });
            }
        }
        int id;
        foreach (GameObject go in qRoom2)
        {
            int.TryParse(go.name.Substring(go.name.Length - 1, 1), out id);
            switch (id)
            {
                case 1:
                    go.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer1);
                    go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer1();
                        }
                    });
                    break;

                case 2:
                    go.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer2);
                    go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer2();
                        }
                    });
                    break;

                case 3:
                    go.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer3);
                    go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer3();
                        }
                    });
                    break;

                case 4:
                    go.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer4);
                    go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer4();
                        }
                    });
                    break;

                case 5:
                    go.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer5);
                    go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer5();
                        }
                    });
                    break;

                case 6:
                    go.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer6);
                    go.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer6();
                        }
                    });
                    break;

                default:
                    break;
            }
        }
        connectionR2.GetComponentInChildren<Button>().onClick.AddListener(CheckConnection);
        connectionR2.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckConnection();
            }
        });
        lockR2.First().GetComponentInChildren<InputField>().onEndEdit.AddListener(CheckPasswordRoom2);

        //setting audio and visual elements for feedback when the player answers
        foreach (GameObject g in audioSource)
        {
            if(g.name == "Game")
            {
                source = g.GetComponent<AudioSource>(); //setting audio source playing "Right" and "Wrong" audios
            }
        }
        foreach(GameObject go in images)
        {
            if(go.name == "Right")
            {
                rightBG = go;
            }
            else if (go.name == "Wrong")
            {
                wrongBG = go;
            }
            else if (go.name == "White")
            {
                whiteBG = go;
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
	protected override void onProcess(int familiesUpdateCount) {
        //animation for the red/green blink when the answer is wrong/right
        float dr = Time.time - timeR;
        float dw = Time.time - timeW;
        if (dr < 0.1 || (dr < 0.4 && dr > 0.3))
        {
            rightBG.SetActive(true);
        }
        else
        {
            rightBG.SetActive(false);
        }
        if (dw < 0.1 || (dw <0.4 && dw >0.3))
        {
            wrongBG.SetActive(true);
        }
        else
        {
            wrongBG.SetActive(false);
        }

        if (moveWall)
        {
            wallRoom2.transform.position += Vector3.up * 0.01f + Vector3.forward * ((Random.value -0.5f)/10 - wallRoom2.transform.position.z);
            if(wallRoom2.transform.position.y > 7.5)
            {
                wallRoom2.SetActive(false);
                moveWall = false;
                source.loop = false;
            }
        }

        //tablet room 1 animation and interaction//
        //if the tablet is "solved" but enigma04 isn't diplayed
        if (tablet1.GetComponent<Selectable>().solved && (!enigma4.activeSelf || fadingToEnigma4))
        {
            //wait the end of the "Right answer" animation before fading to enigma04
            if (rightBG.activeSelf)
            {
                timerWhite = Time.time;
            }
            else //when the "Right answer" animation is finished
            {
                //from time: 0 to 1.5, screen: answers to white
                if (Time.time - timerWhite < 1.5f && Time.time - timerWhite >= 0f)
                {
                    fadingToEnigma4 = true;
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, (Time.time - timerWhite) / 1.5f);
                }
                //from time: 1.5 to 3, screen: white to enigma04
                else if (Time.time - timerWhite < 3f && Time.time - timerWhite > 1.5f)
                {
                    answersRoom1.SetActive(false);
                    enigma4.SetActive(true);
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 1 - (Time.time - timerWhite - 1.5f) / 1.5f);
                }
                else //if time not between 0 and 3
                {
                    //stop fading and set tablet to unsolved
                    whiteBG.SetActive(false);
                    tablet1.GetComponent<Selectable>().solved = false;
                    fadingToEnigma4 = false;
                }
            }
        }
        //if the tablet is "solved" and enigma04 is displayed
        else if (tablet1.GetComponent<Selectable>().solved && enigma4.activeSelf && !fadingToEnigma4 && !doorSoundPlayed)
        {
            //enigma 4 solved, open door to room 2
            door.First().transform.rotation = Quaternion.Euler(0, -135, 0);
            source.PlayOneShot(door.First().GetComponent<Door>().openAudio);
            doorSoundPlayed = true;
        }
        //if the player is playing enigma04 and didn't answer
        else if(!tablet1.GetComponent<Selectable>().solved && enigma4.activeSelf && !wrongAnswerE04)
        {
            foreach(GameObject gear in gears)
            {
                //if a gear is dragged
                if (gear.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
                {
                    gearDragged = gear; //save the dragged gear
                }
            }
            if(gearDragged != null) //if a gear is dragged
            {
                rotateGear = false; //initial value
                if (Input.GetMouseButtonUp(0))  //when the gear is released
                {
                    //if the gear is released in the center of the tablet (player answering)
                    if(gearDragged.transform.localPosition.x<125 && gearDragged.transform.localPosition.x > -125 && gearDragged.transform.localPosition.y < 125f/2 && gearDragged.transform.localPosition.x > -125f / 2)
                    {
                        gearDragged.transform.localPosition = Vector3.zero; //place the gear at the center
                        if (gearDragged.GetComponent<Gear>().isSolution) //if answer is correct
                        {
                            //start audio and animation for "Right answer"
                            source.PlayOneShot(tablet1.GetComponent<Selectable>().right);
                            timeR = Time.time;

                            rotateGear = true;  //rotate gears in the middle
                            tablet1.GetComponent<Selectable>().solved = true; //set tablet to solved
                            foreach(GameObject q in qRoom1)
                            {
                                //hide the question text of enigma04
                                if (q.name.Contains(4.ToString()))
                                {
                                    q.SetActive(false);
                                }
                            }
                        }
                        else //if answer is wrong
                        {
                            //start audio and animation for "Wrong answer"
                            source.PlayOneShot(tablet1.GetComponent<Selectable>().wrong);
                            timeW = Time.time;

                            foreach (GameObject q in qRoom1)
                            {
                                if (q.name.Contains(4.ToString()))
                                {
                                    q.SetActive(true);
                                }
                            }
                            //start the timer for wrong answer
                            wTimerE04 = Time.time;
                            wtt.gameObject.SetActive(true);
                            wtt.text = (15 + wTimerE04 - Time.time).ToString("n2");
                            wrongAnswerE04 = true;

                            gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition; //set gear position to initial position
                        }
                        gearDragged = null; //initial value
                    }
                    else //if the gear is not released in the center
                    {
                        gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition; //set gear position to initial position
                        foreach (GameObject q in qRoom1)
                        {
                            if (q.name.Contains(4.ToString()))
                            {
                                q.SetActive(true);
                            }
                        }
                    }
                    gearDragged = null; //initial value
                }
                else //when dragging a gear
                {
                    //move the gear to mouse position
                    gearDragged.transform.localPosition = (Input.mousePosition - Vector3.right * (float)Camera.main.pixelWidth / 2 - Vector3.up * (float)Camera.main.pixelHeight / 2)*0.627f;
                }
            }
        }
        else if (wrongAnswerE04) //true when the wrong gear is dragged in the center
        {
            if(15 + wTimerE04 - Time.time < 0)
            {
                //when the timer if finished, the player can drag a gear again
                wrongAnswerE04 = false;
                wtt.gameObject.SetActive(false);
            }
            else
            {
                //update the timer
                wtt.text = (15 + wTimerE04 - Time.time).ToString("n2");
            }
        }
        if (rotateGear) //true when the correct answer is given
        {
            foreach(GameObject g in rotatingGears)
            {
                //rotate gears in the middle
                if (g.GetComponent<Gear>())
                {
                    g.transform.rotation = Quaternion.Euler(g.transform.rotation.eulerAngles.x, g.transform.rotation.eulerAngles.y, g.transform.rotation.eulerAngles.z - 1);
                }
                else
                {
                    g.transform.rotation = Quaternion.Euler(g.transform.rotation.eulerAngles.x, g.transform.rotation.eulerAngles.y, g.transform.rotation.eulerAngles.z + 1);
                }
            }
        }

        //tablet room 2 animation and interaction//
        if (connectionR2.activeSelf)
        {
            if (ifConnectionR2.isFocused)
            {
                if(ifConnectionR2.text.Length > 2)
                {
                    if (ifConnectionR2.text.Length == 3)
                    {
                        ifcR2Text = ifConnectionR2.text;
                    }
                    ifConnectionR2.text = ifcR2Text;
                }
            }
        }
        if (tablet2.GetComponent<Selectable>().solved && fadingToAnswersRoom2)
        {
            //wait the end of the "Right answer" animation before fading to password
            if (rightBG.activeSelf)
            {
                timerWhite = Time.time;
            }
            else //when the "Right answer" animation is finished
            {
                //from time: 0 to 1.5, screen: answers to white
                if (Time.time - timerWhite < 1.5f && Time.time - timerWhite >= 0f)
                {
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, (Time.time - timerWhite) / 1.5f);
                    if(Time.time - timerWhite > 1.45f)
                    {
                        answersRoom2.SetActive(false);
                        foreach (Transform child in screen2.transform)
                        {
                            if (child.gameObject.name == "Background")
                            {
                                child.gameObject.SetActive(false);
                            }
                            else if (child.gameObject.name == "Background2")
                            {
                                child.gameObject.SetActive(true);
                            }
                            else if (child.gameObject.name == "Password")
                            {
                                child.gameObject.SetActive(true);
                            }
                        }
                    }
                }
                //from time: 1.5 to 3, screen: white to password
                else if (Time.time - timerWhite < 3f && Time.time - timerWhite > 1.5f)
                {
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 1 - (Time.time - timerWhite - 1.5f) / 1.5f);
                }
                else //if time not between 0 and 3
                {
                    //stop fading
                    whiteBG.SetActive(false);
                    fadingToAnswersRoom2 = false;
                }
            }
        }
        else if (fadingToAnswersRoom2)
        {
            //from time: 0 to 1.5, screen: connection to white
            if (Time.time - timerWhite < 1.5f && Time.time - timerWhite >= 0f)
            {
                whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, (Time.time - timerWhite) / 1.5f);
            }
            //from time: 1.5 to 3, screen: white to answers
            else if (Time.time - timerWhite < 3f && Time.time - timerWhite > 1.5f)
            {
                connectionR2.SetActive(false);
                answersRoom2.SetActive(true);
                whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 1 - (Time.time - timerWhite - 1.5f) / 1.5f);
            }
            else //if time not between 0 and 3
            {
                //stop fading
                whiteBG.SetActive(false);
                fadingToAnswersRoom2 = false;
            }
        }

        //used in the first prototype (not used anymore)
        if (!initialized)
        {
            foreach(GameObject ui in canvas)
            {
                if(ui.name == "UI" && ui.activeSelf)
                {
                    foreach(GameObject go in objects)
                    {
                        if (go.GetComponent<Selectable>().isSelected)
                        {
                            string[] words = null;
                            bool solved = false;
                            string answer = null;
                            words = go.GetComponent<Selectable>().words;
                            answer = go.GetComponent<Selectable>().answer;
                            solved = go.GetComponent<Selectable>().solved;
                            if (solved)
                            {
                                foreach (GameObject da in displayAnswer)
                                {
                                    if (da.name == "DisplayAnswer")
                                    {
                                        da.SetActive(true);
                                        da.GetComponent<Text>().text = answer;
                                    }
                                }
                            }
                            else
                            {
                                int nb = words.Length;
                                int i = 0;
                                foreach (GameObject b in answers)
                                {
                                    if (i < nb)
                                    {
                                        b.SetActive(true);
                                        b.GetComponentInChildren<Text>().text = words[i];
                                        i++;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    initialized = true;
                }
            }
        }
        else
        {
            foreach (GameObject ui in canvas)
            {
                if (ui.name == "UI" && !ui.activeSelf)
                {
                    foreach(GameObject b in answers)
                    {
                        b.SetActive(false);
                    }
                    foreach (GameObject da in displayAnswer)
                    {
                        if (da.name == "DisplayAnswer")
                        {
                            da.SetActive(false);
                        }
                    }
                    initialized = false;
                }
            }
        }
	}

    //used in the first prototype (not used anymore)
    void Answer(Button b)
    {
        foreach (GameObject g in answers)
        {
            g.SetActive(false);
        }
        foreach (GameObject go in objects)
        {
            if (go.GetComponent<Selectable>().isSelected)
            {
                if (b.GetComponentInChildren<Text>().text == go.GetComponent<Selectable>().answer)
                {
                    go.GetComponent<Selectable>().solved = true;
                    Timer.addTimer = true;
                    foreach (GameObject g in displayAnswer)
                    {
                        if (g.name == "Result")
                        {
                            g.GetComponent<Text>().text = vrai;
                            source.PlayOneShot(go.GetComponent<Selectable>().right);
                            timeR = Time.time;
                            g.SetActive(true);
                        }
                    }
                }
                else
                {
                    foreach (GameObject g in displayAnswer)
                    {
                        if (g.name == "Result")
                        {
                            g.GetComponent<Text>().text = faux;
                            source.PlayOneShot(go.GetComponent<Selectable>().wrong);
                            timeW = Time.time;
                            g.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    //used in the first prototype (not used anymore)
    void CloseResult()
    {
        foreach (GameObject g in displayAnswer)
        {
            if (g.name == "Result")
            {
                g.SetActive(false);
            }
        }
        foreach (GameObject go in objects)
        {
            if (go.GetComponent<Selectable>().isSelected)
            {
                if (go.GetComponent<Selectable>().solved)
                {
                    foreach (GameObject da in displayAnswer)
                    {
                        if (da.name == "DisplayAnswer")
                        {
                            da.SetActive(true);
                            da.GetComponent<Text>().text = go.GetComponent<Selectable>().answer;
                        }
                    }
                }
                else
                {
                    string[] words = go.GetComponent<Selectable>().words;
                    int nb = words.Length;
                    int i = 0;
                    foreach (GameObject b in answers)
                    {
                        if (i < nb)
                        {
                            b.SetActive(true);
                            b.GetComponentInChildren<Text>().text = words[i];
                            i++;
                        }
                    }
                }
            }
        }
    }

    /* check the answer of the first question on the tablet 1
     * called when the corresponding button is clicked
     */
    private void CheckT1Answer1()
    {
        foreach(GameObject q in qRoom1)
        {
            //find question 1 of room 1
            if (q.name.Contains(1.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text; //player's answer
                //if the answer's length is 3 and the answer contains aq1r1's numbers
                if (answer.Length == 3 && answer.Contains((aq1r1 / 100).ToString()) && answer.Contains(((aq1r1 / 10)%10).ToString()) && answer.Contains((aq1r1 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(tablet1.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom1)
                    {
                        if (a.name.Contains(1.ToString()))
                        {
                            //show the solution of question 1
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }

                    if (solved) //if all question are solved
                    {
                        tablet1.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        //start fading to enigma04
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet1.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT1Answer1(string s)
    {
        CheckT1Answer1();
    }

    /* check the answer of the second question on the tablet 1
     * called when the corresponding button is clicked
     */
    private void CheckT1Answer2()
    {
        foreach (GameObject q in qRoom1)
        {
            //find question 2 of room 1
            if (q.name.Contains(2.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text; //player's answer
                if (answer.Length == 3 && answer.Contains((aq2r1 / 100).ToString()) && answer.Contains(((aq2r1 / 10) % 10).ToString()) && answer.Contains((aq2r1 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(tablet1.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom1)
                    {
                        if (a.name.Contains(2.ToString()))
                        {
                            //show the solution of question 2
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet1.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        //start fading to enigma04
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet1.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT1Answer2(string s)
    {
        CheckT1Answer2();
    }

    /* check the answer of the third question on the tablet 1
     * called when the corresponding button is clicked
     */
    private void CheckT1Answer3()
    {
        foreach (GameObject q in qRoom1)
        {
            //find question 3 of room 1
            if (q.name.Contains(3.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text; //player's answer
                answer = answer.ToLower();  //minimize the answer
                if(answer == aq3r1) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet1.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom1)
                    {
                        if (a.name.Contains(3.ToString()))
                        {
                            //show the solution of question 3
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet1.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        //start fading to enigma04
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet1.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT1Answer3(string s)
    {
        CheckT1Answer3();
    }

    private void CheckConnection()
    {
        int answer;
        int.TryParse(connectionR2.GetComponentInChildren<InputField>().text, out answer);
        
        if(answer == connectionPassword)
        {
            connectionAnswerCheck1.text = "O";
            connectionAnswerCheck1.color = cacGreen;
            connectionAnswerCheck2.text = "O";
            connectionAnswerCheck2.color = cacGreen;
            connectionAnswerCheck3.text = "O";
            connectionAnswerCheck3.color = cacGreen;
            fadingToAnswersRoom2 = true;
            timerWhite = Time.time;
            whiteBG.SetActive(true);
            whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 0);
        }
        else {
            connectionR2.GetComponentInChildren<InputField>().ActivateInputField();
            if (answer / 100 == connectionPassword / 100)
            {
                connectionAnswerCheck1.text = "O";
                connectionAnswerCheck1.color = cacGreen;
            }
            else if (connectionPassword.ToString().Contains((answer / 100).ToString()))
            {
                connectionAnswerCheck1.text = "?";
                connectionAnswerCheck1.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck1.text = "X";
                connectionAnswerCheck1.color = cacRed;
            }

            if (answer / 10 % 10 == connectionPassword / 10 % 10)
            {
                connectionAnswerCheck2.text = "O";
                connectionAnswerCheck2.color = cacGreen;
            }
            else if (connectionPassword.ToString().Contains((answer / 10 % 10).ToString()))
            {
                connectionAnswerCheck2.text = "?";
                connectionAnswerCheck2.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck2.text = "X";
                connectionAnswerCheck2.color = cacRed;
            }

            if (answer % 10 == connectionPassword % 10)
            {
                connectionAnswerCheck3.text = "O";
                connectionAnswerCheck3.color = cacGreen;
            }
            else if (connectionPassword.ToString().Contains((answer % 10).ToString()))
            {
                connectionAnswerCheck3.text = "?";
                connectionAnswerCheck3.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck3.text = "X";
                connectionAnswerCheck3.color = cacRed;
            }
        }
    }

    private void CheckConnection(string s)
    {
        CheckConnection();
    }

    private void CheckT2Answer1()
    {
        foreach(GameObject q in qRoom2)
        {
            if (q.name.Contains(1.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();
                answer = answer.Replace('é', 'e');
                answer = answer.Replace('è', 'e');
                
                if (answer == aq1r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom2)
                    {
                        if (a.name.Contains(1.ToString()))
                        {
                            //show the solution of question 1
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet2.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        fadingToAnswersRoom2 = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer1(string s)
    {
        CheckT2Answer1();
    }

    private void CheckT2Answer2()
    {
        foreach (GameObject q in qRoom2)
        {
            if (q.name.Contains(2.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text;

                //if the answer's length is 3 and the answer contains aq1r1's numbers
                if (answer.Length == 3 && answer.Contains((aq2r2 / 100).ToString()) && answer.Contains(((aq2r2 / 10) % 10).ToString()) && answer.Contains((aq2r2 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom2)
                    {
                        if (a.name.Contains(2.ToString()))
                        {
                            //show the solution of question 2
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet2.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        fadingToAnswersRoom2 = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer2(string s)
    {
        CheckT2Answer2();
    }

    private void CheckT2Answer3()
    {
        foreach (GameObject q in qRoom2)
        {
            if (q.name.Contains(3.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();

                if (answer == aq3r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom2)
                    {
                        if (a.name.Contains(3.ToString()))
                        {
                            //show the solution of question 3
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet2.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        fadingToAnswersRoom2 = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer3(string s)
    {
        CheckT2Answer3();
    }

    private void CheckT2Answer4()
    {
        foreach (GameObject q in qRoom2)
        {
            if (q.name.Contains(4.ToString()))
            {
                int answer;
                int.TryParse(q.GetComponentInChildren<InputField>().text, out answer);

                if (answer == aq4r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom2)
                    {
                        if (a.name.Contains(4.ToString()))
                        {
                            //show the solution of question 4
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet2.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        fadingToAnswersRoom2 = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer4(string s)
    {
        CheckT2Answer4();
    }

    private void CheckT2Answer5()
    {
        foreach (GameObject q in qRoom2)
        {
            if (q.name.Contains(5.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();
                answer = answer.Replace('é', 'e');
                answer = answer.Replace('è', 'e');
                
                if (answer == aq5r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom2)
                    {
                        if (a.name.Contains(5.ToString()))
                        {
                            //show the solution of question 5
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet2.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        fadingToAnswersRoom2 = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer5(string s)
    {
        CheckT2Answer5();
    }

    private void CheckT2Answer6()
    {
        foreach (GameObject q in qRoom2)
        {
            if (q.name.Contains(6.ToString()))
            {
                answer = q.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();
                
                if (answer == aq6r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    q.SetActive(false); //hide the question
                    bool solved = true;
                    foreach (GameObject a in aRoom2)
                    {
                        if (a.name.Contains(6.ToString()))
                        {
                            //show the solution of question 6
                            a.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        tablet2.GetComponent<Selectable>().solved = true;    //set tablet to solved
                        fadingToAnswersRoom2 = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer6(string s)
    {
        CheckT2Answer6();
    }

    private void CheckPasswordRoom2(string value)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (value != previousTryPassword && value != "")
            {
                previousTryPassword = value;
                if (value == password)
                {
                    lockR2.First().GetComponent<Selectable>().solved = true;
                    moveWall = true;
                    source.clip = lockR2.First().GetComponent<Selectable>().right;
                    source.PlayDelayed(0);
                    source.loop = true;
                }
                else
                {
                    //feedback wrong answer
                    source.PlayOneShot(tablet2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }
}