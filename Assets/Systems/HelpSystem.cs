using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.Monitoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;
using System.Threading;
using UnityEngine.SceneManagement;

public class HelpSystem : FSystem {

    private Family f_subtitlesFamily = FamilyManager.getFamily(new AnyOfTags("HelpSubtitles"), new AllOfComponents(typeof(TextMeshProUGUI)));
    private Family f_traces = FamilyManager.getFamily(new AllOfComponents(typeof(Trace)));
    private Family f_gameHints = FamilyManager.getFamily(new AllOfComponents(typeof(GameHints)));
    private Family f_internalGameHints = FamilyManager.getFamily(new AllOfComponents(typeof(InternalGameHints)));
    private Family f_timer = FamilyManager.getFamily(new AllOfComponents(typeof(Timer)));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));
    private Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformed)));
    private Family f_actionsProcessed = FamilyManager.getFamily(new NoneOfComponents(typeof(ActionPerformed)));
    private Family f_componentMonitoring = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));
    private Family f_askHelpButton = FamilyManager.getFamily(new AnyOfTags("AskHelpButton"), new AllOfComponents(typeof(Button)));
    private Family f_labelWeights = FamilyManager.getFamily(new AllOfComponents(typeof(LabelWeights)));
    private Family f_wrongAnswerInfo = FamilyManager.getFamily(new AllOfComponents(typeof(WrongAnswerInfo), typeof(ComponentMonitoring)));
    private Family f_IARTab = FamilyManager.getFamily(new AnyOfTags("IARTab"));
    private Family f_HUD_H = FamilyManager.getFamily(new AnyOfTags("HUD_H"));

    private Family f_puzzles = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new NoneOfComponents(typeof(DreamFragment)), new AllOfComponents(typeof(ComponentMonitoring)));
    private Family f_puzzlesFragment = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new AllOfComponents(typeof(DreamFragment), typeof(ComponentMonitoring)));

    private Family f_scrollView = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(PrefabContainer)));
    private Family f_enabledHintsIAR = FamilyManager.getFamily(new AllOfComponents(typeof(HintContent)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));

    /// <summary>
    /// Contains hints about the pedagogic content of the game (hints about enigmas and feedback when the player gives a wrong answer)
    /// </summary>
    private GameHints gameHints;
    /// <summary>
    /// Contains hint about game mechanics
    /// </summary>
    private InternalGameHints internalGameHints;
    /// <summary>
    /// Contains weights for each Laalys labels used to increase/decrease labelCount
    /// </summary>
    private Dictionary<string, float> labelWeights;

    /// <summary>
    /// Key: room id
    /// value:  Dictionary<Key: Petri net id, Value: ComponentMonitoring corresponding to the enigma in the meta Petri net.>
    /// This dictionary is used to get from room id the ComponentMonitorings associated to each enigma of the room
    /// </summary>
    private Dictionary<int, Dictionary<int, ComponentMonitoring>> rooms2Enigmas;

    /// <summary>
    /// Associate for each Petri net name the number of remaining action to reach a player objective in this Petri net
    /// </summary>
    private Dictionary<string, int> pnNetsCompletion;
    /// <summary>
    /// Contains the minimal number of steps (actions) to finish the game
    /// </summary>
    private int requiredSteps;
    /// <summary>
    /// A Thread to compute remaining steps for each Petri net
    /// </summary>
    private static Thread thread = null;

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
    /// count the number of feedback given to the player with labelCount or the help button
    /// </summary>
    private int nbFeedBackGiven = 0;
    /// <summary>
    /// True when the player asked help with the help button.
    /// Set to false when the information is processed and the hint is given
    /// </summary>
    private bool playerAskedHelp = false;

    /// <summary>
    /// contains the id of the last room unlocked by the player to know which room the hint has to be about
    /// </summary>
    private UnlockedRoom room;

    /// <summary>
    /// Used to debug: used to display the hint text with subtitles and disable subtitles after 2 seconds
    /// </summary>
    private TextMeshProUGUI subtitles;
    private float subtitlesTimer = float.MinValue;

    /// <summary>
    /// Timer used to count the time without any action (no trace from Laalys, which means it is reseted when a Laalys action is received).
    /// Every time the timer reaches noActionFrequency, "stagntion" weight is added to labelCount and the timer is reset
    /// </summary>
    private float noActionTimer = float.MaxValue;

    /// <summary>
    /// CompoenentMonitoring of the last action in the meta Petri net
    /// </summary>
    private ComponentMonitoring finalComponentMonitoring;

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
    private RectTransform tmpRT;
    private HintContent tmpHC;
    private KeyValuePair<string, List<string>> tmpPair;

    /// <summary>
    /// Pair key: name, Pair value: overrideName.
    /// Stores the gameObject, the name and the overrideName of the ActionPerformed
    /// and processes it after the system ActionManager processed all ActionPerformed of the gameobject.
    /// This information is used only when the ActionManager processed this data because else actions are not performed in Petri nets yet.
    /// The information has to be stored because once processed, the ActionPerformed component is removed by ActionManager
    /// </summary>
    private Dictionary<GameObject, Queue<KeyValuePair<string, string>>> actionPerformedHistory;

    public static HelpSystem instance;
    public static bool shouldPause = true;
    /// <summary>
    /// Timeouts to inform thread this system is still processing. On the thread context we can't use this.Pause property because
    /// when the scene is reloaded the system is not paused and so the thread doesn't stop
    /// </summary>
    private static int lastTimeout;
    private static int currentTimeout;

    /// <summary>
    /// contains HelpSystem parmeters
    /// </summary>
    public static HelpSystemConfig config;

    public HelpSystem()
    {
        if (Application.isPlaying)
        {
            if (!shouldPause)
            {
                //get game hints filled with tips loaded from "Data/Hints_LearningScape.txt"
                gameHints = f_gameHints.First().GetComponent<GameHints>();
                //get internal game hints
                internalGameHints = f_internalGameHints.First().GetComponent<InternalGameHints>();

                //add internal game hints to the dictionary of the component GameHints
                //if the key2 already exists in gameHints.dictionary and gameHints.dictionary[key1][key2].Value isn't empty internalGameHints.dictionary[key1][key2] isn't added
                foreach (string key1 in internalGameHints.dictionary.Keys)
                {
                    if (!gameHints.dictionary.ContainsKey(key1))
                        gameHints.dictionary.Add(key1, new Dictionary<string, List<KeyValuePair<string, List<string>>>>());
                    // merge hints with the same level
                    foreach (string key2 in internalGameHints.dictionary[key1].Keys)
                    {
                        if (!gameHints.dictionary[key1].ContainsKey(key2))
                            gameHints.dictionary[key1].Add(key2, new List<KeyValuePair<string, List<string>>>());

                        gameHints.dictionary[key1][key2].Add(new KeyValuePair<string, List<string>>("", internalGameHints.dictionary[key1][key2]));
                    }
                }

                // Get labels weight
                labelWeights = f_labelWeights.First().GetComponent<LabelWeights>().weights;

                requiredSteps = 0;

                // clone Petri net names from MonitoringManager
                pnNetsCompletion = new Dictionary<string, int>();
                foreach (string pnName in MonitoringManager.Instance.PetriNetsName)
                    pnNetsCompletion.Add(pnName, 0);
                // Removes meta Petri net corresponding to the scene name
                pnNetsCompletion.Remove(SceneManager.GetActiveScene().name);
                //Removes hints of the unused puzzle Petri net
                int pnSelected = -1;
                if (LoadGameContent.gameContent.virtualPuzzle)
                    pnSelected = f_puzzlesFragment.First().GetComponent<ComponentMonitoring>().fullPnSelected;
                else
                    pnSelected = f_puzzles.First().GetComponent<ComponentMonitoring>().fullPnSelected;
                RemoveHintsByPN(pnSelected);
                pnNetsCompletion.Remove(MonitoringManager.Instance.PetriNetsName[pnSelected]);

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

                config.sessionDuration *= 60; //convert to seconds

                room = f_unlockedRoom.First().GetComponent<UnlockedRoom>();

                subtitles = f_subtitlesFamily.First().GetComponent<TextMeshProUGUI>();

                weights = LoadGameContent.enigmasWeight;
                //count the total weighted meta actions
                foreach (string enigmaName in weights.Keys)
                    totalWeightedMetaActions += weights[enigmaName];

                //get help UI components
                scrollViewContent = f_scrollView.First().transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                hintButtonPrefab = f_scrollView.First().GetComponent<PrefabContainer>().prefab;

                //create a pool of int button right at the beginning and activate them when necessary rather than creating them during the game
                hintButtonsPool = new List<GameObject>();
                GameObject tmpGo;
                for (int i = 0; i < 300; i++)
                {
                    tmpGo = GameObject.Instantiate(hintButtonPrefab);
                    tmpGo.transform.SetParent(scrollViewContent.transform);
                    tmpGo.SetActive(false);
                    GameObjectManager.bind(tmpGo);
                    hintButtonsPool.Add(tmpGo);
                }

                f_traces.addEntryCallback(OnNewTraces);

                // The information store in OnNewActionPerformed is used only when the ActionManager processed ActionPerformed data
                // because else actions are not performed in Petri nets yet.
                // The information has to be stored because once processed, the ActionPerformed component is removed by ActionManager
                // OnActionsProcessed callback has to be set before OnNewActionPerformed callback because
                // else OnActionsProcessed will process data stored by OnNewActionPerformed and not processed by ActionManager yet
                f_actionsProcessed.addEntryCallback(OnActionsProcessed);
                f_actions.addEntryCallback(OnNewActionPerformed);

                f_wrongAnswerInfo.addEntryCallback(OnWrongAnswer);
                f_askHelpButton.First().GetComponent<Button>().onClick.AddListener(OnPlayerAskHelp);

                //set player cooldown UI components
                cooldownRT = f_askHelpButton.First().transform.GetChild(1).GetComponent<RectTransform>();
                cooldownText = f_askHelpButton.First().transform.GetChild(2).GetComponent<TextMeshProUGUI>();

                actionPerformedHistory = new Dictionary<GameObject, Queue<KeyValuePair<string, string>>>();

                // WARNING: Before building the game, be sure that following ComponentMonitoring are properly set
                //Get the last monitor in the meta Petri net
                finalComponentMonitoring = MonitoringManager.getMonitorById(164);
                // Init dictionary to know for each rooms [0, 3] which petrinet id is concerned (petri net id is the index of petri net in MonitoringManager). And for each of these petrinet define the ComponentMonitoring associated inside Meta petri net
                rooms2Enigmas = new Dictionary<int, Dictionary<int, ComponentMonitoring>>()
                {
                    {
                        // room 0
                        0, new Dictionary<int, ComponentMonitoring>()
                        {
                            // room 0 is concerned by only one petri net, the second inside MonitoringManager and the ComponentMonitoring modeling this petri net inside the Meta petri net is the one this specified id
                            {1, MonitoringManager.getMonitorById(45)}
                        }
                    },
                    {
                        // room 1
                        1, new Dictionary<int, ComponentMonitoring>()
                        {
                            // room 1 is concerned by the 5 following petri nets and the ComponentMonitoring modeling these petri nets inside Meta petri net are those specified
                            {2, MonitoringManager.getMonitorById(144)},
                            {3, MonitoringManager.getMonitorById(145)},
                            {4, MonitoringManager.getMonitorById(146)},
                            {5, MonitoringManager.getMonitorById(147)},
                            {6, MonitoringManager.getMonitorById(149)}
                        }
                    },
                    {
                        // room 2 is concerned by blablabla (see previous comments, is the same)
                        2, new Dictionary<int, ComponentMonitoring>()
                        {
                            {7, MonitoringManager.getMonitorById(152)},
                            {8, MonitoringManager.getMonitorById(153)},
                            {9, MonitoringManager.getMonitorById(154)},
                            {10, MonitoringManager.getMonitorById(155)},
                            {11, MonitoringManager.getMonitorById(156)},
                            {12, MonitoringManager.getMonitorById(157)},
                            {13, MonitoringManager.getMonitorById(158)}
                        }
                    },
                    {
                        // room 3 is concerned by blablabla (see previous comments, is the same)
                        3, new Dictionary<int, ComponentMonitoring>()
                        {
                            {14, MonitoringManager.getMonitorById(141)}, // both 14th and 15th petri net have the same ComponentMonitoring id inside Meta Pn (the puzzle) 
                            {15, MonitoringManager.getMonitorById(141)}, // both 14th and 15th petri net have the same ComponentMonitoring id inside Meta Pn (the puzzle)
                            {16, MonitoringManager.getMonitorById(168)},
                            {17, MonitoringManager.getMonitorById(169)},
                            {18, MonitoringManager.getMonitorById(170)},
                            {19, MonitoringManager.getMonitorById(0)},
                            {20, MonitoringManager.getMonitorById(4)},
                            {21, MonitoringManager.getMonitorById(21)},
                            {22, MonitoringManager.getMonitorById(99)}
                        }
                    }
                };
            }
            else
            {
                // Remove HUD H
                GameObjectManager.unbind(f_HUD_H.First());
                GameObject.Destroy(f_HUD_H.First());
                // Disable IAR tab
                GameObject tmpGO = null;
                for (int i = 0; i < f_IARTab.Count; i++)
                {
                    tmpGO = f_IARTab.getAt(i);
                    if (tmpGO.transform.parent.gameObject.name == "HelpTab")
                    {
                        GameObjectManager.setGameObjectState(tmpGO.transform.parent.GetChild(0).gameObject, true);
                        GameObjectManager.setGameObjectState(tmpGO.transform.parent.GetChild(1).gameObject, false);
                        break;
                    }
                }
            }
        }
        instance = this;
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
        this.Pause = shouldPause; // stay in pause if required
        if (!this.Pause)
        {
            noActionTimer = Time.time;

            // Compute the number of steps to finish the game
            if (requiredSteps == 0)
            {
                List<string> pnNames = new List<string>(pnNetsCompletion.Keys);
                foreach (string pnName in pnNames)
                {
                    pnNetsCompletion[pnName] = MonitoringManager.getNextActionsToReachPlayerObjective(pnName, int.MaxValue).Count;
                    requiredSteps += pnNetsCompletion[pnName];
                }

                if (thread == null)
                {
                    lastTimeout = -1;
                    currentTimeout = 0;
                    thread = new Thread(updatePnNetsCompletion);
                    thread.Start();
                }
            }
        }
    }

    private void updatePnNetsCompletion()
    {
        try
        {
            while (lastTimeout < currentTimeout)
            {
                lastTimeout = currentTimeout;
                Thread.Sleep(4000); 
                // Update each Petri net
                List<string> pnNames = new List<string>(HelpSystem.instance.pnNetsCompletion.Keys);
                foreach (string pnName in pnNames)
                {
                    HelpSystem.instance.pnNetsCompletion[pnName] = MonitoringManager.getNextActionsToReachPlayerObjective(pnName, int.MaxValue).Count;
                    Thread.Sleep(250);
                }
            }
        }
        catch (Exception e){}
        thread = null;
    }

    private float computeProgressionRatio()
    {
        //enigma done / total enigma
        float enigmaProgression = (totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions;
        //time spent / session duration
        float timeProgression = (Time.time - f_timer.First().GetComponent<Timer>().startingTime) / config.sessionDuration;
        //(nb enigma done / total nb enigma) / (current time / total duration)
        // if time simulation is ahead of enigma resolution then progressionRation is < 1 and then will boost label weight
        // if enigma resolution is ahead of time simulation then progressionRation is > 1 and then will reduce label weight
        return enigmaProgression / timeProgression;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        currentTimeout = familiesUpdateCount;
        if (subtitles.gameObject.activeSelf && Time.time - subtitlesTimer > 2)
        {
            subtitles.text = string.Empty;
            GameObjectManager.setGameObjectState(subtitles.gameObject, false);
        }

        //increase labelCount if the player isn't doing anything
        if(Time.time - noActionTimer > config.noActionFrequency)
        {
            noActionTimer = Time.time;
            //(nb enigma done / total nb enigma) / (current time / total duration)
            float progressionRatio = computeProgressionRatio();
            if (progressionRatio == 0)
                labelCount += labelWeights["stagnation"];
            else
                labelCount += labelWeights["stagnation"] / progressionRatio;
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
    }

    /// <summary>
    /// Called when a new Trace component is created (they are created by ActionManager when an action is traced)
    /// </summary>
    /// <param name="go">The gameobject of the new Trace component</param>
    private void OnNewTraces(GameObject go)
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

        //increase/decrease labelCount depending on the labels
        //give feedback if necessary
        for (int i = 0; i < nbTraces; i++)
        {
            tmpTrace = traces[i];
            nbLabels = tmpTrace.labels.Length;
            //add the weight of each label to labelCount
            for (int j = 0; j < nbLabels; j++)
            {
                if (labelWeights.ContainsKey(tmpTrace.labels[j]))
                {
                    if (progressionRatio != 0)
                        labelCount += labelWeights[tmpTrace.labels[j]] / progressionRatio;
                    else
                        labelCount += labelWeights[tmpTrace.labels[j]];
                    //labelCount can't be negative
                    if (labelCount < 0)
                        labelCount = 0;

                    //if labelCount reached the step calculate the feedback level and ask a hint and the time spent since the last time the system gave a feedback reached countLabel to know if it can give another one
                    if (labelCount > config.labelCountStep && Time.time - systemHintTimer > config.systemHintCooldownDuration)
                        DisplayHint(room.roomNumber);
                }
            }
            GameObjectManager.removeComponent(tmpTrace);
        }
    }

    /// <summary>
    /// Called when a ActionPerformed component is created and store its info in the dictionary actionPerformedHistory.
    /// This information is used only when the ActionManager processed this data because else actions are not performed in Petri nets yet.
    /// The information has to be stored because once processed, the ActionPerformed component is removed by ActionManager
    /// </summary>
    /// <param name="go">The game object of the new ActionPerformed</param>
    private void OnNewActionPerformed(GameObject go)
    {
        ActionPerformed[] actions = go.GetComponents<ActionPerformed>();
        //initialize a queue for this gameobject in actionPerformedHistory if it doesn't exist
        if (!actionPerformedHistory.ContainsKey(go))
            actionPerformedHistory.Add(go, new Queue<KeyValuePair<string, string>>());
        for (int i = 0; i < actions.Length; i++)
        {
            //add the name and the overridename for each ActionPerformed to the dictionary
            //they will then be used to find the corresponding ComponentMonitoring on the gameobject
            ActionPerformed ap = actions[i];
            actionPerformedHistory[go].Enqueue(new KeyValuePair<string, string>(ap.name, ap.overrideName));
        }
    }

    /// <summary>
    /// Called when all ActionPerformed components have been processed and removed by ActionManager 
    /// and uses the stored information to remove hint about actions that can't be reached anymore in Petri nets.
    /// This information is used only when the ActionManager processed this data because else actions are not performed in Petri nets yet.
    /// The information has to be stored because once processed, the ActionPerformed component is removed by ActionManager
    /// </summary>
    /// <param name="go">The game object of the removed ActionPerformed</param>
    private void OnActionsProcessed(GameObject go)
    {
        if (actionPerformedHistory.ContainsKey(go))
        {
            int historySize = actionPerformedHistory[go].Count;
            //foreach information stored for this gameobject
            for(int i = 0; i < historySize; i++)
            {
                //find monitors corresponding to the stored name and overrideName
                List<KeyValuePair<ComponentMonitoring, TransitionLink>> cMonitors = FindMonitors(go, actionPerformedHistory[go].Peek().Key, actionPerformedHistory[go].Peek().Value);
                //remove the information of the list once processed
                actionPerformedHistory[go].Dequeue();
                foreach (KeyValuePair < ComponentMonitoring, TransitionLink > cm in cMonitors)
                {
                    //delete hints linked to the ComponentMonitoring if it is not reachable anymore
                    bool reachable = false;
                    foreach(TransitionLink tl in cm.Key.transitionLinks)
                    {
                        foreach (List<string> linksConcerned in cm.Key.getPossibleSetOfLinks(tl.transition.label))
                        {
                            if (MonitoringManager.getNextActionsToReach(cm.Key, tl.transition.label, int.MaxValue, linksConcerned.ToArray()).Count > 0)
                            {
                                reachable = true;
                                break;
                            }
                        }
                        if (reachable) break;
                    }
                    if (!reachable)
                        RemoveHintsByComponentID(cm.Key.id);
                    //if action performed is a player objective/end action, remove all hints linked to monitors of the same Petri net
                    if (cm.Value.isEndAction)
                        RemoveHintsByPN(cm.Key.fullPnSelected);
                    // Remove Hints conditioned by the non execution of this action
                    RemoveHintsInhibedByAction(cm.Key, cm.Value);
                }
            }
        }
    }

    private int getFeedbackLevel()
    {
        // Calculate appropriate feedback level depending on the number of feedback given, the number feedback availlable, the time spent and the time left

        // First compute time progression
        float timeProgression = (Time.time - f_timer.First().GetComponent<Timer>().startingTime) / config.sessionDuration;

        // Second compute steps progression
        int stepsDone = requiredSteps;
        foreach (int remainingSteps in pnNetsCompletion.Values)
            stepsDone -= remainingSteps;

        float stepProgression = (float)stepsDone / requiredSteps;

        // Then comute feedback level
        int feedbackLevel = -1; // default no feedback
        float delta = stepProgression - timeProgression;
        if (config.feedbackStep1 < delta && delta < 0)
            feedbackLevel = 1;
        else if (config.feedbackStep2 < delta && delta <= config.feedbackStep1)
            feedbackLevel = 2;
        else if (delta <= config.feedbackStep2)
            feedbackLevel = 3;

        return feedbackLevel;
        //return 1;
    }

    /// <summary>
    /// If the player can receive an hint, calculate the feedback level needed
    /// and ask to display an hint corresponding to the feedback level and the last room unlocked
    /// </summary>
    private void OnPlayerAskHelp()
    {
        //check cooldown before sending hint
        if(Time.time - playerHintTimer > config.playerHintCooldownDuration)
        {
            playerAskedHelp = true;
            if (DisplayHint(room.roomNumber))
            {
                //if the player received an hint, start the cooldown
                playerHintTimer = Time.time;
                GameObjectManager.setGameObjectState(cooldownRT.gameObject, true);
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
                    foreach(string wrongAnswer in gameHints.wrongAnswerFeedbacks[key].Keys)
                    {
                        // Parse all wrong answers given by the player
                        foreach (WrongAnswerInfo wai in go.GetComponents<WrongAnswerInfo>())
                        {
                            // Check if the current wrong answer is part of the player answer
                            if (wai.givenAnswer.Contains(wrongAnswer))
                            {
                                //display feedback in hint list in IAR
                                string hintText = "";
                                List<string> availableFeedbacks = gameHints.wrongAnswerFeedbacks[key][wrongAnswer].Value;
                                if (availableFeedbacks.Count > 0)
                                {
                                    // Choose random hint
                                    int randomPos = new System.Random().Next(availableFeedbacks.Count);
                                    hintText = availableFeedbacks[randomPos];
                                    // Add new hint button
                                    Button hintButton = CreateHintButton(key, hintText, cm.id, gameHints.wrongAnswerFeedbacks[key][wrongAnswer].Key);
                                    // Remove this hint
                                    availableFeedbacks.RemoveAt(randomPos);

                                    GameObjectManager.addComponent<ActionPerformedForLRS>(hintButton.gameObject, new
                                    {
                                        verb = "received",
                                        objectType = "feedback",
                                        objectName = string.Concat("hint_", hintButton.transform.GetChild(0).GetComponent<Text>().text),
                                        activityExtensions = new Dictionary<string, List<string>>() {
                                            { "type", new List<string>() { "wrongAnwserHint" } },
                                            { "from", new List<string>() { "system" } },
                                            { "content", new List<string>() { hintButton.GetComponent<HintContent>().text } }
                                        }
                                    });
                                    break;
                                }
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
            if (key.EndsWith("."+id))
                gameHints.dictionary.Remove(key);
        }
    }

    /// <summary>
    /// Look for hints for the ComponentMonitoring parameters and remove these hints if they are tagged by the name of the action
    /// </summary>
    /// <param name="cm"></param>
    /// <param name="tl"></param>
    private void RemoveHintsInhibedByAction(ComponentMonitoring cm, TransitionLink tl)
    {
        string cmId = "."+cm.id.ToString();
        List<string> keys = new List<string>(gameHints.dictionary.Keys);
        foreach (string key in keys)
        {
            if (key.EndsWith(cmId))
            {
                string[] tokens = key.Split('.');
                if (tokens[1].EndsWith("##"+tl.transition.label) || tokens[1].EndsWith("##" + tl.transition.overridedLabel))
                    gameHints.dictionary.Remove(key);
            }
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
    /// Returns a list of TransitionLink corresponding to the name and the overrideName given and their ComponentMonitoring on the given GameObject.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name"></param>
    /// <param name="overrideName"></param>
    /// <returns></returns>
    private List<KeyValuePair<ComponentMonitoring, TransitionLink>> FindMonitors(GameObject go, string name, string overrideName)
    {
        List<KeyValuePair<ComponentMonitoring, TransitionLink>> cMonitors = new List<KeyValuePair<ComponentMonitoring, TransitionLink>>();

        if (name != "" && overrideName != "")
        {
            //look for ComponentMonitorings corresponding to name and overridename
            foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                foreach (TransitionLink tl in cm.transitionLinks)
                    if (tl.transition.label == name && tl.transition.overridedLabel == overrideName)
                        cMonitors.Add(new KeyValuePair<ComponentMonitoring, TransitionLink>(cm, tl));
        }
        else if (name != "")
        {
            //look for the ComponentMonitoring corresponding to the name
            foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                foreach (TransitionLink tl in cm.transitionLinks)
                    if (tl.transition.label == name)
                        cMonitors.Add(new KeyValuePair<ComponentMonitoring, TransitionLink>(cm, tl));
        }
        else if (overrideName != "")
        {
            //look for the ComponentMonitoring corresponding to the overridename
            foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                foreach (TransitionLink tl in cm.transitionLinks)
                    if (tl.transition.overridedLabel == overrideName)
                        cMonitors.Add(new KeyValuePair<ComponentMonitoring, TransitionLink>(cm, tl));
        }
        return cMonitors;
    }

    /// <summary>
    /// Tries to give an hint with the given room and feedback level.
    /// If no hint is found for the given feedback level it will look for hint for another feedback level,
    /// but if no hint is found for the given room it stops looking for a hint
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    private bool DisplayHint(int room)
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
                //get a hint corresponding to the room and the feedback level
                hintName = GetHintName(room, levelsPriority[i]);
                if (hintName != "")
                    break;
                i++;
            }
            //if the feedback level is valid
            if (i < levelsPriority.Count)
            {
                string levelFound = levelsPriority[i].ToString();
                List<KeyValuePair<string, List<string>>> availableHints = gameHints.dictionary[hintName][levelFound];
                // Get a random hint for the selected room/Monitor/level
                int selectedHint = (int)UnityEngine.Random.Range(0, availableHints.Count - 0.01f);
                tmpPair = availableHints[selectedHint];
                // Get a random formulation for this hint that is not already displayed to the player
                string hintText = "";
                // First extract all formulations not already displayed to the player
                List<int> uniqueFormulationIds = new List<int>();
                for (int j = 0 ; j < tmpPair.Value.Count ; j++)
                {
                    bool isUnique = true;
                    foreach (GameObject go in f_enabledHintsIAR)
                        if (go.GetComponent<HintContent>().text == tmpPair.Value[j])
                        {
                            isUnique = false;
                            break;
                        }
                    if (isUnique)
                        uniqueFormulationIds.Add(j);
                }
                int randomForm = uniqueFormulationIds[(int)UnityEngine.Random.Range(0, uniqueFormulationIds.Count - 0.01f)];
                hintText = tmpPair.Value[randomForm];
                // remove selected formulation
                tmpPair.Value.RemoveAt(randomForm);
                // remove hint if no more formulation exists
                if (tmpPair.Value.Count == 0)
                {
                    //remove hint from dictionary
                    availableHints.RemoveAt(selectedHint);

                    if (availableHints.Count == 0)
                    {
                        gameHints.dictionary[hintName].Remove(levelFound);
                        if (gameHints.dictionary[hintName].Keys.Count == 0)
                            gameHints.dictionary.Remove(hintName);
                    }
                }
                string[] tokens = hintName.Split('.');
                int monitorId = int.Parse(tokens[tokens.Length - 1]);
                Button hintButton = CreateHintButton(hintName, hintText, monitorId, tmpPair.Key);

                GameObjectManager.addComponent<ActionPerformedForLRS>(hintButton.gameObject, new
                {
                    verb = "received",
                    objectType = "feedback",
                    objectName = string.Concat("hint_", hintButton.transform.GetChild(0).GetComponent<Text>().text),
                    activityExtensions = new Dictionary<string, List<string>>() {
                    { "type", new List<string>() { "hint" } },
                    { "from", new List<string>() { playerAskedHelp ? "button" : "system" } },
                    { "content", new List<string>() { hintButton.GetComponent<HintContent>().text } }
                }
                });

                systemHintTimer = Time.time;
                nbFeedBackGiven++;
                labelCount = 0;
                playerAskedHelp = false;
                return true;
            }
            else
            {
                Debug.Log(string.Concat("No hint found for the room ", room));
                playerAskedHelp = false;
                return false;
            }
        }
        else
        {
            Debug.Log("Player is ahead of time => no feedback for the moment");
            playerAskedHelp = false;
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hintName">"name.monitorID"</param>
    /// <param name="hintText"></param>
    /// <param name="hintMonitorID"></param>
    /// <param name="hintLink"></param>
    private Button CreateHintButton(string hintName, string hintText, int hintMonitorID, string hintLink = "")
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
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - You should increase hintButtonsPool initial size."));
        }

        tmpGO = hintButtonsPool[0];
        hintButtonsPool.RemoveAt(0);
        Button hintButton = tmpGO.GetComponent<Button>();
        GameObjectManager.setGameObjectState(tmpGO, true);

        tmpHC = tmpGO.GetComponent<HintContent>();
        tmpHC.hintName = hintName;
        tmpHC.text = hintText;
        tmpHC.link = hintLink;
        if (hintMonitorID != -1)
            tmpHC.monitor = MonitoringManager.getMonitorById(hintMonitorID);

        //change subtitle text to display the hint
        //subtitles.text = tmpPair.Value[(int)UnityEngine.Random.Range(0, nbHintTexts - 0.01f)];
        //GameObjectManager.setGameObjectState(subtitles.gameObject, true);
        //subtitlesTimer = Time.time;
        return hintButton;
    }

    private float GetWeightedNumberEnigmasLeft()
    {
        if (finalComponentMonitoring)
        {
            float left = 0;
            List<KeyValuePair<ComponentMonitoring,string>> nextActions = MonitoringManager.getNextActionsToReach(finalComponentMonitoring, "perform", int.MaxValue);
            float weight = -1;

            foreach (KeyValuePair<ComponentMonitoring, string> action in nextActions)
            {
                weight = GetEnigmaWeight(action.Key);
                if (weight > 0)
                    left += weight;
            }

            //Return the number action left multiplied by they weight to reach the last action of the meta Petri net
            return left;
        }
        else
            return -1;
    }

    private float GetEnigmaWeight(ComponentMonitoring monitor)
    {
        // Look for petri net id associated to this meta monitor
        int petriNetId = -1;
        foreach (Dictionary<int, ComponentMonitoring> pn2cm in rooms2Enigmas.Values)
        {
            foreach (KeyValuePair<int, ComponentMonitoring> entry in pn2cm)
            {
                if (entry.Value.id == monitor.id)
                {
                    petriNetId = entry.Key;
                    break;
                }
                if (petriNetId != -1)
                    break;
            }
        }
        // if no petri nets found, return 0
        if (petriNetId == -1)
            return 0;

        string key = MonitoringManager.Instance.PetriNetsName[petriNetId];

        if (weights.ContainsKey(key))
            return weights[key];
        else
            return 0;
    } 

    /// <summary>
    /// Return the name of a randomly selected hint among hints corresponding to the room and the feedback level given. The hint selected contains at least one formulation not already included into the list of hints displayed to the player
    /// </summary>
    /// <param name="room"></param>
    /// <param name="feedbackLevel"></param>
    /// <returns></returns>
    private string GetHintName(int room, int feedbackLevel)
    {
        List<int> nextMonitorIds = GetNextActions(room);
        List<string> availableHintNames = new List<string>();

        foreach (string key in gameHints.dictionary.Keys)
        {
            // First check all key that starts with room number
            if (key.StartsWith(room.ToString()+"."))
            {
                // Then check once that finishes with one of the enabled monitor
                foreach (int monitorId in nextMonitorIds)
                {
                    if (key.EndsWith("."+monitorId.ToString()))
                    {
                        if (gameHints.dictionary[key].ContainsKey(feedbackLevel.ToString()))
                        {
                            // Check if this formulation is not already available in the list of hints (even if it's not for the same ComponentMonitoring id)
                            if (containsUniqueHint(gameHints.dictionary[key][feedbackLevel.ToString()]))
                                availableHintNames.Add(key);
                        }
                    }
                }
            }
        }

        if (availableHintNames.Count > 0)
            //return a name among the valid hints
            return availableHintNames[(int)UnityEngine.Random.Range(0, availableHintNames.Count - 0.001f)];
        else
            return "";
    }


    /// <summary>
    /// Check if at least one of the hints contains at least one formulation not already included into hints displayed to the player
    /// </summary>
    /// <param name="hintsCandidate"></param>
    /// <returns></returns>
    private bool containsUniqueHint(List<KeyValuePair<string, List<string>>> hintsCandidate)
    {
        foreach (KeyValuePair<string, List<string>> pair in hintsCandidate)
        {
            bool isUnique = true;
            foreach (GameObject go in f_enabledHintsIAR)
                if (pair.Value.Contains(go.GetComponent<HintContent>().text))
                {
                    isUnique = false;
                    break;
                }
            if (isUnique)
                return true;
        }
        return false;
    }

    private List<int> GetNextActions(int room)
    {
        List<KeyValuePair<ComponentMonitoring, string>> triggerableActions = MonitoringManager.getTriggerableActions();
        if (!rooms2Enigmas.ContainsKey(room))
        {
            Debug.LogWarning("Room " + room + " doesn't contain any petri net.");
            return new List<int>();
        }
        // Get list of petri nets associated to this room
        Dictionary<int, ComponentMonitoring> pn2cm = rooms2Enigmas[room];

        List<int> actionsIDs = new List<int>();
        
        // Parse all triggerable actions
        for (int i = 0; i < triggerableActions.Count; i++)
        {
            // Check if this action is part of the set of petri nets of this room, if not ignore this action because it is not linked to the room we are interested in
            int pn = triggerableActions[i].Key.fullPnSelected;
            if (pn2cm.ContainsKey(pn))
            {
                // Check if Meta ComponentMonitoring associated to the petri net of the current action is steal fireable, if not ingore this action because it is part of a solved enigma
                // Parse all triggerable action
                for (int iMeta = 0; iMeta < triggerableActions.Count; iMeta++)
                {
                    // filter only those that are part of meta petri net
                    if (triggerableActions[iMeta].Key.fullPnSelected == 0)
                    {
                        // Check if ComponentMonitoring of this meta action is the one expected for the petri net of the action to filter (the one of the first loop)
                        if (triggerableActions[iMeta].Key.id == pn2cm[pn].id)
                        {
                            actionsIDs.Add(triggerableActions[i].Key.id);
                            break;
                        }
                    }
                }
            }
        }

        return actionsIDs;
    }
}