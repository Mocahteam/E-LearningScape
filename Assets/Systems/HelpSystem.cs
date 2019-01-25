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
    private Family f_askHelpButton = FamilyManager.getFamily(new AnyOfTags("AskHelpButton"), new AllOfComponents(typeof(Button)));
    private Family f_newHint = FamilyManager.getFamily(new AllOfComponents(typeof(NewHint)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));

    private Family f_scrollView = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(PrefabContainer)));
    private Family f_description = FamilyManager.getFamily(new AnyOfTags("HelpDescriptionUI"));

    private Family f_debugDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(DebugDisplayer)));
    private DebugDisplayer debugDisplayer;
    private Family f_wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));

    private GameHints gameHints;
    private InternalGameHints internalGameHints;
    private Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>> initialGameHints;
    //key: ComponentMonitoring id
    //value: list of hints names in gameHints.dictionary corresponding to the ComponentMonitoring
    private Dictionary<int, List<string>> availableComponentMonitoringIDs;
    private string highlightTag = "##HL##";

    private float sessionDuration = 3600; //in Seconds
    private float totalWeightedMetaActions = 0;
    private float playerHintCooldownDuration = 300;
    private float systemHintCooldownDuration = 15;
    private float playerHintTimer = float.MinValue;
    private RectTransform cooldownRT;
    private TextMeshProUGUI cooldownText;
    private float cooldownInitialWidth;
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

    private RectTransform scrollViewContent;
    private TextMeshProUGUI hintTitle;
    private TextMeshProUGUI hintText;
    private Button hintLinkButton;
    private string hintLink;
    private GameObject hintButtonPrefab;
    private List<GameObject> hintButtonsPool;
    private Button selectedHint = null;
    private ColorBlock colorHint;
    private ColorBlock colorNewHint;
    private ColorBlock colorSelectedHint;

    private GameObject tmpGO;
    private RectTransform tmpRT;
    private HintContent tmpHC;
    private KeyValuePair<string, List<string>> tmpPair;

    //store the gameObject, the name and the overrideName of the ActionPerformed
    //and processes it after the system ActionManager processed all ActionPerformed of the gameobject
    //Dictionary<GameObject, List<KeyValuePair<name, overrideName>>>
    private Dictionary<GameObject, List<KeyValuePair<string, string>>> actionPerformedHistory;

    public static HelpSystem instance;
    public static bool shouldPause = true;

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
                    gameHints.dictionary[key1].Add(key2, new KeyValuePair<string, List<string>>("www.google.fr", internalGameHints.dictionary[key1][key2]));
                }
            }
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
            int nbHints = 0;
            foreach (string key in gameHints.dictionary.Keys)
                nbHints += gameHints.dictionary[key].Keys.Count;

            if (LoadGameContent.gameContent.virtualPuzzle)
                RemoveHintsByPN("Enigma11_2");
            else
                RemoveHintsByPN("Enigma11_1");

            sessionDuration = LoadGameContent.gameContent.sessionDuration * 60; //convert to seconds

            room = f_unlockedRoom.First().GetComponent<UnlockedRoom>();
            
            subtitles = f_subtitlesFamily.First().GetComponent<TextMeshProUGUI>();
            correctLabels = new List<string>() { "correct" };
            errorLabels = new List<string>() { "useless", "erroneous", "too-early" };
            otherLabels = new List<string>() { "stagnation", "already-seen" };

            weights = LoadGameContent.enigmasWeight;
            foreach (string enigmaName in weights.Keys)
                totalWeightedMetaActions += weights[enigmaName];

            scrollViewContent = f_scrollView.First().transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            hintTitle = f_description.First().transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            hintText = f_description.First().transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            hintLinkButton = f_description.First().transform.GetChild(2).GetComponent<Button>();
            hintLinkButton.onClick.AddListener(OnClickHintLinkButton);
            hintButtonPrefab = f_scrollView.First().GetComponent<PrefabContainer>().prefab;
            //create a pool of int button right at the beginning and activate them when necessary rather than creating them during the game
            if (!shouldPause)
            {
                hintButtonsPool = new List<GameObject>();
                GameObject tmpGo;
                Button b;
                for (int i = 0; i < 300; i++)
                {
                    tmpGo = GameObject.Instantiate(hintButtonPrefab);
                    tmpGo.transform.SetParent(scrollViewContent.transform);
                    tmpGo.SetActive(false);
                    GameObjectManager.bind(tmpGo);
                    hintButtonsPool.Add(tmpGo);
                }
            }

            f_traces.addEntryCallback(OnNewTraces);
            f_actions.addEntryCallback(OnNewActionPerformed);
            f_actionsProcessed.addEntryCallback(OnActionsProcessed);
            f_askHelpButton.First().GetComponent<Button>().onClick.AddListener(OnPlayerAskHelp);
            cooldownRT = f_askHelpButton.First().transform.GetChild(1).GetComponent<RectTransform>();
            cooldownInitialWidth = cooldownRT.sizeDelta.x;
            cooldownText = f_askHelpButton.First().transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            actionPerformedHistory = new Dictionary<GameObject, List<KeyValuePair<string, string>>>();

            colorHint = new ColorBlock();
            colorHint.normalColor =  new Color(189,244,255,255) / 256;
            colorHint.highlightedColor = new Color(137,235,255,255) / 256;
            colorHint.pressedColor = new Color(98,182,199,255) / 256;
            colorHint.disabledColor = new Color(137, 235, 255, 128) / 256;
            colorHint.colorMultiplier = 1;
            colorNewHint = new ColorBlock();
            colorNewHint.normalColor = new Color(254,255,189,255) / 256;
            colorNewHint.highlightedColor = new Color(248,255,137,255) / 256;
            colorNewHint.pressedColor = new Color(199,192,98,255) / 256;
            colorNewHint.disabledColor = new Color(253,255,137,128) / 256;
            colorNewHint.colorMultiplier = 1;
            colorSelectedHint = ColorBlock.defaultColorBlock;

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
        this.Pause = shouldPause;
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

        if (Time.time - playerHintTimer < playerHintCooldownDuration)
        {
            cooldownRT.offsetMax = new Vector2(-(160 * (Time.time - playerHintTimer) / playerHintCooldownDuration), cooldownRT.offsetMax.y);
            float timeLeft = playerHintCooldownDuration - (Time.time - playerHintTimer);
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
            GameObjectManager.setGameObjectState(cooldownRT.gameObject, false);
            cooldownText.text = "";
        }
        #region Debug update
        debugDisplayer.timer = Time.time - f_timer.First().GetComponent<Timer>().startingTime;
        debugDisplayer.playerHintTimer = Time.time - playerHintTimer;
        debugDisplayer.systemHintTimer = Time.time - systemHintTimer;
        debugDisplayer.labelCount = labelCount;
        debugDisplayer.nbFeedBackGiven = nbFeedBackGiven;
        //debugDisplayer.enigmaRatio = (totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions;
        debugDisplayer.timeRatio = (Time.time - f_timer.First().GetComponent<Timer>().startingTime) / sessionDuration;
        debugDisplayer.progressionRatio = debugDisplayer.enigmaRatio / debugDisplayer.timeRatio;
        debugDisplayer.availableComponentMonitoringIDs.Clear();
        debugDisplayer.availableComponentMonitoringIDs.AddRange(availableComponentMonitoringIDs.Keys);
        #endregion
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
                        float numberFeedbackExpected = (sessionDuration - (Time.time - f_timer.First().GetComponent<Timer>().startingTime)) * nbFeedBackGiven / (Time.time - f_timer.First().GetComponent<Timer>().startingTime);
                        float feedbackRatio = numberFeedbackExpected / GetNumberFeedbackLeft();

                        int feedbackLevel = 2;
                        if (feedbackRatio < feedbackStep1)
                            feedbackLevel = 1;
                        else if (feedbackRatio > feedbackStep2)
                            feedbackLevel = 3;

                        if (labelCount > errorStep)
                            DisplayHint(room.roomNumber, feedbackLevel);
                    }
                    else if (otherLabels.Contains(tmpTrace.labels[j]))
                    {
                        labelCount += otherCoef * progressionRatio;
                        float numberFeedbackExpected = (sessionDuration - (Time.time - f_timer.First().GetComponent<Timer>().startingTime)) * nbFeedBackGiven / (Time.time - f_timer.First().GetComponent<Timer>().startingTime);
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
                        //if (!allRemoved)
                        //    Debug.LogWarning(string.Concat("Something went wrong with the removing of hints linked to the ComponentMonitoring with the id ", cm.Key.id, ".",
                        //        System.Environment.NewLine, "Either the id isn't in the dictionary \"availableComponentMonitoringIDs\" or one of the hints isn't in \"gameHints.dictionary\"."));
                    }
                    //if action performed is a player objective/end action, remove all hints linked to monitor of the same Petri net
                    if (cm.Value.isEndAction)
                        RemoveHintsByPN(cm.Key.fullPnSelected);
                }
            }
        }
    }

    private void OnClickHint(Button b)
    {
        if (selectedHint)
            selectedHint.colors = colorHint;
        selectedHint = b;
        selectedHint.colors = colorSelectedHint;
        tmpHC = selectedHint.GetComponent<HintContent>();
        string[] tmpStringArray = tmpHC.hintName.Split('.');
        string buttonName = tmpStringArray[0];
        for (int i = 1; i < tmpStringArray.Length - 1; i++)
            buttonName = string.Concat(buttonName, ".", tmpStringArray[i]);
        hintTitle.text = buttonName;
        hintText.text = tmpHC.text;
        if(tmpHC.link != "")
        {
            //display link button
            hintLink = tmpHC.link;
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, true);
        }
        else
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, false);
        GameObjectManager.removeComponent<NewHint>(selectedHint.gameObject);
    }

    private void OnPlayerAskHelp()
    {
        //check cooldown before sending hint
        if(Time.time - playerHintTimer > playerHintCooldownDuration)
        {
            float numberFeedbackExpected = (sessionDuration - (Time.time - f_timer.First().GetComponent<Timer>().startingTime)) * nbFeedBackGiven / (Time.time - f_timer.First().GetComponent<Timer>().startingTime);
            float feedbackRatio = numberFeedbackExpected / GetNumberFeedbackLeft();

            int feedbackLevel = 2;
            if (feedbackRatio < feedbackStep1)
                feedbackLevel = 1;
            else if (feedbackRatio > feedbackStep2)
                feedbackLevel = 3;
            if (DisplayHint(room.roomNumber, feedbackLevel))
            {
                playerHintTimer = Time.time;
                GameObjectManager.setGameObjectState(cooldownRT.gameObject, true);
            }
        }
    }

    private void OnClickHintLinkButton()
    {
        Application.OpenURL(hintLink);
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
                }
                allRemoved = allRemoved ? wordRemoved : false;
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

    private bool DisplayHint(int room, int feedbackLevel)
    {
        int availableFeedback = CheckAvailableFeedback(room, feedbackLevel);
        if(availableFeedback != -1)
        {
            string hintName = GetHintName(room, availableFeedback);
            if (hintName != "")
            {
                string key1 = string.Concat(room, ".", availableFeedback);

                tmpPair = gameHints.dictionary[key1][hintName];

                bool hasHighlightTag = false;
                if (hintName.Substring(0, highlightTag.Length) == highlightTag)
                {
                    hasHighlightTag = true;
                    //if hint name starts with the highlight tag, highlight the gameobject
                    hintName = hintName.Remove(0, highlightTag.Length);
                }

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

                //show a button to see hint content in hint tab in IAR
                if (hintButtonsPool.Count == 0)
                {
                    //create a new hint button if pool is empty
                    GameObject tmpGo = GameObject.Instantiate(hintButtonPrefab);
                    tmpGo.transform.SetParent(scrollViewContent.transform);
                    tmpGo.SetActive(false);
                    GameObjectManager.bind(tmpGo);
                    Button b = tmpGo.GetComponent<Button>();
                    b.onClick.AddListener(delegate { OnClickHint(b); });
                    hintButtonsPool.Add(tmpGo);

                    Debug.LogWarning("You should increase hintButtonsPool initial size");
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - You should increase hintButtonsPool initial size."));
                }

                tmpGO = hintButtonsPool[0];
                hintButtonsPool.RemoveAt(0);
                Button hintButton = tmpGO.GetComponent<Button>();
                string buttonName = tmpStringArray[0];
                for (int i = 1; i < tmpStringArray.Length - 1; i++)
                    buttonName = string.Concat(buttonName, ".", tmpStringArray[i]);
                tmpGO.transform.GetChild(0).GetComponent<Text>().text = buttonName;
                hintButton.colors = colorNewHint;
                GameObjectManager.setGameObjectState(tmpGO, true);
                int nbActivatedHint = scrollViewContent.GetComponentsInChildren<Button>().Length;
                scrollViewContent.sizeDelta = new Vector2(scrollViewContent.sizeDelta.x, (nbActivatedHint + 1) * hintButton.GetComponent<RectTransform>().sizeDelta.y);
                tmpRT = tmpGO.GetComponent<RectTransform>();
                tmpRT.localScale = Vector3.one;
                tmpRT.offsetMin = new Vector2(0, tmpRT.offsetMin.y);
                tmpRT.offsetMax = new Vector2(0, tmpRT.offsetMax.y);
                tmpRT.anchoredPosition = new Vector2(0, -(nbActivatedHint + 0.5f) * hintButton.GetComponent<RectTransform>().sizeDelta.y);
                tmpHC = tmpGO.GetComponent<HintContent>();
                tmpHC.hintName = hintName;
                int nbHintTexts = tmpPair.Value.Count;
                if (nbHintTexts > 0)
                {
                    tmpHC.text = tmpPair.Value[(int)UnityEngine.Random.Range(0, nbHintTexts - 0.01f)];
                    //change subtitle text tot display the hint
                    //subtitles.text = tmpPair.Value[(int)UnityEngine.Random.Range(0, nbHintTexts - 0.01f)];
                    //GameObjectManager.setGameObjectState(subtitles.gameObject, true);
                    //subtitlesTimer = Time.time;
                    tmpHC.link = tmpPair.Key;
                }
                if (id != -1)
                    tmpHC.monitor = MonitoringManager.getMonitorById(id);
                hintButton.onClick.AddListener(delegate { OnClickHint(hintButton); });

                //remove hint from dictionary
                if(hasHighlightTag)
                    gameHints.dictionary[key1].Remove(string.Concat(highlightTag, hintName));
                else
                    gameHints.dictionary[key1].Remove(hintName);

                systemHintTimer = Time.time;
                nbFeedBackGiven++;
                return true;
            }
            else
            {
                Debug.Log("No hint found.");
                return false;
            }
        }
        else
        {
            Debug.Log(string.Concat("No hint found for the room ", room));
            return false;
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

        if (availableHintNames.Count > 0)
            //return a name among the valid hints
            return availableHintNames[(int)UnityEngine.Random.Range(0, availableHintNames.Count - 0.001f)];
        else
            return "";
    }

    private List<int> GetNextActions(int room)
    {
        List<KeyValuePair<ComponentMonitoring, string>> triggerableActions = MonitoringManager.getTriggerableActions();
        List<int> rdpIDs = null;

        //get Petri nets ids depending on the room selected
        switch (room)
        {
            //Ids in rpdIDs are the ids in PetriNetNames of MonitoringManager (Visible in inspector)
            case 0:
                rdpIDs = new List<int>() { 1 };
                break;
            case 1:
                rdpIDs = new List<int>() { 2, 3, 4, 16, 17 };
                break;
            case 2:
                rdpIDs = new List<int>() { 5, 6, 7, 8, 9, 10, 18 };
                break;

            case 3:
                rdpIDs = new List<int>() { 11, 12, 13, 14, 15 };
                break;

            default:
                rdpIDs = new List<int>();
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