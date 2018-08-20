using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;

public class IARQueryEvaluator : FSystem {

    // Evaluate queries inside IAR

    // Contains all query
    private Family f_queries = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    public static IARQueryEvaluator instance;

    private HashSet<string> availableOrSolutions;

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
                query.GetComponentInChildren<InputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        CheckAnswer(query);
                    }
                });
                foreach (string or in query.GetComponent<QuerySolution>().orSolutions)
                    availableOrSolutions.Add(or);
            }
        }
        instance = this;
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
        }
        else
        {
            // notify player success
            GameObjectManager.addComponent<PlayUIEffect>(query, new { effectCode = 3 });
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