using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;

public class IARQueryEvaluator : FSystem {

    // Evaluate queries answer input inside IAR input fields.
    // This system has to be set inside MainLoop before IARTabNavigation in order to stop it if text is input, specialy in case of "a" caracter is enter.

    // Contains all query
    private Family f_queries = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_queriesRoom2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"));
    private Family f_answerRoom2 = FamilyManager.getFamily(new AnyOfTags("A-R2"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not displayed for the second room
    private Family f_uiEffects = FamilyManager.getFamily(new AnyOfTags("UIEffect"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_mainloop = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));
    private Family f_berthiaumeClue = FamilyManager.getFamily(new AllOfComponents(typeof(BerthiaumeClueSeen)));
    private Family f_toggles = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle)));
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
        }
    }

    private void CheckAnswer(GameObject query)
    {
        string answer = query.GetComponentInChildren<InputField>().text; //player's answer
        // format answer
        answer = answer.Replace('é', 'e');
        answer = answer.Replace('è', 'e');
        answer = answer.ToUpper();
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
                if (availableOrSolutions.Contains(qs.orSolutions[i]) && answer.Contains(qs.orSolutions[i]))
                {
                    error = false;
                    availableOrSolutions.Remove(qs.orSolutions[i]); // consume this "or" solution
                }
            }
        }
        if (error)
        {
            // notify player error
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 1 });

            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "Wrong", performedBy = "player" });
        }
        else
        {
            // notify player success
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 2 });

            if(query.tag == "Q-R3")
            {
                if(answer == "PLANIFICATION")
                {
                    int nbToggle = f_toggles.Count;
                    for(int i = 0; i < nbToggle; i++)
                    {
                        if(f_toggles.getAt(i).name == "TogglePuzzle")
                        {
                            if (f_toggles.getAt(i).GetComponent<Toggle>().isOn)
                                GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = answer.ToLower(), performedBy = "player", orLabels = new string[] { "l17" } });
                            else
                                GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = answer.ToLower(), performedBy = "player", orLabels = new string[] { "l16" } });
                            break;
                        }
                    }
                }
                else
                    GameObjectManager.addComponent<ActionPerformed>(query.transform.parent.gameObject, new { overrideName = answer.ToLower(), performedBy = "player" });
            }
            else
                GameObjectManager.addComponent<ActionPerformed>(query, new { name = "Correct", performedBy = "player" });

            GameObjectManager.addComponent<ActionPerformed>(query, new { name = "perform", performedBy = "system" });

            // Toggle UI element (hide input text and button and show answer)
            for (int i = 1; i < query.transform.childCount; i++)
            {
                GameObject child = query.transform.GetChild(i).gameObject;
                GameObjectManager.setGameObjectState(child, !child.activeSelf);

            }
            // set final answer for third room
            if (query.tag == "Q-R3")
                query.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = answer;
            // if linked hide item in inventory
            foreach (LinkedWith item in query.GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(item.link, false);

        }
    }
}