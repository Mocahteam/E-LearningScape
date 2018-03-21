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
    private Family screen1 = FamilyManager.getFamily(new AnyOfTags("Screen1")); //screen of the tablet room 1 (UI)
    private Family gears = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));
    private Family rotatingGears = FamilyManager.getFamily(new AnyOfTags("RotateGear")); //gears that can rotate (middle top, middle bot, and the solution gear)
    private Family wTimerText = FamilyManager.getFamily(new AnyOfTags("WrongTimerText"));

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

    public SetAnswer()
    {
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
                go.GetComponentInChildren<Button>().onClick.AddListener(CheckAnswer1);
            }
            else if (go.name.Contains(2.ToString()))
            {
                go.GetComponentInChildren<Button>().onClick.AddListener(CheckAnswer2);
            }
            else if (go.name.Contains(3.ToString()))
            {
                go.GetComponentInChildren<Button>().onClick.AddListener(CheckAnswer3);
            }
        }

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

        foreach(Transform child in screen1.First().transform)
        {
            if(child.gameObject.name == "AnswersInput")
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
        
        //tablet room 1 animation and interaction//
        //if the tablet is "solved" but enigma04 isn't diplayed
        if (tablet.First().GetComponent<Selectable>().solved && (!enigma4.activeSelf || fadingToEnigma4))
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
                    tablet.First().GetComponent<Selectable>().solved = false;
                    fadingToEnigma4 = false;
                }
            }
        }
        //if the tablet is "solved" and enigma04 is displayed
        else if (tablet.First().GetComponent<Selectable>().solved && enigma4.activeSelf && !fadingToEnigma4)
        {
            //enigma 4 solved, open door to room 2
        }
        //if the player is playing enigma04 and didn't answer
        else if(!tablet.First().GetComponent<Selectable>().solved && enigma4.activeSelf && !wrongAnswerE04)
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
                            source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
                            timeR = Time.time;

                            rotateGear = true;  //rotate gears in the middle
                            tablet.First().GetComponent<Selectable>().solved = true; //set tablet to solved
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
                            source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
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
                    gearDragged.transform.localPosition = Input.mousePosition - Vector3.right * (float)Camera.main.pixelWidth / 2 - Vector3.up * (float)Camera.main.pixelHeight / 2;
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

    /* check the answer of the first question on the tablet
     * called when the corresponding button is clicked
     */
    private void CheckAnswer1()
    {
        foreach(GameObject q in qRoom1)
        {
            //find question 1 of room 1
            if (q.name.Contains(1.ToString()))
            {
                string answer = q.GetComponentInChildren<InputField>().text; //player's answer
                //if the answer's length is 3 and the answer contains aq1r1's numbers
                if (answer.Length == 3 && answer.Contains((aq1r1 / 100).ToString()) && answer.Contains(((aq1r1 / 10)%10).ToString()) && answer.Contains((aq1r1 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
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
                        tablet.First().GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    /* check the answer of the second question on the tablet
     * called when the corresponding button is clicked
     */
    private void CheckAnswer2()
    {
        foreach (GameObject q in qRoom1)
        {
            //find question 2 of room 1
            if (q.name.Contains(2.ToString()))
            {
                string answer = q.GetComponentInChildren<InputField>().text; //player's answer
                if (answer.Length == 3 && answer.Contains((aq2r1 / 100).ToString()) && answer.Contains(((aq2r1 / 10) % 10).ToString()) && answer.Contains((aq2r1 % 10).ToString()))
                {
                    //feedback right answer
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
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
                        tablet.First().GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    /* check the answer of the third question on the tablet
     * called when the corresponding button is clicked
     */
    private void CheckAnswer3()
    {
        foreach (GameObject q in qRoom1)
        {
            //find question 3 of room 1
            if (q.name.Contains(3.ToString()))
            {
                string answer = q.GetComponentInChildren<InputField>().text; //player's answer
                answer = answer.ToLower();  //minimize the answer
                if(answer == aq3r1) //if answer is correct
                {
                    //feedback right answer
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
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
                        tablet.First().GetComponent<Selectable>().solved = true;    //set tablet to solved
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
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }
}