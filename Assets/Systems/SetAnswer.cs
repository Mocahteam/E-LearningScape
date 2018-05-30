using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;

public class SetAnswer : FSystem
{

    private Family audioSource = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)));
    private Family images = FamilyManager.getFamily(new AllOfComponents(typeof(Image)));
    private Family qRoom1 = FamilyManager.getFamily(new AnyOfTags("Q-R1")); //questions of the room 1 (tablet)
    private Family screenIAR = FamilyManager.getFamily(new AnyOfTags("ScreenIAR"));
    private Family aRoom1 = FamilyManager.getFamily(new AnyOfTags("A-R1")); //answers of the room 1 (tablet)
    private Family gears = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));
    private Family rotatingGears = FamilyManager.getFamily(new AnyOfTags("RotateGear")); //gears that can rotate (middle top, middle bot, and the solution gear)
    private Family wTimerText = FamilyManager.getFamily(new AnyOfTags("WrongTimerText"));
    private Family qRoom2 = FamilyManager.getFamily(new AnyOfTags("Q-R2")); //questions of the room 2 (tablet)
    private Family aRoom2 = FamilyManager.getFamily(new AnyOfTags("A-R2")); //answers of the room 2 (tablet)
    private Family symbolsE12Tag = FamilyManager.getFamily(new AnyOfTags("E12_Symbol"));
    private Family symbolsE12Component = FamilyManager.getFamily(new AllOfComponents(typeof(E12_Symbol)));
    private Family removableBoardWords = FamilyManager.getFamily(new AnyOfTags("BoardWords"));
    private Family qRoom3 = FamilyManager.getFamily(new AnyOfTags("Q-R3")); //questions of the room 3 (tablet)
    private Family aRoom3 = FamilyManager.getFamily(new AnyOfTags("A-R3")); //answers of the room 3 (tablet)
    private Family cGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family glassesBackgrounds = FamilyManager.getFamily(new AnyOfTags("GlassesBG"));
    private Family game = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family endRoom = FamilyManager.getFamily(new AnyOfTags("EndRoom"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family waterFloor = FamilyManager.getFamily(new AnyOfTags("WaterFloor"));
    private Family collectableGO = FamilyManager.getFamily(new AllOfComponents(typeof(CollectableGO)));


    //elements used for visual and audio feedback when answering
    private GameObject rightBG;
    private float timeR = 0;
    private GameObject wrongBG;
    private float timeW = 0;
    private AudioSource source;

    string answer;
    
    private GameObject screen1;
    private int aq1r1 = 128;    //answer question 1 room 1
    private int aq2r1 = 459;    //answer question 2 room 1
    private string aq3r1 = "1623810149"; //answer question 3 room 1
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
    private int connectionPassword = 345;
    private bool fadingToAnswersRoom2 = false;
    private GameObject answersRoom2; //ui empty containing inputfields to answer
    private string aq1r2 = "contrat pedagogique";
    private int aq2r2 = 914;
    private string aq3r2 = "smart";
    private int aq4r2 = 1956;
    private string aq5r2 = "312";
    private string aq6r2 = "collaboration";
    
    private GameObject screen3;
    private GameObject answersRoom3;
    private string aq1r3 = "planification";
    private string aq2r3 = "ressources";
    private string aq3r3 = "contraintes";
    private string aq4r3 = "contenu";
    private bool answer1R3Given = false;
    private bool answer2R3Given = false;
    private bool answer3R3Given = false;
    private bool answer4R3Given = false;
    private bool fadingToEndRoom = false;

    private bool usingLamp = false;
    private string symbolLetter;

    public static bool credits = true;

    //tmp gameobjects used to loop in famillies
    private GameObject forGO;
    private GameObject forGO2;
    private GameObject forGO3;

    public SetAnswer()
    {
        credits = false;
        timeR = -Mathf.Infinity;
        timeW = -Mathf.Infinity;
        timerWhite = -Mathf.Infinity;

        int nbScreen = screenIAR.Count;
        for (int i = 0; i < nbScreen; i++)
        {
            forGO = screenIAR.getAt(i);
            if (forGO.name.Contains(1.ToString()))
            {
                screen1 = forGO;
            }
            else if (forGO.name.Contains(2.ToString()))
            {
                screen2 = forGO;
            }
            else if (forGO.name.Contains(3.ToString()))
            {
                screen3 = forGO;
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
        int nbGears = gears.Count;
        for (int i = 0; i < nbGears; i++)
        {
            forGO = gears.getAt(i);
            //set the initial position of each gear to their local position at the beginning of the game
            forGO.GetComponent<Gear>().initialPosition = forGO.transform.localPosition;
        }

        foreach (Transform child in screen2.transform)
        {
            if (child.gameObject.name == "ConnectionScreen")
            {
                connectionR2 = child.gameObject;
                foreach(InputField inputField in connectionR2.GetComponentsInChildren<InputField>())
                {
                    if(inputField.gameObject.name == "Password")
                    {
                        ifConnectionR2 = inputField;
                        break;
                    }
                }
                foreach (Transform c in ifConnectionR2.gameObject.transform)
                {
                    if (c.gameObject.name == "AnswerCheck")
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
        foreach (Transform child in screen3.transform)
        {
            if (child.gameObject.name == "AnswersInput")
            {
                answersRoom3 = child.gameObject;
            }
        }

        int nbSymbols = symbolsE12Component.Count;
        for (int i = 0; i < nbSymbols; i++)
        {
            forGO = symbolsE12Component.getAt(i);
            forGO.GetComponent<E12_Symbol>().position = forGO.transform.position;
        }

        //initialise buttons with listener
        int nb = qRoom1.Count;
        for (int i = 0; i < nb; i++)
        {
            forGO = qRoom1.getAt(i);
            if (forGO.name.Contains(1.ToString()))
            {
                forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT1Answer1);
                forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckT1Answer1();
                    }
                });
            }
            else if (forGO.name.Contains(2.ToString()))
            {
                forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT1Answer2);
                forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckT1Answer2();
                    }
                });
            }
            else if (forGO.name.Contains(3.ToString()))
            {
                forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT1Answer3);
                forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckT1Answer3();
                    }
                });
            }
        }
        int id;
        nb = qRoom2.Count;
        for (int i = 0; i < nb; i++)
        {
            forGO = qRoom2.getAt(i);
            int.TryParse(forGO.name.Substring(forGO.name.Length - 1, 1), out id);
            switch (id)
            {
                case 1:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer1);
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer1();
                        }
                    });
                    break;

                case 2:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer2);
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer2();
                        }
                    });
                    break;

                case 3:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer3);
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer3();
                        }
                    });
                    break;

                case 4:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer4);
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer4();
                        }
                    });
                    break;

                case 5:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer5);
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT2Answer5();
                        }
                    });
                    break;

                case 6:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(CheckT2Answer6);
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
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
        foreach (InputField inputField in connectionR2.GetComponentsInChildren<InputField>())
        {
            if (inputField.gameObject.name == "Password")
            {
                inputField.onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckConnection();
                    }
                }); ;
                break;
            }
        }
        nb = qRoom3.Count;
        for (int i = 0; i < nb; i++)
        {
            forGO = qRoom3.getAt(i);
            switch (i)
            {
                case 0:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                        CheckT3Answer(qRoom3.getAt(0));
                    });
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT3Answer(qRoom3.getAt(0));
                        }
                    });
                    break;

                case 1:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                        CheckT3Answer(qRoom3.getAt(1));
                    });
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT3Answer(qRoom3.getAt(1));
                        }
                    });
                    break;

                case 2:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                        CheckT3Answer(qRoom3.getAt(2));
                    });
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT3Answer(qRoom3.getAt(2));
                        }
                    });
                    break;

                case 3:
                    forGO.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                        CheckT3Answer(qRoom3.getAt(3));
                    });
                    forGO.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckT3Answer(qRoom3.getAt(3));
                        }
                    });
                    break;

                default:
                    break;
            }
        }


        //setting audio and visual elements for feedback when the player answers
        nb = audioSource.Count;
        for (int i = 0; i < nb; i++)
        {
            forGO = audioSource.getAt(i);
            if (forGO.name == "Game")
            {
                source = forGO.GetComponent<AudioSource>(); //setting audio source playing "Right" and "Wrong" audios
            }
        }
        nb = images.Count;
        for (int i = 0; i < nb; i++)
        {
            forGO = images.getAt(i);
            if (forGO.name == "Right")
            {
                rightBG = forGO;
            }
            else if (forGO.name == "Wrong")
            {
                wrongBG = forGO;
            }
            else if (forGO.name == "White")
            {
                whiteBG = forGO;
            }
        }
        /* set all board's removable words to "occludable"
         * the occlusion is then made by an invisible material that hides all objects behind it having this setting
         */
        nb = removableBoardWords.Count;
        Renderer[] renderers;
        for (int i = 0; i < nb; i++)
        {
            renderers = removableBoardWords.getAt(i).GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.material.renderQueue = 3002;
            }
        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        //animation for the red/green blink when the answer is wrong/right
        if (Selectable.askRight)
        {
            Selectable.askRight = false;
            //feedback right answer
            source.PlayOneShot(screen1.GetComponent<Selectable>().right);
            timeR = Time.time;
        }
        if (Selectable.askWrong)
        {
            //feedback wrong answer
            source.PlayOneShot(screen1.GetComponent<Selectable>().wrong);
            timeW = Time.time;
            Selectable.askWrong = false;
        }
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
        if (dw < 0.1 || (dw < 0.4 && dw > 0.3))
        {
            wrongBG.SetActive(true);
        }
        else
        {
            wrongBG.SetActive(false);
        }

        //tablet room 1 animation and interaction//
        //if the tablet is "solved" but enigma04 isn't diplayed
        if (screen1.GetComponent<Selectable>().solved && (!enigma4.activeSelf || fadingToEnigma4))
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
                    screen1.GetComponent<Selectable>().solved = false;
                    fadingToEnigma4 = false;
                }
            }
        }
        //if the tablet is "solved" and enigma04 is displayed
        else if (screen1.GetComponent<Selectable>().solved && enigma4.activeSelf && !fadingToEnigma4 && !IARTab.room2Unlocked)
        {
            //enigma 4 solved, open door to room 2
            if(dr > 2f)
            {
                IARTab.room2Unlocked = true;
            }
        }
        //if the player is playing enigma04 and didn't answer
        else if (!screen1.GetComponent<Selectable>().solved && enigma4.activeSelf && !wrongAnswerE04)
        {
            int nbGears = gears.Count;
            for (int i = 0; i < nbGears; i++)
            {
                forGO = gears.getAt(i);
                //if a gear is dragged
                if (forGO.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
                {
                    gearDragged = forGO; //save the dragged gear
                }
            }
            if (gearDragged != null) //if a gear is dragged
            {
                rotateGear = false; //initial value
                if (Input.GetMouseButtonUp(0))  //when the gear is released
                {
                    //if the gear is released in the center of the tablet (player answering)
                    if (gearDragged.transform.localPosition.x < 125 && gearDragged.transform.localPosition.x > -125 && gearDragged.transform.localPosition.y < 125f / 2 && gearDragged.transform.localPosition.x > -125f / 2)
                    {
                        gearDragged.transform.localPosition = Vector3.zero; //place the gear at the center
                        if (gearDragged.GetComponent<Gear>().isSolution) //if answer is correct
                        {
                            //start audio and animation for "Right answer"
                            source.PlayOneShot(screen1.GetComponent<Selectable>().right);
                            timeR = Time.time;

                            rotateGear = true;  //rotate gears in the middle
                            screen1.GetComponent<Selectable>().solved = true; //set tablet to solved
                            int nbQRoom1 = qRoom1.Count;
                            for (int i = 0; i < nbQRoom1; i++)
                            {
                                forGO = qRoom1.getAt(i);
                                //hide the question text of enigma04
                                if (forGO.name.Contains(4.ToString()))
                                {
                                    forGO.SetActive(false);
                                }
                            }
                        }
                        else //if answer is wrong
                        {
                            //start audio and animation for "Wrong answer"
                            source.PlayOneShot(screen1.GetComponent<Selectable>().wrong);
                            timeW = Time.time;

                            int nbQRoom1 = qRoom1.Count;
                            for (int i = 0; i < nbQRoom1; i++)
                            {
                                forGO = qRoom1.getAt(i);
                                if (forGO.name.Contains(4.ToString()))
                                {
                                    forGO.SetActive(true);
                                }
                            }
                            //start the timer for wrong answer
                            wTimerE04 = Time.time;
                            wtt.gameObject.SetActive(true);
                            wtt.text = (5 + wTimerE04 - Time.time).ToString("n2");
                            wrongAnswerE04 = true;

                            gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition; //set gear position to initial position
                        }
                        gearDragged = null; //initial value
                    }
                    else //if the gear is not released in the center
                    {
                        gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition; //set gear position to initial position
                        int nbQRoom1 = qRoom1.Count;
                        for (int i = 0; i < nbQRoom1; i++)
                        {
                            forGO = qRoom1.getAt(i);
                            if (forGO.name.Contains(4.ToString()))
                            {
                                forGO.SetActive(true);
                            }
                        }
                    }
                    gearDragged = null; //initial value
                }
                else //when dragging a gear
                {
                    //move the gear to mouse position
                    gearDragged.transform.localPosition = (Input.mousePosition - Vector3.right * (float)Camera.main.pixelWidth / 2 - Vector3.up * (float)Camera.main.pixelHeight / 2) * 0.627f;
                }
            }
        }
        else if (wrongAnswerE04) //true when the wrong gear is dragged in the center
        {
            if (wTimerE04 - Time.time < 0)
            {
                //when the timer if finished, the player can drag a gear again
                wrongAnswerE04 = false;
                wtt.gameObject.SetActive(false);
            }
            else
            {
                //update the timer
                wtt.text = (5 + wTimerE04 - Time.time).ToString("n2");
            }
        }
        if (rotateGear) //true when the correct answer is given
        {
            int nbRotGears = rotatingGears.Count;
            for (int i = 0; i < nbRotGears; i++)
            {
                forGO = rotatingGears.getAt(i);
                //rotate gears in the middle
                if (forGO.GetComponent<Gear>())
                {
                    forGO.transform.rotation = Quaternion.Euler(forGO.transform.rotation.eulerAngles.x, forGO.transform.rotation.eulerAngles.y, forGO.transform.rotation.eulerAngles.z - 1);
                }
                else
                {
                    forGO.transform.rotation = Quaternion.Euler(forGO.transform.rotation.eulerAngles.x, forGO.transform.rotation.eulerAngles.y, forGO.transform.rotation.eulerAngles.z + 1);
                }
            }
        }

        //tablet room 2 animation and interaction//
        if (connectionR2.activeSelf)
        {
            if (ifConnectionR2.isFocused)
            {
                if (ifConnectionR2.text.Length > 2)
                {
                    if (ifConnectionR2.text.Length == 3)
                    {
                        ifcR2Text = ifConnectionR2.text;
                    }
                    ifConnectionR2.text = ifcR2Text;
                }
            }
        }
        if (screen2.GetComponent<Selectable>().solved && fadingToAnswersRoom2)
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
                    if (Time.time - timerWhite > 1.45f)
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
        if (screen3.GetComponent<Selectable>().solved && fadingToEndRoom)
        {
            //wait the end of the "Right answer" animation before fading to end room
            if (rightBG.activeSelf)
            {
                timerWhite = Time.time;
            }
            else //when the "Right answer" animation is finished
            {
                //from time: 0 to 2, screen: answers to white
                if (Time.time - timerWhite < 2f && Time.time - timerWhite >= 0f)
                {
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, (Time.time - timerWhite) / 2f);
                    if (Time.time - timerWhite > 1.95f)
                    {
                        player.First().transform.position = new Vector3(74, 0.33f, -3);
                        player.First().transform.localRotation = Quaternion.Euler(0, -90, 0);
                        Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        game.First().SetActive(false);
                        endRoom.First().SetActive(true);
                        if (player.First().GetComponentInChildren<Light>())
                        {
                            player.First().GetComponentInChildren<Light>().gameObject.SetActive(false);
                        }
                        IARTab.askCloseIAR = true;
                        RenderSettings.fogDensity = 0;
                        Camera.main.farClipPlane = 300;
                        foreach(Transform child in endRoom.First().transform)
                        {
                            if (child.gameObject.GetComponent<MeshRenderer>())
                            {
                                child.gameObject.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
                            }
                        }
                        foreach (Transform child in waterFloor.First().transform)
                        {
                            if (child.gameObject.GetComponent<MeshRenderer>())
                            {
                                child.gameObject.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
                            }
                        }
                    }
                }
                //from time: 2 to 4, screen: white to end room
                else if (Time.time - timerWhite < 4f && Time.time - timerWhite > 2f)
                {
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 1 - (Time.time - timerWhite - 2) / 2f);
                }
                else //if time not between 0 and 4
                {
                    foreach (FSystem s in FSystemManager.updateSystems())
                    {
                        //stop all FYFY systems but "SetAnswer" and "StoryDisplaying"
                        if (s.ToString() != "SetAnswer" && s.ToString() != "StoryDisplaying" && !(s.ToString() == "DreamFragmentCollect" && credits))
                        {
                            s.Pause = true;
                        }
                    }
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    //stop fading
                    whiteBG.SetActive(false);
                    fadingToEndRoom = false;
                }
            }
        }

        if (CollectableGO.usingLamp)
        {
            int nbSymbols = symbolsE12Tag.Count;
            for (int i = 0; i < nbSymbols; i++)
            {
                forGO = symbolsE12Tag.getAt(i);
                Vector3 position = forGO.GetComponentInChildren<E12_Symbol>().position;
                //if the symbol is illuminated by the lamp
                if (Vector3.Angle(position - Camera.main.transform.position, Camera.main.transform.forward) < 22)
                {
                    forGO.SetActive(true);
                    //calculate the intersection between player direction and the wall
                    float d = Vector3.Dot((position - Camera.main.transform.position), forGO.transform.forward) / Vector3.Dot(Camera.main.transform.forward, forGO.transform.forward);
                    //move the mask to the calculated position
                    forGO.transform.position = Camera.main.transform.position + Camera.main.transform.forward * d;
                    //set the symbol position to its initial position (it shouldn't move but it is moved because of it being the mask's child)
                    forGO.GetComponentInChildren<E12_Symbol>().gameObject.transform.position = position;
                    //calculate the new scale of the mask (depending on the distance with the player)
                    float a = (0.026f - 0.015f) / (5.49f - 3.29f);
                    float b = 0.026f - a * 5.49f;
                    float scale = a * (forGO.transform.position - Camera.main.transform.position).magnitude + b;
                    //change the scale of the mask and set the symbol scale to its initial scale
                    forGO.GetComponentInChildren<E12_Symbol>().gameObject.transform.localScale *= forGO.transform.localScale.x / scale * forGO.transform.parent.localScale.x * 100;
                    forGO.transform.localScale = new Vector3(scale, scale, scale) / forGO.transform.parent.localScale.x / 100;
                }
                else
                {
                    //disable the mask and the symbol
                    forGO.transform.position = position;
                    forGO.GetComponentInChildren<E12_Symbol>().gameObject.transform.position = position;
                    forGO.SetActive(false);
                }
            }
        }
        //when lamp just got disabled
        else if (usingLamp)
        {
            //disable all symbols and masks
            int nbSymbols = symbolsE12Tag.Count;
            for (int i = 0; i < nbSymbols; i++)
            {
                symbolsE12Tag.getAt(i).SetActive(false);
            }
        }
        usingLamp = CollectableGO.usingLamp;
    }

    /* check the answer of the first question on the tablet 1
     * called when the corresponding button is clicked
     */
    private void CheckT1Answer1()
    {
        int nbQRoom1 = qRoom1.Count;
        for (int i = 0; i < nbQRoom1; i++)
        {
            forGO = qRoom1.getAt(i);
            //find question 1 of room 1
            if (forGO.name.Contains(1.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text; //player's answer
                //if the answer's length is 3 and the answer contains aq1r1's numbers
                if (answer.Length == 3 && answer.Contains((aq1r1 / 100).ToString()) && answer.Contains(((aq1r1 / 10) % 10).ToString()) && answer.Contains((aq1r1 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(screen1.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    int nb = collectableGO.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        forGO2 = collectableGO.getAt(j);
                        if (forGO2.name.Contains("KeyE03"))
                        {
                            forGO2.SetActive(false);
                        }
                    }
                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom1 = aRoom1.Count;
                    for (int j = 0; j < nbARoom1; j++)
                    {
                        forGO2 = aRoom1.getAt(j);
                        if (forGO2.name.Contains(1.ToString()))
                        {
                            //show the solution of question 1
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }

                    if (solved) //if all question are solved
                    {
                        screen1.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen1.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    /* check the answer of the second question on the tablet 1
     * called when the corresponding button is clicked
     */
    private void CheckT1Answer2()
    {
        int nbQRoom1 = qRoom1.Count;
        for (int i = 0; i < nbQRoom1; i++)
        {
            forGO = qRoom1.getAt(i);
            //find question 2 of room 1
            if (forGO.name.Contains(2.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text; //player's answer
                if (answer.Length == 3 && answer.Contains((aq2r1 / 100).ToString()) && answer.Contains(((aq2r1 / 10) % 10).ToString()) && answer.Contains((aq2r1 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(screen1.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    
                    int nb = collectableGO.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        forGO2 = collectableGO.getAt(j);
                        if (forGO2.name.Contains("Syllabus") || forGO2.name.Contains("Wire"))
                        {
                            forGO2.SetActive(false);
                        }
                    }
                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom1 = aRoom1.Count;
                    for (int j = 0; j < nbARoom1; j++)
                    {
                        forGO2 = aRoom1.getAt(j);
                        if (forGO2.name.Contains(2.ToString()))
                        {
                            //show the solution of question 2
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen1.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen1.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    /* check the answer of the third question on the tablet 1
     * called when the corresponding button is clicked
     */
    private void CheckT1Answer3()
    {
        int nbQRoom1 = qRoom1.Count;
        for (int i = 0; i < nbQRoom1; i++)
        {
            forGO = qRoom1.getAt(i);
            //find question 3 of room 1
            if (forGO.name.Contains(3.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text; //player's answer
                if (answer == aq3r1) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(screen1.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom1 = aRoom1.Count;
                    for (int j = 0; j < nbARoom1; j++)
                    {
                        forGO2 = aRoom1.getAt(j);
                        if (forGO2.name.Contains(3.ToString()))
                        {
                            //show the solution of question 3
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen1.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen1.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckConnection() //mastermind
    {
        int answer;
        int.TryParse(ifConnectionR2.text, out answer);

        if (answer == connectionPassword) //if the answer is correct
        {
            //show correct answer feedback for the 3 numbers
            connectionAnswerCheck1.text = "O";
            connectionAnswerCheck1.color = cacGreen;
            connectionAnswerCheck2.text = "O";
            connectionAnswerCheck2.color = cacGreen;
            connectionAnswerCheck3.text = "O";
            connectionAnswerCheck3.color = cacGreen;

            //start fading to the next step of the tablet (questions and inpufields)
            fadingToAnswersRoom2 = true;
            timerWhite = Time.time;
            whiteBG.SetActive(true);
            whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 0);
        }
        else
        {
            ifConnectionR2.ActivateInputField();
            //else, feedback following the rules of mastermind ('O' correct, '?' right number but wrong place, 'X' wrong number)
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

    private void CheckT2Answer1()
    {
        int nbQRoom2 = qRoom2.Count;
        for (int i = 0; i < nbQRoom2; i++)
        {
            forGO = qRoom2.getAt(i);
            if (forGO.name.Contains(1.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();
                answer = answer.Replace('é', 'e');
                answer = answer.Replace('è', 'e');

                if (answer == aq1r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(screen2.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    
                    int nb = collectableGO.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        forGO2 = collectableGO.getAt(j);
                        if (forGO2.name.Contains("Glasses") || forGO2.name.Contains("KeyE08"))
                        {
                            forGO2.SetActive(false);
                        }
                    }
                    nb = glassesBackgrounds.Count;
                    for(int j = 0; j < nb; j++)
                    {
                        glassesBackgrounds.getAt(j).SetActive(false);
                    }
                    CollectableGO.usingGlasses1 = false;
                    CollectableGO.usingGlasses2 = false;
                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom2 = aRoom2.Count;
                    for (int j = 0; j < nbARoom2; j++)
                    {
                        forGO2 = aRoom2.getAt(j);
                        if (forGO2.name.Contains(1.ToString()))
                        {
                            //show the solution of question 1
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen2.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer2()
    {
        int nbQRoom2 = qRoom2.Count;
        for (int i = 0; i < nbQRoom2; i++)
        {
            forGO = qRoom2.getAt(i);
            if (forGO.name.Contains(2.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text;

                //if the answer's length is 3 and the answer contains aq1r1's numbers
                if (answer.Length == 3 && answer.Contains((aq2r2 / 100).ToString()) && answer.Contains(((aq2r2 / 10) % 10).ToString()) && answer.Contains((aq2r2 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(screen2.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    
                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom2 = aRoom2.Count;
                    for (int j = 0; j < nbARoom2; j++)
                    {
                        forGO2 = aRoom2.getAt(j);
                        if (forGO2.name.Contains(2.ToString()))
                        {
                            //show the solution of question 2
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen2.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer3()
    {
        int nbQRoom2 = qRoom2.Count;
        for (int i = 0; i < nbQRoom2; i++)
        {
            forGO = qRoom2.getAt(i);
            if (forGO.name.Contains(3.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();

                if (answer == aq3r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(screen2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom2 = aRoom2.Count;
                    for (int j = 0; j < nbARoom2; j++)
                    {
                        forGO2 = aRoom2.getAt(j);
                        if (forGO2.name.Contains(3.ToString()))
                        {
                            //show the solution of question 3
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen2.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer4()
    {
        int nbQRoom2 = qRoom2.Count;
        for (int i = 0; i < nbQRoom2; i++)
        {
            forGO = qRoom2.getAt(i);
            if (forGO.name.Contains(4.ToString()))
            {
                int answer;
                int.TryParse(forGO.GetComponentInChildren<InputField>().text, out answer);

                if (answer == aq4r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(screen2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom2 = aRoom2.Count;
                    for (int j = 0; j < nbARoom2; j++)
                    {
                        forGO2 = aRoom2.getAt(j);
                        if (forGO2.name.Contains(4.ToString()))
                        {
                            //show the solution of question 4
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen2.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer5()
    {
        int nbQRoom2 = qRoom2.Count;
        for (int i = 0; i < nbQRoom2; i++)
        {
            forGO = qRoom2.getAt(i);
            if (forGO.name.Contains(5.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text;

                if (answer == aq5r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(screen2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom2 = aRoom2.Count;
                    for (int j = 0; j < nbARoom2; j++)
                    {
                        forGO2 = aRoom2.getAt(j);
                        if (forGO2.name.Contains(5.ToString()))
                        {
                            //show the solution of question 5
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen2.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckT2Answer6()
    {
        int nbQRoom2 = qRoom2.Count;
        for (int i = 0; i < nbQRoom2; i++)
        {
            forGO = qRoom2.getAt(i);
            if (forGO.name.Contains(6.ToString()))
            {
                answer = forGO.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();

                if (answer == aq6r2) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(screen2.GetComponent<Selectable>().right);
                    timeR = Time.time;

                    forGO.SetActive(false); //hide the question
                    bool solved = true;
                    int nbARoom2 = aRoom2.Count;
                    for (int j = 0; j < nbARoom2; j++)
                    {
                        forGO2 = aRoom2.getAt(j);
                        if (forGO2.name.Contains(6.ToString()))
                        {
                            //show the solution of question 6
                            forGO2.SetActive(true);
                        }
                        else
                        {
                            //check if other question are solved
                            if (!forGO2.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved) //if all question are solved
                    {
                        screen2.GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(screen2.GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    //check if the answer given in one of the inputfields equals an answer not given yet
    private void CheckT3Answer(GameObject question)
    {
        answer = question.GetComponentInChildren<InputField>().text.ToLower();
        //if answer 1 is given and wasn't given
        if (answer == aq1r3 && !answer1R3Given)
        {
            int count = aRoom3.Count;
            for (int i = 0; i < count; i++)
            {
                forGO = aRoom3.getAt(i);
                if (forGO.name.Contains(1.ToString()))
                {
                    //find the corresponding answer UI and display it
                    forGO.SetActive(true);
                    forGO.transform.position = question.transform.position;
                    question.SetActive(false);
                    answer1R3Given = true; //set this answer to "given"
                    //feedback right answer
                    source.PlayOneShot(screen3.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    //disable inventory elements used to answer to this question
                    int nb = collectableGO.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        forGO2 = collectableGO.getAt(j);
                        if (forGO2.name.Contains("Puzzle"))
                        {
                            forGO2.SetActive(false);
                        }
                    }
                    //check if all anwer were given
                    if (answer2R3Given && answer3R3Given && answer4R3Given)
                    {
                        screen3.GetComponent<Selectable>().solved = true;
                        fadingToEndRoom = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
            }
        }
        //if answer 2 is given and wasn't given
        else if (answer == aq2r3 && !answer2R3Given)
        {
            int count = aRoom3.Count;
            for (int i = 0; i < count; i++)
            {
                forGO = aRoom3.getAt(i);
                if (forGO.name.Contains(2.ToString()))
                {
                    //find the corresponding answer UI and display it
                    forGO.SetActive(true);
                    forGO.transform.position = question.transform.position;
                    question.SetActive(false);
                    answer2R3Given = true; //set this answer to "given"
                    //feedback right answer
                    source.PlayOneShot(screen3.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    //disable inventory elements used to answer to this question
                    int nb = collectableGO.Count;
                    for (int j = 0; j < nb; j++)
                    {
                        forGO2 = collectableGO.getAt(j);
                        if (forGO2.name.Contains("Lamp"))
                        {
                            forGO2.SetActive(false);
                        }
                    }
                    //check if all anwer were given
                    if (answer1R3Given && answer3R3Given && answer4R3Given)
                    {
                        screen3.GetComponent<Selectable>().solved = true;
                        fadingToEndRoom = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
            }
        }
        //if answer 3 is given and wasn't given
        else if (answer == aq3r3 && !answer3R3Given)
        {
            int count = aRoom3.Count;
            for (int i = 0; i < count; i++)
            {
                forGO = aRoom3.getAt(i);
                if (forGO.name.Contains(3.ToString()))
                {
                    //find the corresponding answer UI and display it
                    forGO.SetActive(true);
                    forGO.transform.position = question.transform.position;
                    question.SetActive(false);
                    answer3R3Given = true; //set this answer to "given"
                    //feedback right answer
                    source.PlayOneShot(screen3.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    //check if all anwer were given
                    if (answer2R3Given && answer1R3Given && answer4R3Given)
                    {
                        screen3.GetComponent<Selectable>().solved = true;
                        fadingToEndRoom = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
            }
        }
        //if answer 4 is given and wasn't given
        else if (answer == aq4r3 && !answer4R3Given)
        {
            int count = aRoom3.Count;
            for (int i = 0; i < count; i++)
            {
                forGO = aRoom3.getAt(i);
                if (forGO.name.Contains(4.ToString()))
                {
                    //find the corresponding answer UI and display it
                    forGO.SetActive(true);
                    forGO.transform.position = question.transform.position;
                    question.SetActive(false);
                    answer4R3Given = true; //set this answer to "given"
                    //feedback right answer
                    source.PlayOneShot(screen3.GetComponent<Selectable>().right);
                    timeR = Time.time;
                    //check if all anwer were given
                    if (answer2R3Given && answer3R3Given && answer1R3Given)
                    {
                        screen3.GetComponent<Selectable>().solved = true;
                        fadingToEndRoom = true;
                        //start fading to password
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
            }
        }
        else
        {
            //feedback wrong answer
            source.PlayOneShot(screen3.GetComponent<Selectable>().wrong);
            timeW = Time.time;
        }
    }
}