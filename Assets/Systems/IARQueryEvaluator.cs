using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class IARQueryEvaluator : FSystem {

    // Evaluate queries answer input inside IAR input fields.
    // This system has to be set inside MainLoop before IARTabNavigation in order to stop it if text is input, specialy in case of "a" caracter is enter.

    // Contains all query
    private Family f_queries = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_answerRoom2 = FamilyManager.getFamily(new AnyOfTags("A-R2"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not displayed for the second room
    private Family f_queriesRoom3 = FamilyManager.getFamily(new AnyOfTags("Q-R3"));
    private Family f_uiEffects = FamilyManager.getFamily(new AnyOfTags("UIEffect"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_selectedTab = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedTab)));

    private Family f_terminalScreens = FamilyManager.getFamily(new AnyOfTags("TerminalScreen"));

    public static IARQueryEvaluator instance;

    public GameObject queriesRoom2;
    public UnlockedRoom unlockedRoom;

    private HashSet<string> availableOrSolutions;
    private Dictionary<string, List<string>> splitOrSolutions;
    private Dictionary<string, KeyValuePair<string, string>> room3AnswerFeedbacks;
    private bool showRoom2FinalCode = false;

    private string tmpStr;
    private List<string> tmpListString;

    public IARQueryEvaluator()
    {
        instance = this;
    }

    protected override void onStart()
    {
        // Init callbacks and/or solutions
        availableOrSolutions = new HashSet<string>();
        splitOrSolutions = new Dictionary<string, List<string>>();
        foreach (GameObject query in f_queries)
            foreach (string or in query.GetComponent<QuerySolution>().orSolutions)
            {
                availableOrSolutions.Add(or);
                if (!splitOrSolutions.ContainsKey(or))
                {
                    //split each solution by "##" and use this to check if the player answer contains all parts of a solution
                    tmpListString = new List<string>(or.Split(new string[] { "##" }, System.StringSplitOptions.None));
                    for (int i = 0; i < tmpListString.Count; i++)
                        tmpListString[i] = LoadGameContent.StringToAnswer(tmpListString[i]);
                    splitOrSolutions.Add(or, tmpListString);
                }
            }

        // Init feedbacks for room 3
        if (!SceneManager.GetActiveScene().name.Contains("Tuto"))
        {
            room3AnswerFeedbacks = new Dictionary<string, KeyValuePair<string, string>>();
            tmpStr = LoadGameContent.StringToAnswer(LoadGameContent.gameContent.puzzleAnswer);
            room3AnswerFeedbacks.Add(tmpStr, new KeyValuePair<string, string>(LoadGameContent.gameContent.puzzleAnswerFeedback, LoadGameContent.gameContent.puzzleAnswerFeedbackDesc));
            tmpStr = LoadGameContent.StringToAnswer(LoadGameContent.gameContent.enigma16Answer);
            if (!room3AnswerFeedbacks.ContainsKey(tmpStr))
                room3AnswerFeedbacks.Add(tmpStr, new KeyValuePair<string, string>(LoadGameContent.gameContent.enigma16AnswerFeedback, LoadGameContent.gameContent.enigma16AnswerFeedbackDesc));
            tmpStr = LoadGameContent.StringToAnswer(LoadGameContent.gameContent.lampAnswer);
            if (!room3AnswerFeedbacks.ContainsKey(tmpStr))
                room3AnswerFeedbacks.Add(tmpStr, new KeyValuePair<string, string>(LoadGameContent.gameContent.lampAnswerFeedback, LoadGameContent.gameContent.lampAnswerFeedbackDesc));
            tmpStr = LoadGameContent.StringToAnswer(LoadGameContent.gameContent.whiteBoardAnswer);
            if (!room3AnswerFeedbacks.ContainsKey(tmpStr))
                room3AnswerFeedbacks.Add(tmpStr, new KeyValuePair<string, string>(LoadGameContent.gameContent.whiteBoardAnswerFeedback, LoadGameContent.gameContent.whiteBoardAnswerFeedbackDesc));

            f_answerRoom2.addExitCallback(onNewAnswerDisplayed);
        }
        f_uiEffects.addEntryCallback(onUIEffectEnd);
    }

    private void onNewAnswerDisplayed(int instanceId)
    {
        // When all answer was displayed => ask to display final code for second room
        if (f_answerRoom2.Count == 0)
            showRoom2FinalCode = true;
    }

    private void onUIEffectEnd (GameObject go)
    {
        if (showRoom2FinalCode)
        {
            showRoom2FinalCode = false;
            showR2FinalCode();
        }
    }

    public void showR2FinalCode()
    {
        // disable queries
        GameObjectManager.setGameObjectState(queriesRoom2.transform.GetChild(0).gameObject, false);
        // enable final code
        GameObjectManager.setGameObjectState(queriesRoom2.transform.GetChild(1).gameObject, true);
    }

    public void IarOnEndEditAnswer(GameObject query)
    {
        if (Input.GetButtonDown("Submit"))
            IarCheckAnswer(query);
        if (Input.GetButtonDown("Cancel"))
            IARTabNavigation.instance.skipNextClose = true;
    }

    public void IarCheckAnswer(GameObject query)
    {
        string answer = query.GetComponentInChildren<TMP_InputField>().text; //player's answer
        string solution = "";
        // format answer
        answer = LoadGameContent.StringToAnswer(answer);
        // get query
        QuerySolution qs = query.GetComponent<QuerySolution>();
        // Check mandatory solution
        bool error = false;
        // if andSolution available
        if (qs.andSolutions.Count > 0)
        {
            int andLength = 0;
            for (int i = 0; i < qs.andSolutions.Count && !error; i++)
            {
                //split each part of the solution by "##" and use this to check if the player answer contains one of this part of a solution
                tmpListString = new List<string>(qs.andSolutions[i].Split(new string[] { "##" }, System.StringSplitOptions.None));
                bool found = false;
                for (int j = 0; j < tmpListString.Count && !found; j++)
                    if (answer.Contains(tmpListString[j]))
                    {
                        found = true;
                        andLength += tmpListString[j].Length;
                    }
                error = !found;
            }
            // check answer is the same size of andSolutions
            error = error || answer.Length != andLength;
        }
        // if orSolutions available
        else if (qs.orSolutions.Count > 0)
        {
            error = true;
            for (int i = 0; i < qs.orSolutions.Count && error; i++)
            {
                bool containsSolution = false;
                //check if the given answer contains one of the solution parts
                foreach (string s in splitOrSolutions[qs.orSolutions[i]])
                {
                    if (answer == s)
                    {
                        containsSolution = true;
                        break;
                    }
                }
                // if answer includes this solution and this solution is still available
                if (containsSolution && availableOrSolutions.Contains(qs.orSolutions[i]))
                {
                    error = false;
                    availableOrSolutions.Remove(qs.orSolutions[i]); // consume this "or" solution
                    // override answer by the solution
                    solution = qs.orSolutions[i];
                }
            }
        }

        GameObjectManager.addComponent<ActionPerformedForLRS>(query, new
        {
            verb = "answered",
            objectType = "question",
            activityExtensions = new Dictionary<string, string>() {
                { "value", query.name }
            },
            result = true,
            success = error ? -1 : 1,
            response = answer
        });
        answer = solution != "" ? solution : answer;

        if (error)
        {
            // notify player error
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 1 });
            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "Wrong", performedBy = "player" });

            GameObjectManager.addComponent<WrongAnswerInfo>(query, new { givenAnswer = answer });

            // set focus on selected tab
            EventSystem.current.SetSelectedGameObject(null);
            if (f_selectedTab.Count > 0)
                EventSystem.current.SetSelectedGameObject(f_selectedTab.getAt(0));
        }
        else
        {
            // notify player success
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 2 });

            // set focus on selected tab
            EventSystem.current.SetSelectedGameObject(null);
            if (f_selectedTab.Count > 0)
                EventSystem.current.SetSelectedGameObject(f_selectedTab.getAt(0));

            // set final answer for third room (due to OR options)
            if (query.tag == "Q-R3")
            {
                SetRoom3AnswerFeedback(solution, query);

                // Prepare correct trace
                string context = "";
                if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.puzzleAnswer))
                    context = LoadGameContent.internalGameContent.virtualPuzzle ? "VirtualPuzzle" : "PhysicalPuzzle";
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.lampAnswer))
                    context = "Lamp";
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.enigma16Answer))
                    context = "Enigma16";
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.whiteBoardAnswer))
                    context = "WhiteBoard";

                // mark associated enigma ready to solve
                GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "ReadyToSolve_" + context, performedBy = "player" });
                // propagate information inside Meta (special case for Puzzle, we have to remove Virtual/Physical to avoid to manage or links)
                GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "ReadyToSolve_" + (context.Contains("Puzzle") ? "Puzzle" : context) + "_Meta", performedBy = "system" });
                // validate associated enigma to this query and disable associated enigma to other queries
                foreach (GameObject go in f_queriesRoom3)
                    if (go == query)
                        GameObjectManager.addComponent<ActionPerformed>(go, new { overrideName = (context.Contains("Puzzle") ? "Puzzle" : context) + "_Solved", performedBy = "player" });
                    else
                        GameObjectManager.addComponent<ActionPerformed>(go, new { overrideName = (context.Contains("Puzzle") ? "Puzzle" : context) + "_Locked", performedBy = "player" });
            }

            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "Correct", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "perform", performedBy = "system" }); // meta

            // Toggle UI element (hide input text and button and show answer)
            for (int i = 1; i < query.transform.childCount; i++)
            {
                GameObject child = query.transform.GetChild(i).gameObject;
                GameObjectManager.setGameObjectState(child, !child.activeSelf);
                // Trace to LRS displayed immediate feedback
                if (i == 3)
                {
                    string feedbackTexts = "";
                    foreach (Transform grandSon in child.transform)
                    {
                        TextMeshProUGUI tmp = grandSon.gameObject.GetComponent<TextMeshProUGUI>();
                        if (tmp && tmp.text != "")
                            feedbackTexts += grandSon.gameObject.GetComponent<TextMeshProUGUI>().text+" ";
                    }
                    if (feedbackTexts != "")
                    {
                        GameObjectManager.addComponent<ActionPerformedForLRS>(query, new
                        {
                            verb = "received",
                            objectType = "feedback",
                            activityExtensions = new Dictionary<string, string>() {
                                { "value", query.name },
                                { "content", feedbackTexts }
                            }
                        });
                    }
                }
            }

            // put a screenshot of the IAR on the terminal of the last unlocked room
            if (f_terminalScreens.Count >= unlockedRoom.roomNumber)
                MainLoop.instance.StartCoroutine(SetTerminalScreen());

            // if linked hide item in inventory
            foreach (LinkedWith item in query.GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(item.link, false);
        }
    }

    /// <summary>
    /// Checks depending on the given solution which feedback should be used and sets it in the given query.
    /// </summary>
    /// <param name="solution">The solution corresponding to the answer given by the player</param>
    /// <param name="query">The query in which the player answered</param>
    private void SetRoom3AnswerFeedback(string solution, GameObject query)
    {
        string feedback = "";
        string description = "";

        if (room3AnswerFeedbacks.ContainsKey(solution))
        {
            feedback = room3AnswerFeedbacks[solution].Key;
            description = room3AnswerFeedbacks[solution].Value;
        }

        if(feedback != "" && query)
        {
            try
            {
                query.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = feedback;
                query.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Couldn't set the feedback of the answer \"" + feedback + "\" because of an element missing.");
            }
        }
        else
            Debug.LogWarning("Couldn't set the feedback of the answer because of invalid solution or query.");
    }

    public IEnumerator SetTerminalScreen()
    {
        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, Camera.main.pixelWidth, Camera.main.pixelHeight), 0, 0);
        tex.Apply(false);
        foreach(GameObject screen in f_terminalScreens)
        {
            screen.GetComponent<Renderer>().material.mainTexture = tex;
            screen.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}