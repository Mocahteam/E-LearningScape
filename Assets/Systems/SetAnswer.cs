using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using TMPro;

public class SetAnswer : FSystem {

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family objects = FamilyManager.getFamily(new AnyOfTags("Object"), new AllOfComponents(typeof(Selectable)));
    private Family answers = FamilyManager.getFamily(new AnyOfTags("Answer"), new AllOfComponents(typeof(Button)));
    private Family displayAnswer = FamilyManager.getFamily(new AnyOfTags("Answer"), new NoneOfComponents(typeof(Button)));
    private Family audioSource = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)));
    private Family images = FamilyManager.getFamily(new AllOfComponents(typeof(Image)));
    private Family qRoom1 = FamilyManager.getFamily(new AnyOfTags("Q-R1"));
    private Family tablet = FamilyManager.getFamily(new AnyOfTags("Tablet"));
    private Family aRoom1 = FamilyManager.getFamily(new AnyOfTags("A-R1"));
    private Family screen1 = FamilyManager.getFamily(new AnyOfTags("Screen1"));
    private Family gears = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));
    private Family rotatingGears = FamilyManager.getFamily(new AnyOfTags("RotateGear"));
    private Family wTimerText = FamilyManager.getFamily(new AnyOfTags("WrongTimerText"));

    private bool initialized = false;
    private string vrai = "VRAI";
    private GameObject rightBG;
    private float timeR = 0;
    private string faux = "FAUX";
    private GameObject wrongBG;
    private float timeW = 0;
    private AudioSource source;
    private int aq1r1 = 128;//answer question 1 room 1
    private int aq2r1 = 459;
    private string aq3r1 = "il faut savoir changer de posture";
    private GameObject answersRoom1;
    private GameObject enigma4;
    private GameObject whiteBG;
    private float timerWhite = Mathf.Infinity;
    private bool fadingToEnigma4 = false;
    private GameObject gearDragged = null;
    private bool rotateGear = false;
    private float wTimerE04 = Mathf.Infinity;
    private TextMeshProUGUI wtt;
    private bool wrongAnswerE04 = false;

    public SetAnswer()
    {
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
        foreach(GameObject g in audioSource)
        {
            if(g.name == "Game")
            {
                source = g.GetComponent<AudioSource>();
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
        foreach(GameObject go in gears)
        {
            go.GetComponent<Gear>().initialPosition = go.transform.localPosition;
        }
        wtt = wTimerText.First().GetComponent<TextMeshProUGUI>();
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

        if (tablet.First().GetComponent<Selectable>().solved && (!enigma4.activeSelf || fadingToEnigma4))
        {
            if (rightBG.activeSelf)
            {
                timerWhite = Time.time;
            }
            else
            {
                if (Time.time - timerWhite < 1.5f && Time.time - timerWhite >= 0f)
                {
                    fadingToEnigma4 = true;
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, (Time.time - timerWhite) / 1.5f);
                }
                else if (Time.time - timerWhite < 3f && Time.time - timerWhite > 1.5f)
                {
                    answersRoom1.SetActive(false);
                    enigma4.SetActive(true);
                    whiteBG.GetComponent<Image>().color = new Color(whiteBG.GetComponent<Image>().color.r, whiteBG.GetComponent<Image>().color.g, whiteBG.GetComponent<Image>().color.b, 1 - (Time.time - timerWhite - 1.5f) / 1.5f);
                }
                else
                {
                    whiteBG.SetActive(false);
                    tablet.First().GetComponent<Selectable>().solved = false;
                    fadingToEnigma4 = false;
                }
            }
        }
        else if (tablet.First().GetComponent<Selectable>().solved && enigma4.activeSelf && !fadingToEnigma4)
        {
            //enigma 4 solved
        }
        else if(!tablet.First().GetComponent<Selectable>().solved && enigma4.activeSelf && !wrongAnswerE04)
        {
            foreach(GameObject gear in gears)
            {
                if (gear.GetComponent<PointerOver>())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        gearDragged = gear;
                    }
                }
            }
            if(gearDragged != null)
            {
                rotateGear = false;
                if (Input.GetMouseButtonUp(0))
                {
                    if(gearDragged.transform.localPosition.x<125 && gearDragged.transform.localPosition.x > -125 && gearDragged.transform.localPosition.y < 125f/2 && gearDragged.transform.localPosition.x > -125f / 2)
                    {
                        gearDragged.transform.localPosition = Vector3.zero;
                        if (gearDragged.GetComponent<Gear>().isSolution)
                        {
                            source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
                            timeR = Time.time;
                            rotateGear = true;
                            tablet.First().GetComponent<Selectable>().solved = true;
                            foreach(GameObject q in qRoom1)
                            {
                                if (q.name.Contains(4.ToString()))
                                {
                                    q.SetActive(false);
                                }
                            }
                        }
                        else
                        {
                            source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                            timeW = Time.time;
                            foreach (GameObject q in qRoom1)
                            {
                                if (q.name.Contains(4.ToString()))
                                {
                                    q.SetActive(true);
                                }
                            }
                            wTimerE04 = Time.time;
                            wtt.gameObject.SetActive(true);
                            wtt.text = (15 + wTimerE04 - Time.time).ToString("n2");
                            wrongAnswerE04 = true;
                            gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition;
                        }
                        gearDragged = null;
                    }
                    else
                    {
                        gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition;
                        foreach (GameObject q in qRoom1)
                        {
                            if (q.name.Contains(4.ToString()))
                            {
                                q.SetActive(true);
                            }
                        }
                    }
                    gearDragged = null;
                }
                else
                {
                    gearDragged.transform.localPosition = Input.mousePosition - Vector3.right * (float)Camera.main.pixelWidth / 2 - Vector3.up * (float)Camera.main.pixelHeight / 2;
                }
            }
        }
        else if (wrongAnswerE04)
        {
            if(15 + wTimerE04 - Time.time < 0)
            {
                wrongAnswerE04 = false;
                wtt.gameObject.SetActive(false);
            }
            else
            {
                wtt.text = (15 + wTimerE04 - Time.time).ToString("n2");
            }
        }
        if (rotateGear)
        {
            foreach(GameObject g in rotatingGears)
            {
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
                            if(go.tag == "Object")
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

    private void CheckAnswer1()
    {
        foreach(GameObject q in qRoom1)
        {
            if (q.name.Contains(1.ToString()))
            {
                string answer = q.GetComponentInChildren<InputField>().text;
                if (answer.Length == 3 && answer.Contains((aq1r1 / 100).ToString()) && answer.Contains(((aq1r1 / 10)%10).ToString()) && answer.Contains((aq1r1 % 10).ToString()))
                {
                    //right
                    bool solved = true;
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
                    timeR = Time.time;
                    q.SetActive(false);
                    foreach(GameObject a in aRoom1)
                    {
                        if (a.name.Contains(1.ToString()))
                        {
                            a.SetActive(true);
                        }
                        else
                        {
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved)
                    {
                        tablet.First().GetComponent<Selectable>().solved = true;
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //wrong
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckAnswer2()
    {
        foreach (GameObject q in qRoom1)
        {
            if (q.name.Contains(2.ToString()))
            {
                string answer = q.GetComponentInChildren<InputField>().text;
                if (answer.Length == 3 && answer.Contains((aq2r1 / 100).ToString()) && answer.Contains(((aq2r1 / 10) % 10).ToString()) && answer.Contains((aq2r1 % 10).ToString()))
                {
                    //right
                    bool solved = true;
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
                    timeR = Time.time;
                    q.SetActive(false);
                    foreach (GameObject a in aRoom1)
                    {
                        if (a.name.Contains(2.ToString()))
                        {
                            a.SetActive(true);
                        }
                        else
                        {
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved)
                    {
                        tablet.First().GetComponent<Selectable>().solved = true;
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //wrong
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }

    private void CheckAnswer3()
    {
        foreach (GameObject q in qRoom1)
        {
            if (q.name.Contains(3.ToString()))
            {
                string answer = q.GetComponentInChildren<InputField>().text;
                answer = answer.ToLower();
                if(answer == aq3r1)
                {
                    //right
                    bool solved = true;
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().right);
                    timeR = Time.time;
                    q.SetActive(false);
                    foreach (GameObject a in aRoom1)
                    {
                        if (a.name.Contains(3.ToString()))
                        {
                            a.SetActive(true);
                        }
                        else
                        {
                            if (!a.activeSelf)
                            {
                                solved = false;
                            }
                        }
                    }
                    if (solved)
                    {
                        tablet.First().GetComponent<Selectable>().solved = true;
                        timerWhite = Time.time;
                        whiteBG.SetActive(true);
                        Color c = whiteBG.GetComponent<Image>().color;
                        whiteBG.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                    }
                }
                else
                {
                    //wrong
                    source.PlayOneShot(tablet.First().GetComponent<Selectable>().wrong);
                    timeW = Time.time;
                }
            }
        }
    }
}