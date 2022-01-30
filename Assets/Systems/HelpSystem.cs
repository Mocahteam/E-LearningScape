using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.Monitoring;
using System;
using System.Collections.Generic;
using TMPro;
using System.Threading;

public class HelpSystem : FSystem {

    private Family f_traces = FamilyManager.getFamily(new AllOfComponents(typeof(Trace)));
    private Family f_componentMonitoring = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));
    private Family f_wrongAnswerInfo = FamilyManager.getFamily(new AllOfComponents(typeof(WrongAnswerInfo), typeof(ComponentMonitoring)));

    private Family f_puzzles = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new NoneOfComponents(typeof(DreamFragment)), new AllOfComponents(typeof(ComponentMonitoring)));
    private Family f_puzzlesFragment = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new AllOfComponents(typeof(DreamFragment), typeof(ComponentMonitoring)));

    /// <summary>
    /// Contains hints filled with tips loaded from "Hints_LearningScape.txt"
    /// </summary>
    public GameHints gameHints;

    public InternalGameHints internalGameHints;
    public GameObject askHelpButton;
    /// <summary>
    /// Contains weights for each Laalys labels used to increase/decrease labelCount
    /// </summary>
    public LabelWeights labelWeights;
    public GameObject helpTab; 
    public Timer timer;

    public GameObject scrollView;

    /// <summary>
    /// Key: ComponentMonitoring id of enigmas inside meta Petri net
    /// Value: id of sub Petri net that details the resolution process of enigma
    /// </summary>
    private Dictionary<int, int> EnigmaIdToPnId;

    /// <summary>
    /// Associate for each Petri net name the number of remaining action to reach a player objective in this Petri net
    /// </summary>
    private Dictionary<string, int> pnNetsRemainingSteps;
    /// <summary>
    /// Associate for each Petri net name the number of action to carry out at the beginning of the game to reach a player objective in this Petri net
    /// </summary>
    public Dictionary<string, int> pnNetsRequiredStepsOnStart;
    /// <summary>
    /// A Thread to compute remaining steps for each Petri net
    /// </summary>
    private static Thread thread = null;
    private static Mutex mut = new Mutex();


    /// <summary>
    /// a stack to store the name of Petri nets for which we have to clean hints
    /// </summary>
    private Stack<string> cleanHintsByPn;

    /// <summary>
    /// sum of actions/enigmas in meta Petri net multiplied by their wheight
    /// </summary>
    private float totalWeightedMetaActions = 0;
    /// <summary>
    /// used to count the time spent since the player last asked a hint
    /// </summary>
    private float playerHintTimer = float.MinValue;
    /// <summary>
    /// GameObject components used to show to the player the time left before being able to ask help again
    /// </summary>
    private RectTransform cooldownRT;
    private TextMeshProUGUI cooldownText;
    private TMP_Text noHintAvailable;
    /// <summary>
    /// used to count the time spent since the system last gave a hint to the player with labelCount
    /// </summary>
    private float systemHintTimer = -1;

    /// <summary>
    /// When a label is received from Laalys, its weight is added to labelCount.
    /// LabelCount can't be negative
    /// </summary>
    private float labelCount = 0;
    /// <summary>
    /// True when the player asked help with the help button.
    /// Set to false when the information is processed and the hint is given
    /// </summary>
    private bool playerAskedHelp = false;

    /// <summary>
    /// Timer used to count the time without any action (no trace from Laalys, which means it is reseted when a Laalys action is received).
    /// Every time the timer reaches noActionFrequency, "stagntion" weight is added to labelCount and the timer is reset
    /// </summary>
    private float noActionTimer = float.MaxValue;

    /// <summary>
    /// key: enigma name, value: enigma weight.
    /// Used by GetEnigmaWeight(ComponentMonitoring monitor) to calculate the weight of an enigma to know the progression of the player
    /// </summary>
    private Dictionary<string, float> weights;

    /// <summary>
    /// The content of the scroll view containing received hint buttons (left part of help tab in IAR)
    /// </summary>
    private RectTransform scrollViewContent;
    /// <summary>
    /// prefab used to instantiate hint buttons
    /// </summary>
    private GameObject hintButtonPrefab;
    /// <summary>
    /// pool of disabled hint buttons used to enable a button when a hint is received rather then instantiating a one (optimisation)
    /// </summary>
    private List<GameObject> hintButtonsPool;

    private GameObject tmpGO;

    public static HelpSystem instance;
    public static bool shouldPause = true;
    /// <summary>
    /// Timeouts to inform thread this system is still processing. On the thread context we can't use this.Pause property because
    /// when the scene is reloaded the system is not paused and so the thread doesn't stop
    /// </summary>
    public static bool killThread;

    /// <summary>
    /// contains HelpSystem parmeters
    /// </summary>
    public static HelpSystemConfig config;

    /// <summary>
    /// Count the number of hint given
    /// </summary>
    private int hintCounter = 0;

    public float HintCooldown { 
        get
        { 
            return config.playerHintCooldownDuration - (Time.time - playerHintTimer);
        }
    }

    public float SystemHintTimer { get => systemHintTimer;}
    public float LabelCount { get => labelCount;}
    public GameHints GameHints { get => gameHints;}

    public HelpSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        initHelpSystem();
    }

    public void initHelpSystem()
    {
        // Define IAR tab state
        GameObjectManager.setGameObjectState(helpTab, !shouldPause);

        if (!shouldPause)
        {
            //add internal game hints to the dictionary of the component GameHints
            foreach (string key1 in internalGameHints.dictionary.Keys)
            {
                if (!gameHints.dictionary.ContainsKey(key1))
                    gameHints.dictionary.Add(key1, new Dictionary<string, List<KeyValuePair<string, string>>>());
                // merge hints with the same level
                foreach (string key2 in internalGameHints.dictionary[key1].Keys)
                {
                    if (!gameHints.dictionary[key1].ContainsKey(key2))
                        gameHints.dictionary[key1].Add(key2, new List<KeyValuePair<string, string>>());

                    foreach (string hintContent in internalGameHints.dictionary[key1][key2])
                        gameHints.dictionary[key1][key2].Add(new KeyValuePair<string, string>("", hintContent));
                }
            }

            // clone Petri net names from MonitoringManager
            pnNetsRemainingSteps = new Dictionary<string, int>();
            pnNetsRequiredStepsOnStart = new Dictionary<string, int>();
            foreach (string pnName in MonitoringManager.Instance.PetriNetsName)
            {
                pnNetsRemainingSteps.Add(pnName, 0);
                pnNetsRequiredStepsOnStart.Add(pnName, 0); // will be properly initialized in OnResume function
            }
            // Removes meta Petri net
            pnNetsRemainingSteps.Remove(MonitoringManager.Instance.PetriNetsName[0]);
            pnNetsRequiredStepsOnStart.Remove(MonitoringManager.Instance.PetriNetsName[0]);
            //Removes hints of the unused puzzle Petri net
            int pnSelected = -1;
            if (LoadGameContent.internalGameContent.virtualPuzzle)
                pnSelected = f_puzzlesFragment.First().GetComponent<ComponentMonitoring>().fullPnSelected;
            else
                pnSelected = f_puzzles.First().GetComponent<ComponentMonitoring>().fullPnSelected;
            RemoveHintsByPN(pnSelected);
            pnNetsRemainingSteps.Remove(MonitoringManager.Instance.PetriNetsName[pnSelected]);
            pnNetsRequiredStepsOnStart.Remove(MonitoringManager.Instance.PetriNetsName[pnSelected]);

            cleanHintsByPn = new Stack<string>();

            //format expected answers to be compared to formated answers from IARQueryEvaluator
            List<string> tmpListString;
            string tmpString;
            foreach (string key1 in gameHints.wrongAnswerFeedbacks.Keys)
            {
                tmpListString = new List<string>(gameHints.wrongAnswerFeedbacks[key1].Keys);
                foreach (string key2 in tmpListString)
                {
                    tmpString = LoadGameContent.StringToAnswer(key2);
                    if (!gameHints.wrongAnswerFeedbacks[key1].ContainsKey(tmpString))
                    {
                        // Add new upper case key (without accents) and copy value for the lower case key
                        gameHints.wrongAnswerFeedbacks[key1].Add(tmpString, gameHints.wrongAnswerFeedbacks[key1][key2]);
                        // Remove lower case entry
                        gameHints.wrongAnswerFeedbacks[key1].Remove(key2);
                    }
                }
            }

            weights = LoadGameContent.enigmasWeight;
            //count the total weighted meta actions
            foreach (string enigmaName in weights.Keys)
                totalWeightedMetaActions += weights[enigmaName];

            //get help UI components
            scrollViewContent = scrollView.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            hintButtonPrefab = scrollView.GetComponent<PrefabContainer>().prefab;

            //create a pool of int button right at the beginning and activate them when necessary rather than creating them during the game
            hintButtonsPool = new List<GameObject>();
            GameObject tmpGo;
            for (int i = 0; i < 100; i++)
            {
                tmpGo = GameObject.Instantiate(hintButtonPrefab);
                tmpGo.transform.SetParent(scrollViewContent.transform);
                tmpGo.SetActive(false);
                GameObjectManager.bind(tmpGo);
                hintButtonsPool.Add(tmpGo);
            }

            f_wrongAnswerInfo.addEntryCallback(OnWrongAnswer);

            //set player cooldown UI components
            cooldownRT = askHelpButton.transform.GetChild(1).GetComponent<RectTransform>();
            cooldownText = askHelpButton.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            noHintAvailable = askHelpButton.transform.GetChild(3).GetComponent<TMP_Text>();

            // WARNING: Before building the game, be sure that following ComponentMonitorings are properly set
            // Init dictionary to know for each enigma of the meta Petri net the associated sub Petri net id
            EnigmaIdToPnId = new Dictionary<int, int>()
                {
                    { 45,  1},
                    {144,  2},
                    {145,  3},
                    {146,  4},
                    {147,  5},
                    {149,  6},
                    {152,  7},
                    {153,  8},
                    {154,  9},
                    {155, 10},
                    {156, 11},
                    {157, 12},
                    {158, 13},
                    {141, LoadGameContent.internalGameContent.virtualPuzzle ? 14 : 15},
                    {168, 16},
                    {169, 17},
                    {170, 18},
                    {  0, 19},
                    {  4, 20},
                    { 21, 21},
                    { 99, 22},
                    {164, -1}
                };
        }
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        this.Pause = shouldPause; // stay in pause if required
        if (!this.Pause)
        {
            noActionTimer = Time.time;

            if (thread == null)
            {
                List<string> pnNames = new List<string>(pnNetsRemainingSteps.Keys);
                foreach (string pnName in pnNames)
                {
                    pnNetsRemainingSteps[pnName] = MonitoringManager.getNextActionsToReachPlayerObjective(pnName, int.MaxValue).Count;
                    if (pnNetsRequiredStepsOnStart[pnName] == 0) // not initialized
                        pnNetsRequiredStepsOnStart[pnName] = pnNetsRemainingSteps[pnName];
                }
                killThread = false;
                thread = new Thread(updatePnCompletion);
                thread.Start();
            }
        }
    }

    public void updatePnCompletion()
    {
        try
        {
            int lastCount;
            while (!killThread) // see HelpSystemKiller Monobehavior attached to MainLoop Game object
            {
                // Update each Petri net
                List<string> pnNames = new List<string>(instance.pnNetsRemainingSteps.Keys);
                foreach (string pnName in pnNames)
                {
                    lastCount = MonitoringManager.getNextActionsToReachPlayerObjective(pnName, int.MaxValue).Count;
                    if (lastCount == 0 && lastCount < instance.pnNetsRemainingSteps[pnName])
                        cleanHintsByPn.Push(pnName);
                    mut.WaitOne();
                    instance.pnNetsRemainingSteps[pnName] = lastCount;
                    mut.ReleaseMutex();
                    Thread.Sleep(250);
                }
            }
        }
        catch (Exception e){
            Debug.Log(e.Message);
        }
        thread = null;
    }

    // Evaluate if resolution process is ahead of remaining time or not.
    // Return a value between [-1.0, 1.0] that models if player is ahead from time progression. For exemple, -1.0 means time is over and no enigma was resolved, 1.0 means game is over from its beginning, 0.0 means time and resolution process are synchronized. 
    private float computeProgressionRatio()
    {
        // First compute time progression
        float timeProgression = (Time.time - timer.startingTime) / config.sessionDuration;

        // Second compute resolution progression (taking into account enigma weights)
        float resolutionDone = 0;

        mut.WaitOne(); // because pnNetsRemainingSteps is also modified in thread, we have to protect it during iterations

        foreach (KeyValuePair<string, int> entry in pnNetsRemainingSteps)
        {
            string pnName = entry.Key;
            float remainingSteps = entry.Value;
            float requiredSteps = pnNetsRequiredStepsOnStart[pnName];
            resolutionDone += ((requiredSteps - remainingSteps) / requiredSteps)*weights[pnName];
        }
        mut.ReleaseMutex();

        float enigmaProgression = resolutionDone / totalWeightedMetaActions;

        return Mathf.Clamp(enigmaProgression - timeProgression, -1f, 1f);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        // process pending traces
        foreach (GameObject go in f_traces)
            ProcessTraces(go);

        //increase labelCount if the player isn't doing anything
        if(Time.time - noActionTimer > config.noActionFrequency)
        {
            noActionTimer = Time.time;
            float progressionRatio = computeProgressionRatio();
            if (progressionRatio < 0) // means players are late
            {
                labelCount += (0.1f + labelWeights.weights["stagnation"]) / Mathf.Max(1 + progressionRatio, 0.01f);
                //if labelCount reached the step calculate the feedback level and ask a hint and the time spent since the last time the system gave a feedback reached countLabel to know if it can give another one
                if (labelCount > config.labelCountStep && Time.time - systemHintTimer > config.systemHintCooldownDuration)
                {
                    if (DisplayHint())
                    {
                        Debug.Log("Hint given due to inactivity (progression ratio: " + progressionRatio + "; labelCount: " + labelCount + ")");
                        labelCount = 0;
                    }
                }
            }
        }

        //check the time spent since the last time the player asked help
        if (Time.time - playerHintTimer < config.playerHintCooldownDuration)
        {
            //if the player stil has to wait before being able to ask help update the timer displayed and the UI
            cooldownRT.offsetMax = new Vector2(-(160 * (Time.time - playerHintTimer) / config.playerHintCooldownDuration), cooldownRT.offsetMax.y);
            float timeLeft = config.playerHintCooldownDuration - (Time.time - playerHintTimer);
            int hours = (int)timeLeft / 3600;
            int minutes = (int)(timeLeft % 3600) / 60;
            int seconds = (int)(timeLeft % 3600) % 60;
            if (hours == 0)
            {
                if(minutes == 0)
                    cooldownText.text = seconds.ToString();
                else
                    cooldownText.text = string.Concat(minutes, ":", seconds.ToString("D2"));
            }
            else
                cooldownText.text = string.Concat(hours, ":", minutes.ToString("D2"), ":", seconds.ToString("D2"));
        }
        else if (cooldownRT.gameObject.activeSelf)
        {
            //if the player can ask help but the cooldown UI is still enabled, disable it
            GameObjectManager.setGameObjectState(cooldownRT.gameObject, false);
            cooldownText.text = "";
        }

        if (noHintAvailable.color.a > 0)
            noHintAvailable.color = new Color(noHintAvailable.color.r, noHintAvailable.color.g, noHintAvailable.color.b, noHintAvailable.color.a-Time.deltaTime/5);

        while (cleanHintsByPn.Count > 0)
            RemoveHintsByPN(cleanHintsByPn.Pop());
    }

    /// <summary>
    /// This function takes into account the label of each pending trace and display Hint if score exceeds threshold and time out is over
    /// Called when a new Trace component is created (they are created by ActionManager when an action is traced)
    /// </summary>
    /// <param name="go">The gameobject of the new Trace component</param>
    private void ProcessTraces(GameObject go)
    {
        //reset no action timer every time an action is traced
        noActionTimer = Time.time;

        if (systemHintTimer < 0)
            systemHintTimer = Time.time;

        float progressionRatio = computeProgressionRatio();

        Trace[] traces = go.GetComponents<Trace>();
        Trace tmpTrace = null;
        int nbTraces = traces.Length;
        int nbLabels = -1;

        for (int i = 0; i < nbTraces; i++)
        {
            tmpTrace = traces[i];

            // get FamilyMonitoring from Family
            if (tmpTrace.componentMonitoring == null && tmpTrace.family != null)
                tmpTrace.componentMonitoring = MonitoringManager.Instance.getFamilyMonitoring(tmpTrace.family);

            // Check if hints are available for this trace
            Dictionary<string, List<KeyValuePair<string, string>>> hints;
            if (gameHints.dictionary.TryGetValue(tmpTrace.componentMonitoring.id + "." + tmpTrace.actionName, out hints))
            {
                // Check if options are defined for this action
                List < KeyValuePair<string, string>> options;
                if (hints.TryGetValue("options", out options))
                {
                    // parse all options 
                    bool askToRemove = false;
                    foreach (KeyValuePair<string, string> option in options)
                    {
                        // For options, only Value is available and could be equal to ?? or --
                        // ?? means force hint to display
                        // -- means force hint to destroy
                        if (option.Value == "??")
                            DisplayHint(tmpTrace.componentMonitoring, tmpTrace.actionName);
                        if (option.Value == "--")
                            askToRemove = true;
                    }
                    if (askToRemove)
                        gameHints.dictionary.Remove(tmpTrace.componentMonitoring.id + "." + tmpTrace.actionName);
                }
                // Check if this action is still alive => if not remove hints
                if (!tmpTrace.componentMonitoring.isStillReachable(tmpTrace.actionName))
                    gameHints.dictionary.Remove(tmpTrace.componentMonitoring.id + "." + tmpTrace.actionName);
            }

            // Take into account labels
            nbLabels = tmpTrace.labels.Length;
            //add the weight of each label to labelCount
            for (int j = 0; j < nbLabels; j++)
            {
                if (labelWeights.weights.ContainsKey(tmpTrace.labels[j]))
                {
                    labelCount += (0.1f+labelWeights.weights[tmpTrace.labels[j]]) / Mathf.Max(1 + progressionRatio, 0.01f);
                    //labelCount can't be negative
                    if (labelCount < 0)
                        labelCount = 0;

                    //if labelCount reached the step calculate the feedback level and ask a hint and the time spent since the last time the system gave a feedback reached countLabel to know if it can give another one
                    if (labelCount > config.labelCountStep && Time.time - systemHintTimer > config.systemHintCooldownDuration){
                        if (DisplayHint())
                        {
                            Debug.Log("Hint given (progression ratio: " + progressionRatio + "; labelCount: " + labelCount + ")");
                            labelCount = 0;
                        }
					}
                }
            }

            GameObjectManager.removeComponent(tmpTrace);
        }
    }

    private int getFeedbackLevel()
    {
        // Calculate appropriate feedback level depending on player progression

        float progression = computeProgressionRatio();

        // Then comute feedback level
        int feedbackLevel = -1; // default no feedback
        if (config.feedbackStep1 < progression && progression < -0.02)
            feedbackLevel = 1;
        else if (config.feedbackStep2 < progression && progression <= config.feedbackStep1)
            feedbackLevel = 2;
        else if (progression <= config.feedbackStep2)
            feedbackLevel = 3;

        return feedbackLevel;
        //return 1;
    }

    /// <summary>
    /// If the player can receive an hint, calculate the feedback level needed
    /// and ask to display an hint corresponding to the feedback level and the last room unlocked
    /// </summary>
    public void OnPlayerAskHelp()
    {
        //check cooldown before sending hint
        if (Time.time - playerHintTimer > config.playerHintCooldownDuration)
        {
            playerAskedHelp = true;
            if (DisplayHint())
            {
                //if the player received an hint, start the cooldown
                playerHintTimer = Time.time;
                GameObjectManager.setGameObjectState(cooldownRT.gameObject, true);
            }
            else
            {
                // show not hint available
                noHintAvailable.color = new Color(noHintAvailable.color.r, noHintAvailable.color.g, noHintAvailable.color.b, 1);
            }
        }
    }

    /// <summary>
    /// Called when a new WrongAnswerInfo component is created and process it to give a feedback on the answer given.
    /// These components are created when the player makes an action "Wrong"
    /// </summary>
    /// <param name="go"></param>
    private void OnWrongAnswer(GameObject go)
    {
        // Check if wrong answers are defined for each the monitor of this game object
        foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
        {
            // Parse all wrongAnswerFeedback
            foreach (string key in gameHints.wrongAnswerFeedbacks.Keys)
            {
                // Check if this feedback match with current ComponentMonitoring
                if (key.EndsWith("."+cm.id))
                {
                    // We found a feedback for this ComponentMonitoring => Parse all wrong answers defined to check if it is part of player answer
                    List<string> keys = new List<string>(gameHints.wrongAnswerFeedbacks[key].Keys);
                    foreach (string wrongAnswer in keys)
                    {
                        // Parse all wrong answers given by the player
                        foreach (WrongAnswerInfo wai in go.GetComponents<WrongAnswerInfo>())
                        {
                            // Check if the current wrong answer is part of the player answer
                            if (LoadGameContent.StringToAnswer(wai.givenAnswer).Contains(wrongAnswer))
                            {
                                //display feedback in hint list in IAR
                                string hintText = gameHints.wrongAnswerFeedbacks[key][wrongAnswer].Value;
                                // Add new hint button
                                CreateHintButton(cm, key, hintText, gameHints.wrongAnswerFeedbacks[key][wrongAnswer].Key);
                                // Remove this hint
                                gameHints.wrongAnswerFeedbacks[key].Remove(wrongAnswer);
                                break;
                            }
                        }
                    }
                }
            }
        }

        WrongAnswerInfo[] wrongAnswerArray = go.GetComponents<WrongAnswerInfo>();
        //remove WrongAnswerInfo components
        for (int i = wrongAnswerArray.Length - 1; i > -1; i--)
            GameObjectManager.removeComponent(wrongAnswerArray[i]);
    }

    /// <summary>
    /// Remove from gameHints.dictionary all hints linked to the given id.
    /// </summary>
    /// <param name="id">The id of the ComponentMonitoring that will get its hints removed</param>
    private void RemoveHintsByComponentID(int id)
    {
        List<string> keys = new List<string>(gameHints.dictionary.Keys);
        foreach (string key in keys)
        {
            if (key.StartsWith(id+"."))
                gameHints.dictionary.Remove(key);
        }
    }

    /// <summary>
    /// Remove all hints linked to a monitor of the given Petri net
    /// </summary>
    /// <param name="id">The id of the Petri net that will get its hints removed</param>
    private void RemoveHintsByPN(int id)
    {
        int nbComponentMonitoringGO = f_componentMonitoring.Count;
        ComponentMonitoring[] monitors = null;
        int nbMonitors;
        //foreach ComponentMonitoring in each gameobject in f_componentMonitoring
        for (int i = 0; i < nbComponentMonitoringGO; i++)
        {
            monitors = f_componentMonitoring.getAt(i).GetComponents<ComponentMonitoring>();
            nbMonitors = monitors.Length;
            for(int j = 0; j < nbMonitors; j++)
                //if the monitor belongs to the given Petri net, remove its hints
                if (monitors[j].fullPnSelected == id)
                    RemoveHintsByComponentID(monitors[j].id);
        }
    }

    /// <summary>
    /// Remove all hints linked to a monitor of the given Petri net
    /// </summary>
    /// <param name="name">The name of the Petri net that will get its hints removed</param>
    private void RemoveHintsByPN(string name)
    {
        //look for the id corresponding to the Petri net name and remove hints for this id
        for(int i = 0; i < MonitoringManager.Instance.PetriNetsName.Count; i++)
        {
            if(MonitoringManager.Instance.PetriNetsName[i] == name)
            {
                RemoveHintsByPN(i);
                break;
            }
        }
    }

    /// <summary>
    /// Tries to give the most appropriate hint
    /// </summary>
    /// <param name="cm"></param>
    /// <returns>true if a hint is given to the player and false otherwise</returns>
    private bool DisplayHint()
    {
        // actions candidate to hints
        List<KeyValuePair<ComponentMonitoring, string>> actionCandidates = new List<KeyValuePair<ComponentMonitoring, string>>();
        // get the shortest path in meta Petri net to end the game
        List<KeyValuePair<ComponentMonitoring, string>> usefullEnigmas = MonitoringManager.getNextActionsToReachPlayerObjective(MonitoringManager.Instance.PetriNetsName[0], int.MaxValue);
        // get all activated actions current activated enigma in meta Petri net
        List<KeyValuePair<ComponentMonitoring, string>> triggerableActions = MonitoringManager.getTriggerableActions();
        // compute intersections between the two lists to identify enabled enigmas that have to be solved in order to progress inside the game
        List<KeyValuePair<ComponentMonitoring, string>> selectedEnigmas = new List<KeyValuePair<ComponentMonitoring, string>>();
        foreach (KeyValuePair<ComponentMonitoring, string> triggerableAction in triggerableActions)
        {
            // filter action of the meta Petri net (available enigmas)
            if (triggerableAction.Key.fullPnSelected == 0)
            {
                // Check if this action is usefull to end the game
                foreach (KeyValuePair<ComponentMonitoring, string> usefullEnigma in usefullEnigmas)
                {
                    if (usefullEnigma.Key.id == triggerableAction.Key.id && usefullEnigma.Value == triggerableAction.Value)
                        selectedEnigmas.Add(usefullEnigma);
                }
            }
        }
        // Parse all selected enigmas
        foreach (KeyValuePair<ComponentMonitoring, string> selectedEnigma in selectedEnigmas)
        {
            int associatedPnToEnigma = EnigmaIdToPnId[selectedEnigma.Key.id];
            if (associatedPnToEnigma != -1)
            {
                // get the shortest path in sub Petri net to reach the end of the enigma
                List<KeyValuePair<ComponentMonitoring, string>> usefullActions = MonitoringManager.getNextActionsToReachPlayerObjective(MonitoringManager.Instance.PetriNetsName[associatedPnToEnigma], int.MaxValue);
                // compute intersections between the usefullActions and the triggerableActions in order to select action required to solve enigma
                foreach (KeyValuePair<ComponentMonitoring, string> triggerableAction in triggerableActions)
                {
                    // filter action of the selected sub Petri net
                    if (triggerableAction.Key.fullPnSelected == associatedPnToEnigma)
                    {
                        // Select candidates which are triggerable, inside the sortest path and have hints available
                        foreach (KeyValuePair<ComponentMonitoring, string> usefullAction in usefullActions)
                        {
                            if (usefullAction.Key.id == triggerableAction.Key.id && usefullAction.Value == triggerableAction.Value && gameHints.dictionary.ContainsKey(usefullAction.Key.id + "." + usefullAction.Value))
                                actionCandidates.Add(usefullAction);
                        }
                    }
                }
            }
        }
        if (actionCandidates.Count > 0)
        {
            KeyValuePair<ComponentMonitoring, string> candidateSelection = actionCandidates[(int)UnityEngine.Random.Range(0, actionCandidates.Count - 0.01f)];
            return DisplayHint(candidateSelection.Key, candidateSelection.Value);
        }
        else
            return false;
    }

    /// <summary>
    /// Find an hint for the given ComponentMonitoring action name. We assume that this action has at least one hint available.
    /// </summary>
    /// <param name="cm"></param>
    /// <returns>true if a hint is given to the player and false otherwise</returns>
    public bool DisplayHint(ComponentMonitoring cm, string actionName)
    {
        // compute level priority. 
        int requiredLevel = getFeedbackLevel();
        if (requiredLevel != -1)
        {
            // First we check asked feedback level and next from the most abstract (level 1) to the most precise (level 3)
            List<int> levelsPriority = new List<int> { requiredLevel, 1, 2, 3 };
            string hintName = "";
            int i = 0;
            while (i < levelsPriority.Count)
            {
                //check if at least one formulation exists for the current level
                if (gameHints.dictionary[cm.id + "." + actionName].ContainsKey(levelsPriority[i].ToString()))
                {
                    hintName = cm.id + "." + actionName;
                    break;
                }
                i++;
            }
            //if the feedback level is valid
            if (i < levelsPriority.Count)
            {
                string levelFound = levelsPriority[i].ToString();
                List<KeyValuePair<string, string>> availableHints = gameHints.dictionary[hintName][levelFound];
                // Get a random hint for the selected action
                int selectedHint = (int)UnityEngine.Random.Range(0, availableHints.Count - 0.01f);
                KeyValuePair<string, string> tmpPair = availableHints[selectedHint];
                string hintText = tmpPair.Value;
                // remove hints with level lower than selected one
                if (levelFound == "3")
                    gameHints.dictionary[hintName].Remove("2");
                if (levelFound == "3" || levelFound == "2")
                    gameHints.dictionary[hintName].Remove("1");
                // remove selected formulation
                availableHints.RemoveAt(selectedHint);
                if (availableHints.Count == 0)
                {
                    gameHints.dictionary[hintName].Remove(levelFound);
                    if (!gameHints.dictionary[hintName].ContainsKey("1") && !gameHints.dictionary[hintName].ContainsKey("2") && !gameHints.dictionary[hintName].ContainsKey("3"))
                        gameHints.dictionary.Remove(hintName);
                }
                CreateHintButton(cm, actionName, hintText, tmpPair.Key, levelFound);
                return true;
            }
            else
            {
                Debug.Log(string.Concat("No hint found for the action \"", actionName, "\" of the ComponentMonitoring id ", cm.id, " (GameObject name: ", cm.gameObject.name, ")"));
                playerAskedHelp = false;
                return false;
            }
        }
        else
        {
            Debug.Log("Player is ahead of time => no feedback for the moment (progression ratio: "+ computeProgressionRatio()+")");
            playerAskedHelp = false;
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hintMonitor"></param>
    /// <param name="actionName"></param>
    /// <param name="hintText"></param>
    /// <param name="hintLink"></param>
    public Button CreateHintButton(ComponentMonitoring hintMonitor, string actionName, string hintText, string hintLink = "", string hintLevel = "", bool enableTrace = true)
    {
        //show a button to see hint content in hint tab in IAR
        if (hintButtonsPool.Count == 0)
        {
            //create a new hint button if pool is empty
            tmpGO = GameObject.Instantiate(hintButtonPrefab);
            tmpGO.transform.SetParent(scrollViewContent.transform);
            tmpGO.SetActive(false);
            GameObjectManager.bind(tmpGO);
            hintButtonsPool.Add(tmpGO);

            Debug.LogWarning("You should increase hintButtonsPool initial size");
        }

        tmpGO = hintButtonsPool[0];
        hintButtonsPool.RemoveAt(0);
        Button hintButton = tmpGO.GetComponent<Button>();
        GameObjectManager.setGameObjectState(tmpGO, true);

        HintContent tmpHC = tmpGO.GetComponent<HintContent>();
        tmpHC.monitor = hintMonitor;
        tmpHC.actionName = actionName;
        tmpHC.text = hintText;
        tmpHC.level = hintLevel;
        tmpHC.link = hintLink;

        hintCounter++;
        tmpGO.transform.GetChild(0).GetComponent<TMP_Text>().text = LoadGameContent.internalGameContent.hintButtonText + " " + hintCounter;

        if (enableTrace)
        {
            GameObjectManager.addComponent<ActionPerformedForLRS>(hintButton.gameObject, new
            {
                verb = "received",
                objectType = "feedback",
                activityExtensions = new Dictionary<string, string>() {
                    { "type", "hint" },
                    { "value", hintButton.transform.GetChild(0).GetComponent<TMP_Text>().text },
                    { "from", playerAskedHelp ? "button" : "system" },
                    { "reference", hintMonitor.id+"."+actionName },
                    { "content", tmpHC.text }
                }
            });
        }

        if (playerAskedHelp)
            playerAskedHelp = false;
        else
            systemHintTimer = Time.time;

        return hintButton;
    }

    /// <summary>
    /// Used in SaveManager during loading
    /// </summary>
    public void LoadHelpSystemValues(float hintCooldown, float systemHintTimer, float helpLabelCount)
    {
        playerHintTimer = Time.time + hintCooldown - config.playerHintCooldownDuration;

        float timeLeft = config.playerHintCooldownDuration - (Time.time - playerHintTimer);
        if (timeLeft > 0)
            GameObjectManager.setGameObjectState(cooldownRT.gameObject, true);

        this.systemHintTimer = systemHintTimer;
        labelCount = helpLabelCount;
        noActionTimer = Time.time;
    }
}