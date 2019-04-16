using UnityEngine;
using UnityEngine.UI;
using FYFY;
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
    private Family f_uiEffects = FamilyManager.getFamily(new AnyOfTags("UIEffect"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));

    public static IARQueryEvaluator instance;

    private HashSet<string> availableOrSolutions;
    private bool showRoom2FinalCode = false;

    public IARQueryEvaluator()
    {
        if (Application.isPlaying)
        {
            // Init callbacks and or solutions
            availableOrSolutions = new HashSet<string>();
            foreach (GameObject query in f_queries)
            {
                query.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                    CheckAnswer(query);
                });
                query.GetComponentInChildren<InputField>().onValidateInput += delegate (string input, int charIndex, char addedChar) {
                    IARTabNavigation.instance.Pause = true;
                    return addedChar;
                };
                query.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        CheckAnswer(query);
                    IARTabNavigation.instance.Pause = false;
                });
                foreach (string or in query.GetComponent<QuerySolution>().orSolutions)
                    availableOrSolutions.Add(or);
            }

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

    private void CheckAnswer(GameObject query)
    {
        string answer = query.GetComponentInChildren<InputField>().text; //player's answer
        // format answer
        answer = LoadGameContent.StringToAnswer(answer);
        // get query
        QuerySolution qs = query.GetComponent<QuerySolution>();
        // Check mandatory solution
        bool error = false;
        for (int i = 0 ; i < qs.andSolutions.Count && !error ; i++)
        {
            if (!answer.Contains(qs.andSolutions[i]))
                error = true;
        }
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

            Dans le système qui gère les WrongAnswerInfo prendre en compte le cas des réponses où l'ordre n'importe pas
            GameObjectManager.addComponent<WrongAnswerInfo>(query, new { givenAnswer = answer });

            Normalement ici je ne devrais à faire que 
                1 Action perform sur la question avec l'action Wrong => le ActionPerformed requiert un ComponentMonitoring ce qui n'est pas le cas pour les dernières questions

                Transformer les réseaux de Petri
                    Pour chaque enigme faire un rx dont l'action finale sera de dire si l'énigme a été résolue => permet de connaitre les actions à faire pour terminer cette énigme
                    Faire un Rdp pour chaque question => chacun contient l'action Wrong et le succès est connecté à une place qui est alimentée par quatres transitions qui modélisent si la bonne réponse d'une question a été trouvée. Si le Wrong est tiré on peut ainsi savoir quelle énigme il reste à résoudre. Si le joueur répond correctement à une question il faut donc exécuté la validation de l'énigme dans le réseau propre à chaque énigme et aussi valider l'execution de l'énigme dans chacun des Rdp des énigmes...


            if (query.tag == "Q-R3")
            {
                if (availableOrSolutions.Contains(LoadGameContent.StringToAnswer(LoadGameContent.gameContent.puzzleAnswer)))
                {
                    if (LoadGameContent.gameContent.virtualPuzzle)
                    {
                        GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Wrong11_1", performedBy = "player" });
                        GameObjectManager.addComponent<WrongAnswerInfo>(query, new { componentMonitoringID = 101, givenAnswer = answer });
                    }
                    else
                    {
                        GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Wrong11_2", performedBy = "player" });
                        GameObjectManager.addComponent<WrongAnswerInfo>(query, new { componentMonitoringID = 110, givenAnswer = answer });
                    }
                }
                if (availableOrSolutions.Contains(LoadGameContent.StringToAnswer(LoadGameContent.gameContent.lampAnswer)))
                {
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Wrong12", performedBy = "player" });
                    GameObjectManager.addComponent<WrongAnswerInfo>(query, new { componentMonitoringID = 122, givenAnswer = answer });
                }
                if (availableOrSolutions.Contains(LoadGameContent.StringToAnswer(LoadGameContent.gameContent.enigma12Answer)))
                {
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Wrong13", performedBy = "player" });
                    GameObjectManager.addComponent<WrongAnswerInfo>(query, new { componentMonitoringID = 126, givenAnswer = answer });
                }
                if (availableOrSolutions.Contains(LoadGameContent.StringToAnswer(LoadGameContent.gameContent.whiteBoardAnswer)))
                {
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Wrong14", performedBy = "player" });
                    GameObjectManager.addComponent<WrongAnswerInfo>(query, new { componentMonitoringID = 142, givenAnswer = answer });
                }
            }
        }
        else
        {
            // notify player success
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 2 });

            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "Correct", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "perform", performedBy = "system" }); // meta

            if (query.tag == "Q-R3")
            {
                if(answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.puzzleAnswer))
                {
                    if (LoadGameContent.gameContent.virtualPuzzle)
                        GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Correct11_1", performedBy = "player" });
                    else
                        GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Correct11_2", performedBy = "player" });
                }
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.lampAnswer))
                {
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Correct12", performedBy = "player" });
                }
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.enigma12Answer))
                {
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Correct13", performedBy = "player" });
                }
                else if (answer == LoadGameContent.StringToAnswer(LoadGameContent.gameContent.whiteBoardAnswer))
                {
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = "Correct14", performedBy = "player" });
                }
            }

            // set final answer for third room (due to OR options)
            if (query.tag == "Q-R3")
                query.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = answer;

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

            // if linked hide item in inventory
            foreach (LinkedWith item in query.GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(item.link, false);

        }
    }
}