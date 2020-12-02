using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using FYFY_plugins.Monitoring;

public class IARQueryEvaluator : FSystem {

    // Evaluate queries answer input inside IAR input fields.
    // This system has to be set inside MainLoop before IARTabNavigation in order to stop it if text is input, specialy in case of "a" caracter is enter.

    // Contains all query
    private Family f_queries = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_queriesRoom2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"));
    private Family f_answerRoom2 = FamilyManager.getFamily(new AnyOfTags("A-R2"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not displayed for the second room
    private Family f_queriesRoom3 = FamilyManager.getFamily(new AnyOfTags("Q-R3"));
    private Family f_uiEffects = FamilyManager.getFamily(new AnyOfTags("UIEffect"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));
    private Family f_selectedTab = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedTab)));

    private Family f_terminalScreens = FamilyManager.getFamily(new AnyOfTags("TerminalScreen"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    public static IARQueryEvaluator instance;

    private HashSet<string> availableOrSolutions;
    private bool showRoom2FinalCode = false;

    public IARQueryEvaluator()
    {
        if (Application.isPlaying)
        {
            // Init callbacks and/or solutions
            availableOrSolutions = new HashSet<string>();
            foreach (GameObject query in f_queries)
                foreach (string or in query.GetComponent<QuerySolution>().orSolutions)
                    availableOrSolutions.Add(or);

            f_answerRoom2.addExitCallback(onNewAnswerDisplayed);
            f_uiEffects.addEntryCallback(onUIEffectEnd);
        }
        instance = this;
    }

    // return true if UI with name "name" is selected into inventory
    private GameObject isSelected(string name)
    {
        foreach (GameObject go in f_itemSelected)
            if (go.name == name)
                return go;
        return null;
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
            GameObject queries = f_queriesRoom2.First().transform.parent.gameObject;
            // disable queries
            GameObjectManager.setGameObjectState(queries, false);
            // enable final code
            GameObjectManager.setGameObjectState(queries.transform.parent.GetChild(1).gameObject, true);

            GameObjectManager.addComponent<ActionPerformedForLRS>(queries.transform.parent.gameObject, new
            {
                verb = "completed",
                objectType = "menu",
                objectName = queries.transform.parent.gameObject.name
            });
            GameObjectManager.addComponent<ActionPerformedForLRS>(queries.transform.parent.GetChild(1).gameObject, new
            {
                verb = "accessed",
                objectType = "viewable",
                objectName = "Password_Room2",
                activityExtensions = new Dictionary<string, List<string>>() { { "value", new List<string>() { queries.transform.parent.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text } } }
            });
        }
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
        string answer = query.GetComponentInChildren<InputField>().text; //player's answer
        // format answer
        answer = LoadGameContent.StringToAnswer(answer);
        // get query
        QuerySolution qs = query.GetComponent<QuerySolution>();
        // Check mandatory solution
        bool error = false;
        int andLength = 0;
        for (int i = 0 ; i < qs.andSolutions.Count && !error ; i++)
        {
            andLength += qs.andSolutions[i].Length;
            if (!answer.Contains(qs.andSolutions[i]))
                error = true;
        }
        // if no error and no orSolutions available, check if answer is the same size of andSolutions
        if (!error && qs.orSolutions.Count == 0)
            error = answer.Length != andLength;
        // if no error and orSolutions available
        if (!error && qs.orSolutions.Count > 0)
        {
            error = true;
            for (int i = 0; i < qs.orSolutions.Count && error; i++)
            {
                // if answer includes this solution and this solution is still available
                if (answer.Contains(qs.orSolutions[i]) && availableOrSolutions.Contains(qs.orSolutions[i]))
                {
                    error = false;
                    availableOrSolutions.Remove(qs.orSolutions[i]); // consume this "or" solution
                    // override answer by the solution
                    answer = qs.orSolutions[i];
                }
            }
        }

        GameObjectManager.addComponent<ActionPerformedForLRS>(query, new
        {
            verb = "answered",
            objectType = "question",
            objectName = string.Concat(query.name, "-", query.tag),
            result = true,
            success = error ? -1 : 1,
            response = answer
        });

        if (error)
        {
            // notify player error
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 1 });
            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "Wrong", performedBy = "player" });

            GameObjectManager.addComponent<WrongAnswerInfo>(query, new { givenAnswer = answer });
        }
        else
        {
            // notify player success
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 2 });

            // set focus on selected tab
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(f_selectedTab.getAt(0));

            // set final answer for third room (due to OR options)
            if (query.tag == "Q-R3")
            {
                query.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = answer;

                // Prepare correct trace
                string context = "";
                if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.puzzleAnswer))
                    context = LoadGameContent.gameContent.virtualPuzzle ? "VirtualPuzzle" : "PhysicalPuzzle";
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.lampAnswer))
                    context = "Lamp";
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.enigma16Answer))
                    context = "Enigma16";
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.whiteBoardAnswer))
                    context = "WhiteBoard";
                // mark associated enigma ready to solve
                GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "ReadyToSolve_"+context, performedBy = "player" });
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
                    List<string> feedbackTexts = new List<string>();
                    foreach (Transform grandSon in child.transform)
                    {
                        TextMeshProUGUI tmp = grandSon.gameObject.GetComponent<TextMeshProUGUI>();
                        if (tmp && tmp.text != "")
                            feedbackTexts.Add(grandSon.gameObject.GetComponent<TextMeshProUGUI>().text);
                    }
                    if (feedbackTexts.Count > 0)
                    {
                        GameObjectManager.addComponent<ActionPerformedForLRS>(query, new
                        {
                            verb = "received",
                            objectType = "feedback",
                            objectName = string.Concat(query.name, "-", query.tag, "_feedback"),
                            activityExtensions = new Dictionary<string, List<string>>() {
                            { "content", feedbackTexts },
                            { "type", new List<string>() { "answer description" } }
                        }
                        });
                    }
                }
            }

            // put a screenshot of the IAR on the terminal of the last unlocked room
            int lastUnlockedRoom = f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber;
            if (f_terminalScreens.Count >= lastUnlockedRoom)
                MainLoop.instance.StartCoroutine(SetTerminalScreen(lastUnlockedRoom - 1));

            // if linked hide item in inventory
            foreach (LinkedWith item in query.GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(item.link, false);

        }
    }

    public IEnumerator SetTerminalScreen(int screenID)
    {
        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, Camera.main.pixelWidth, Camera.main.pixelHeight), 0, 0);
        tex.Apply(false);
        f_terminalScreens.getAt(screenID).GetComponent<Renderer>().material.mainTexture = tex;
    }
}