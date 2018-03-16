using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class SetAnswerOld : FSystem { //set answer for first prototype (not used anymore)

    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family objects = FamilyManager.getFamily(new AnyOfTags("Object"), new AllOfComponents(typeof(Selectable)));
    private Family answers = FamilyManager.getFamily(new AnyOfTags("Answer"), new AllOfComponents(typeof(Button)));
    private Family displayAnswer = FamilyManager.getFamily(new AnyOfTags("Answer"), new NoneOfComponents(typeof(Button)));
    private Family audioSource = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)));
    private Family images = FamilyManager.getFamily(new AllOfComponents(typeof(Image)));

    private bool initialized = false;
    private string vrai = "VRAI";
    private GameObject rightBG;
    private float timeR = 0;
    private string faux = "FAUX";
    private GameObject wrongtBG;
    private float timeW = 0;
    private AudioSource source;

    public SetAnswerOld()
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
                wrongtBG = go;
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
            wrongtBG.SetActive(true);
        }
        else
        {
            wrongtBG.SetActive(false);
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
}