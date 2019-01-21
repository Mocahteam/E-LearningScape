using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Newtonsoft.Json;

public class HelpSystem : FSystem {

    private Family f_subtitlesFamily = FamilyManager.getFamily(new AnyOfTags("HelpSubtitles"), new AllOfComponents(typeof(TextMeshProUGUI)));
    private Family f_traces = FamilyManager.getFamily(new AllOfComponents(typeof(Trace)));
    private Family f_gameHints = FamilyManager.getFamily(new AllOfComponents(typeof(GameHints)));
    private Family f_internalGameHints = FamilyManager.getFamily(new AllOfComponents(typeof(InternalGameHints)));
    private Family f_defaultGameContent = FamilyManager.getFamily(new AllOfComponents(typeof(DefaultGameContent)));
    private Family f_timer = FamilyManager.getFamily(new AllOfComponents(typeof(Timer)));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));
    private Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformed)));
    private Family f_actionsProcessed = FamilyManager.getFamily(new NoneOfComponents(typeof(ActionPerformed)));
    private Family f_componentMonitoring = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));

    private Family f_debugDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(DebugDisplayer)));
    private DebugDisplayer debugDisplayer;

    private GameHints gameHints;
    private InternalGameHints internalGameHints;
    private Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>> initialGameHints;
    //key: ComponentMonitoring id
    //value: list of hints names in gameHints.dictionary corresponding to the ComponentMonitoring
    private Dictionary<int, List<string>> availableComponentMonitoringIDs;
    private string highlightTag = "##HL##";

    private float sessionDuration = 3600; //in Seconds
    private Timer timer;
    private float totalWeightedMetaActions = 0;
    private float playerHintCooldownDuration = 300;
    private float systemHintCooldownDuration = 15;
    private float playerHintTimer = float.MinValue;
    private float systemHintTimer = float.MinValue;

    private float labelCount = 0;
    private int nbFeedBackGiven = 0;
    private float feedbackStep1 = 0.75f;
    private float feedbackStep2 = 2;
    private List<string> correctLabels;
    private float correctCoef = -2;
    private List<string> errorLabels;
    private float errorCoef = 2;
    private List<string> otherLabels;
    private float otherCoef = 1;
    private float errorStep = 50;
    private float otherStep = 75;

    private UnlockedRoom room;

    private TextMeshProUGUI subtitles;
    private float subtitlesTimer = float.MinValue;

    private float noActionTimer = float.MaxValue;

    private ComponentMonitoring finalComponentMonitoring;

    private Dictionary<string, float> weights;

    //store the gameObject, the name and the overrideName of the ActionPerformed
    //and processes it after the system ActionManager processed all ActionPerformed of the gameobject
    //Dictionary<GameObject, List<KeyValuePair<name, overrideName>>>
    private Dictionary<GameObject, List<KeyValuePair<string, string>>> actionPerformedHistory;

    public static HelpSystem instance;

    public HelpSystem()
    {
        if (Application.isPlaying)
        {
            //get game hints filled with tips loaded from "Data/Hints_LearningScape.txt"
            gameHints = f_gameHints.First().GetComponent<GameHints>();
            //get internal game hints
            internalGameHints = f_internalGameHints.First().GetComponent<InternalGameHints>();
            //Get the last monitor in the meta Petri net
            finalComponentMonitoring = MonitoringManager.getMonitorById(164);
            
            //add internal game hints to the dictionary of the component GameHints
            foreach(string key1 in internalGameHints.dictionary.Keys)
            {
                if (!gameHints.dictionary.ContainsKey(key1))
                    gameHints.dictionary.Add(key1, new Dictionary<string, KeyValuePair<string, List<string>>>());
                foreach (string key2 in internalGameHints.dictionary[key1].Keys)
                {
                    gameHints.dictionary[key1].Add(string.Concat(highlightTag, key2), new KeyValuePair<string, List<string>>("", internalGameHints.dictionary[key1][key2]));
                }
            }
            if (LoadGameContent.gameContent.virtualPuzzle)
                RemoveHintsByPN("Enigma11_2");
            else
                RemoveHintsByPN("Enigma11_1");
            initialGameHints = new Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>(gameHints.dictionary);

            availableComponentMonitoringIDs = new Dictionary<int, List<string>>();
            foreach(string key1 in gameHints.dictionary.Keys)
                foreach(string key2 in gameHints.dictionary[key1].Keys)
                {
                    int id = -1;
                    string[] tmpStringArray = key2.Split('.');
                    try
                    {
                        id = int.Parse(tmpStringArray[tmpStringArray.Length - 1]);
                    }
                    catch (System.Exception)
                    {
                    }
                    if (id != -1)
                    {
                        //initialize availableComponentMonitoringIDs
                        if (availableComponentMonitoringIDs.ContainsKey(id))
                        {
                            if (availableComponentMonitoringIDs[id] == null)
                                availableComponentMonitoringIDs[id] = new List<string>();
                            availableComponentMonitoringIDs[id].Add(key2);
                        }
                        else
                            availableComponentMonitoringIDs.Add(id, new List<string>() { key2 });
                    }
                }

            sessionDuration = LoadGameContent.gameContent.sessionDuration * 60; //convert to seconds
            timer = f_timer.First().GetComponent<Timer>();

            room = f_unlockedRoom.First().GetComponent<UnlockedRoom>();
            
            subtitles = f_subtitlesFamily.First().GetComponent<TextMeshProUGUI>();
            correctLabels = new List<string>() { "correct" };
            errorLabels = new List<string>() { "useless", "erroneous", "too-early" };
            otherLabels = new List<string>() { "stagnation", "already-seen" };

            weights = LoadGameContent.enigmasWeight;
            foreach (string enigmaName in weights.Keys)
                totalWeightedMetaActions += weights[enigmaName];

            f_traces.addEntryCallback(OnNewTraces);
            f_actions.addEntryCallback(OnNewActionPerformed);
            f_actionsProcessed.addEntryCallback(OnActionsProcessed);

            #region Debug Init
            debugDisplayer = f_debugDisplayer.First().GetComponent<DebugDisplayer>();
            debugDisplayer.totalWeightedMetaActions = totalWeightedMetaActions;
            debugDisplayer.feedbackStep1 = feedbackStep1;
            debugDisplayer.feedbackStep2 = feedbackStep2;
            debugDisplayer.correctCoef = correctCoef;
            debugDisplayer.errorCoef = errorCoef;
            debugDisplayer.otherCoef = otherCoef;
            debugDisplayer.errorStep = errorStep;
            debugDisplayer.otherStep = otherStep;
            debugDisplayer.availableComponentMonitoringIDs = new List<int>();
            #endregion
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
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        if (subtitles.gameObject.activeSelf && Time.time - subtitlesTimer > 2)
        {
            subtitles.text = string.Empty;
            GameObjectManager.setGameObjectState(subtitles.gameObject, false);
        }

        //increase labelCount if the player isn't doing anything
        if(Time.time - noActionTimer > 10)
        {
            noActionTimer = Time.time;
            //(nb enigma done / total nb enigma) / (current time / total duration)
            float progressionRatio = ((totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions) / ((Time.time - f_timer.First().GetComponent<Timer>().startingTime) / sessionDuration);
            labelCount += otherCoef * progressionRatio;

        }
        #region Debug update
        debugDisplayer.timer = Time.time - timer.startingTime;
        debugDisplayer.playerHintTimer = Time.time - playerHintTimer;
        debugDisplayer.systemHintTimer = Time.time - systemHintTimer;
        debugDisplayer.labelCount = labelCount;
        debugDisplayer.nbFeedBackGiven = nbFeedBackGiven;
        debugDisplayer.enigmaRatio = (totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions;
        debugDisplayer.timeRatio = (Time.time - f_timer.First().GetComponent<Timer>().startingTime) / sessionDuration;
        debugDisplayer.progressionRatio = debugDisplayer.enigmaRatio / debugDisplayer.timeRatio;
        debugDisplayer.availableComponentMonitoringIDs.Clear();
        debugDisplayer.availableComponentMonitoringIDs.AddRange(availableComponentMonitoringIDs.Keys);
        #endregion
    }

    private void OnNewActionPerformed(GameObject go)
    {
        ActionPerformed[] actions = go.GetComponents<ActionPerformed>();
        if (!actionPerformedHistory.ContainsKey(go))
            actionPerformedHistory.Add(go, new List<KeyValuePair<string, string>>());
        for (int i = 0; i < actions.Length; i++)
        {
            ActionPerformed ap = actions[i];
            actionPerformedHistory[go].Add(new KeyValuePair<string, string>(ap.name, ap.overrideName));
        }
    }

    private void OnActionsProcessed(GameObject go)
    {
        if (actionPerformedHistory.ContainsKey(go))
        {
            foreach(KeyValuePair<string, string> pair in actionPerformedHistory[go])
            {
                List<KeyValuePair<ComponentMonitoring, TransitionLink>> cMonitors = FindMonitors(go, pair.Key, pair.Value);
                foreach (KeyValuePair < ComponentMonitoring, TransitionLink > cm in cMonitors)
                {
                    //delete hints linked to the ComponentMonitoring if it is not reachable anymore
                    bool reachable = false;
                    foreach(TransitionLink tl in cm.Key.transitionLinks)
                    {
                        if(MonitoringManager.getNextActionsToReach(cm.Key, tl.transition.label, int.MaxValue).Count > 0)
                        {
                            reachable = true;
                            break;
                        }
                    }
                    if (!reachable)
                    {
                        bool allRemoved = RemoveHintsByComponentID(cm.Key.id);
                        if (!allRemoved)
                            Debug.LogWarning(string.Concat("Something went wrong with the removing of hints linked to the ComponentMonitoring with the id ", cm.Key.id, ".",
                                System.Environment.NewLine, "Eiher the id isn't in the dictionary \"availableComponentMonitoringIDs\" or one of the hints isn't in \"gameHints.dictionary\"."));
                    }
                    //if action performed is a player objective/end action, remove all hints linked to monitor of the same Petri net
                    if (cm.Value.isEndAction)
                        RemoveHintsByPN(cm.Key.fullPnSelected);
                }
            }
        }
    }

    /// <summary>
    /// Remove from gameHints.dictionary all hints linked to the given id.
    /// Return false if the id doesn't exist or if one of the hint name in availableComponentMonitoringIDs[id] wasn't found in the dictionary.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool RemoveHintsByComponentID(int id)
    {
        if (availableComponentMonitoringIDs.ContainsKey(id))
        {
            List<string> nameList = availableComponentMonitoringIDs[id];
            bool allRemoved = true;
            foreach(string name in nameList)
            {
                bool wordRemoved = false;
                foreach (string key in gameHints.dictionary.Keys)
                {
                    if (gameHints.dictionary[key].ContainsKey(name))
                    {
                        gameHints.dictionary[key].Remove(name);
                        wordRemoved = true;
                        break;
                    }
                    allRemoved = allRemoved ? wordRemoved : false;
                }
            }
            return allRemoved;
        }
        else
            return false;
    }

    private void RemoveHintsByPN(int id)
    {
        int nbComponentMonitoringGO = f_componentMonitoring.Count;
        ComponentMonitoring[] monitors = null;
        int nbMonitors;
        for(int i = 0; i < nbComponentMonitoringGO; i++)
        {
            monitors = f_componentMonitoring.getAt(i).GetComponents<ComponentMonitoring>();
            nbMonitors = monitors.Length;
            for(int j = 0; j < nbMonitors; j++)
                if (monitors[j].fullPnSelected == id)
                    RemoveHintsByComponentID(monitors[j].id);
        }
    }

    private void RemoveHintsByPN(string name)
    {
        int nbComponentMonitoringGO = f_componentMonitoring.Count;
        ComponentMonitoring[] monitors = null;
        int nbMonitors;
        for (int i = 0; i < nbComponentMonitoringGO; i++)
        {
            monitors = f_componentMonitoring.getAt(i).GetComponents<ComponentMonitoring>();
            nbMonitors = monitors.Length;
            for (int j = 0; j < nbMonitors; j++)
                if (MonitoringManager.Instance.PetriNetsName[monitors[j].fullPnSelected] == name)
                    RemoveHintsByComponentID(monitors[j].id);
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

    private void OnNewTraces(GameObject go)
    {
        noActionTimer = Time.time;

        if (Time.time - systemHintTimer > systemHintCooldownDuration)
        {
            float enigmaProgression = (totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions;
            float timeProgression = (Time.time - f_timer.First().GetComponent<Timer>().startingTime) / sessionDuration;
            //(nb enigma done / total nb enigma) / (current time / total duration)
            float progressionRatio = enigmaProgression / timeProgression;

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
                for (int j = 0; j < nbLabels; j++)
                {
                    if (correctLabels.Contains(tmpTrace.labels[j]))
                        labelCount += correctCoef / progressionRatio;
                    else if (errorLabels.Contains(tmpTrace.labels[j]))
                    {
                        labelCount += errorCoef * progressionRatio;
                        float numberFeedbackExpected = (sessionDuration - (Time.time - timer.startingTime)) * nbFeedBackGiven / (Time.time - timer.startingTime);
                        float feedbackRatio = numberFeedbackExpected / GetNumberFeedbackLeft();

                        int feedbackLevel = 2;
                        if (feedbackRatio < feedbackStep1)
                            feedbackLevel = 1;
                        else if (feedbackRatio > feedbackStep2)
                            feedbackLevel = 3;

                        if(labelCount > errorStep)
                            DisplayHint(room.roomNumber, feedbackLevel);
                    }
                    else if (otherLabels.Contains(tmpTrace.labels[j]))
                    {
                        labelCount += otherCoef * progressionRatio;
                        float numberFeedbackExpected = (sessionDuration - (Time.time - timer.startingTime)) * nbFeedBackGiven / (Time.time - timer.startingTime);
                        float feedbackRatio = numberFeedbackExpected / GetNumberFeedbackLeft();

                        int feedbackLevel = 2;
                        if (feedbackRatio < feedbackStep1)
                            feedbackLevel = 1;
                        else if (feedbackRatio > feedbackStep2)
                            feedbackLevel = 3;

                        if (labelCount > otherStep)
                            DisplayHint(room.roomNumber, feedbackLevel);
                    }
                }
            }
        }
    }

    private void DisplayHint(int room, int feedbackLevel)
    {
        int availableFeedback = CheckAvailableFeedback(room, feedbackLevel);
        if(availableFeedback != -1)
        {
            string hintName = GetHintName(room, availableFeedback);
            string key1 = string.Concat(room, ".", availableFeedback);

            int nbHintTexts = gameHints.dictionary[key1][hintName].Value.Count;
            if (nbHintTexts > 0)
            {
                //change subtitle text tot display the hint
                subtitles.text = gameHints.dictionary[key1][hintName].Value[(int)Random.Range(0, nbHintTexts - 0.01f)];
                GameObjectManager.setGameObjectState(subtitles.gameObject, true);
                subtitlesTimer = Time.time;
            }

            if (hintName.Substring(0, highlightTag.Length) == highlightTag)
            {
                //if hint name starts with the highlight tag, highlight the gameobject
            }

            //remove hint from dictionary
            gameHints.dictionary[key1].Remove(hintName);
            //remove hint from availableComponentMonitoringIDs
            int id = -1;
            string[] tmpStringArray = hintName.Split('.');
            try
            {
                id = int.Parse(tmpStringArray[tmpStringArray.Length - 1]);
            }
            catch (System.Exception)
            {
            }
            if (availableComponentMonitoringIDs.ContainsKey(id) && availableComponentMonitoringIDs[id].Contains(hintName))
            {
                availableComponentMonitoringIDs[id].Remove(hintName);
                if (availableComponentMonitoringIDs[id].Count == 0)
                    availableComponentMonitoringIDs.Remove(id);
            }

            systemHintTimer = Time.time;
            nbFeedBackGiven++;
        }
        else
        {
            Debug.Log(string.Concat("No hint found for the room ", room));
        }
    }

    /// <summary>
    /// Checks if there are available hints for the room and the level given
    /// If there are it returns the feedback level given
    /// Else it looks for another feedback level containing hints for the given room looking first in the levels under the given one, and returns it
    /// If there aren't any hint for the given room, returns -1
    /// </summary>
    /// <param name="room"></param>
    /// <param name="feedbackLevel"></param>
    /// <returns></returns>
    private int CheckAvailableFeedback(int room, int feedbackLevel)
    {
        if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", feedbackLevel)))
            return feedbackLevel;
        else
        {
            if(feedbackLevel == 1)
            {
                if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 2)))
                    return 2;
                else if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 3)))
                    return 3;
                else return -1;
            }
            else if (feedbackLevel == 2)
            {
                if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 1)))
                    return 1;
                else if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 3)))
                    return 3;
                else return -1;
            }
            else if (feedbackLevel == 3)
            {
                if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 1)))
                    return 1;
                else if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 2)))
                    return 2;
                else return -1;
            }
            else
            {
                if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 1)))
                    return 1;
                else if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 2)))
                    return 2;
                else if (gameHints.dictionary.ContainsKey(string.Concat(room, ".", 3)))
                    return 3;
                else return -1;
            }
        }
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
                if (weight >= 0)
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
        string key = "";

        if (monitor.gameObject.tag.Contains("Q-R"))
        {
            int questionID = int.Parse(string.Concat(monitor.gameObject.tag[monitor.gameObject.tag.Length-1], monitor.gameObject.name[monitor.gameObject.name.Length-1]));
            switch (questionID)
            {
                case 11:
                    key = "ballBox";
                    break;

                case 12:
                    key = "plankAndWire";
                    break;

                case 13:
                    key = "greenFragments";
                    break;

                case 21:
                    key = "glasses";
                    break;

                case 22:
                    key = "enigma6";
                    break;

                case 23:
                    key = "scrolls";
                    break;

                case 24:
                    key = "mirror";
                    break;

                case 25:
                    key = "enigma9";
                    break;

                case 26:
                    key = "enigma10";
                    break;

                default:
                    return 1;
            }
        }
        else if(monitor.gameObject.name == "AnswersInput")
        {
            foreach(TransitionLink tl in monitor.transitionLinks)
            {
                if(tl.transition.overridedLabel == "solvePuzzle")
                {
                    key = "puzzle";
                    break;
                }
                else if (tl.transition.overridedLabel == "solveLamp")
                {
                    key = "lamp";
                    break;
                }
                else if (tl.transition.overridedLabel == "solveEnigma13")
                {
                    key = "enigma13";
                    break;
                }
                else if (tl.transition.overridedLabel == "solveWhiteBoard")
                {
                    key = "whiteBoard";
                    break;
                }
            }
            if (key == "")
                return 1;
        }
        else if (monitor.gameObject.tag == "Login")
            key = "loginPanel";
        else if (monitor.gameObject.tag == "Gears")
            key = "gearsEnigma";
        else if (monitor.gameObject.tag == "LockRoom2")
            return 0;

        if (weights.ContainsKey(key))
            return weights[key];
        else
            return 1;
    } 

    /// <summary>
    /// Return the name of a randomly selected hint among hints corresponding to the room and the feedback level given
    /// </summary>
    /// <param name="room"></param>
    /// <param name="feedbackLevel"></param>
    /// <returns></returns>
    private string GetHintName(int room, int feedbackLevel)
    {
        List<int> nextActions = GetNextActions(room);
        List<string> availableHintNames = new List<string>();

        string key1 = string.Concat(room, ".", feedbackLevel);
        string[] splitedHintName = null;
        int id = -1;

        if (gameHints.dictionary.ContainsKey(key1))
        {
            //process all hints corresponding to the room and the feedback level given
            foreach (string hintName in gameHints.dictionary[key1].Keys)
            {
                splitedHintName = hintName.Split('.');
                if (int.TryParse(splitedHintName[splitedHintName.Length - 1], out id))
                    //add the hint name in availableHintNames if the id correspond to a next action
                    if (nextActions.Contains(id))
                        availableHintNames.Add(hintName);
            }
        }
        else
            return "";

        //return a name among the valid hints
        return availableHintNames[(int)Random.Range(0, availableHintNames.Count - 0.001f)];
    }

    private List<int> GetNextActions(int room)
    {
        List<KeyValuePair<ComponentMonitoring, string>> triggerableActions = MonitoringManager.getTriggerableActions();
        List<int> rdpIDs = null;

        //get Petri nets ids depending on the room selected
        switch (room)
        {
            case 2:
                rdpIDs = new List<int>() { 0, 5, 6, 7, 8, 9, 10 };
                break;

            case 3:
                rdpIDs = new List<int>() { 11, 12, 13, 14, 15 };
                break;

            default:
                rdpIDs = new List<int>() { 0, 2, 3, 4 };
                break;
        }

        List<int> actionsIDs = new List<int>();
        
        for(int i = 0; i < triggerableActions.Count; i++)
        {
            //add the ComponentMonitoring ID if his Petri net is in the list
            if (rdpIDs.Contains(triggerableActions[i].Key.fullPnSelected))
                actionsIDs.Add(triggerableActions[i].Key.id);
        }

        return actionsIDs;
    }

    /// <summary>
    /// Return the number of feedback left in gamehints.dictionary
    /// A feedback is removed from the dictionary when it is used, or when the enigma its ComponentMonitoring belongs to is solved
    /// </summary>
    /// <returns></returns>
    private int GetNumberFeedbackLeft()
    {
        int numberFeedback = 0;
        foreach (string key in gameHints.dictionary.Keys)
            numberFeedback += gameHints.dictionary[key].Count;
        return numberFeedback;
    }
}