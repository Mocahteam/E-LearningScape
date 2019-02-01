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
    private Family f_labelWeights = FamilyManager.getFamily(new AllOfComponents(typeof(LabelWeights)));
    private Family f_wrongAnswerInfo = FamilyManager.getFamily(new AllOfComponents(typeof(WrongAnswerInfo)));

    private Family f_scrollView = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(PrefabContainer)));
    private Family f_description = FamilyManager.getFamily(new AnyOfTags("HelpDescriptionUI"));

    private Family f_wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"));

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
    /// key: ComponentMonitoring id,
    /// value: list of hints names in gameHints.dictionary corresponding to the ComponentMonitoring
    /// </summary>
    private Dictionary<int, List<string>> availableComponentMonitoringIDs;
    /// <summary>
    /// The system checks if this tag is at the beginning of the hint about to be given and can process it differently.
    /// This tag was meant to highlight the gameobject of the corresponding ComponentMonitoring when the hint is given
    /// </summary>
    private string highlightTag = "##HL##";
    /// <summary>
    /// key: Petri net id, value: ComponentMonitoring corresponding to the enigma in the meta Petri net.
    /// This dictionary is used to filter the hint that can be given to the player.
    /// The hint can be given if its enigma is triggerable in the meta Petri net
    /// </summary>
    private Dictionary<int, ComponentMonitoring> checkEnigmaOrderMeta;

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
    private float cooldownInitialWidth;
    /// <summary>
    /// used to count the time spent since the system last gave a hint to the player with labelCount
    /// </summary>
    private float systemHintTimer = float.MinValue;

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
    /// Description of the selected hint in IAR (right part of help tab in IAR)
    /// </summary>
    private TextMeshProUGUI hintTitle;
    private TextMeshProUGUI hintText;
    /// <summary>
    /// used to open a link to get more info about a hint (disabled if link is empty)
    /// </summary>
    private Button hintLinkButton;
    /// <summary>
    /// contains the link of the last selected hint and used to open the link when the hintLinkButton is clicked
    /// </summary>
    private string hintLink;
    /// <summary>
    /// prefab used to instantiate hint buttons
    /// </summary>
    private GameObject hintButtonPrefab;
    /// <summary>
    /// pool of disabled hint buttons used to enable a button when a hint is received rather then instantiating a one (optimisation)
    /// </summary>
    private List<GameObject> hintButtonsPool;
    private Button selectedHint = null;

    /// <summary>
    /// color of a hint button
    /// </summary>
    private ColorBlock colorHint;
    /// <summary>
    /// color of a new hint button
    /// </summary>
    private ColorBlock colorNewHint;
    /// <summary>
    /// color of the selected hint button
    /// </summary>
    private ColorBlock colorSelectedHint;

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
    /// contains HelpSystem parmeters
    /// </summary>
    public static HelpSystemConfig config;

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
            //if the key2 already exists in gameHints.dictionary and gameHints.dictionary[key1][key2].Value isn't empty internalGameHints.dictionary[key1][key2] isn't added
            foreach (string key1 in internalGameHints.dictionary.Keys)
            {
                if (!gameHints.dictionary.ContainsKey(key1))
                    gameHints.dictionary.Add(key1, new Dictionary<string, KeyValuePair<string, List<string>>>());
                foreach (string key2 in internalGameHints.dictionary[key1].Keys)
                {
                    if (gameHints.dictionary[key1].ContainsKey(key2)){
                        if(gameHints.dictionary[key1][key2].Value == null || gameHints.dictionary[key1][key2].Value.Count == 0)
                        {
                            gameHints.dictionary[key1][key2].Value.AddRange(internalGameHints.dictionary[key1][key2]);
                        }
                    }
                    else
                        gameHints.dictionary[key1].Add(key2, new KeyValuePair<string, List<string>>("", internalGameHints.dictionary[key1][key2]));
                }
            }
            labelWeights = f_labelWeights.First().GetComponent<LabelWeights>().weights;

            //Initialize checkEnigmaOrderMeta with the Petri net id and the corresponding ComponentMonitoring int the meta Petri net
            //has to be changed if ComponentMonitoring ids are modified
            checkEnigmaOrderMeta = new Dictionary<int, ComponentMonitoring>() {
                { 1, MonitoringManager.getMonitorById(143) },
                { 2, MonitoringManager.getMonitorById(144) },
                { 3, MonitoringManager.getMonitorById(145) },
                { 4, MonitoringManager.getMonitorById(146) },
                { 5, MonitoringManager.getMonitorById(152) },
                { 6, MonitoringManager.getMonitorById(153) },
                { 7, MonitoringManager.getMonitorById(154) },
                { 8, MonitoringManager.getMonitorById(155) },
                { 9, MonitoringManager.getMonitorById(156) },
                { 10, MonitoringManager.getMonitorById(157) },
                { 16, MonitoringManager.getMonitorById(147) },
                { 17, MonitoringManager.getMonitorById(149) },
                { 18, MonitoringManager.getMonitorById(158) }
            };

            //initialize availableComponentMonitoringIDs
            availableComponentMonitoringIDs = new Dictionary<int, List<string>>();
            List<string> keys1 = new List<string>(gameHints.dictionary.Keys);
            foreach (string key1 in keys1)
            {
                List<string> keys2 = new List<string>(gameHints.dictionary[key1].Keys);
                foreach (string key2 in keys2)
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

                    if (MonitoringManager.getMonitorById(id) == null)
                    {
                        gameHints.dictionary[key1].Remove(key2);
                        if (gameHints.dictionary[key1].Count == 0)
                            gameHints.dictionary.Remove(key1);
                    }
                }
            }

            //Removes hints of the unused puzzle Petri net
            if (LoadGameContent.gameContent.virtualPuzzle)
                RemoveHintsByPN("Enigma11_2");
            else
                RemoveHintsByPN("Enigma11_1");

            //format expected answers to be compared to formated answers from IARQueryEvaluator
            List<string> tmpListString;
            string tmpString;
            Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>> tmpDictionary = new Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>(gameHints.wrongAnswerFeedbacks);
            foreach (string key1 in gameHints.wrongAnswerFeedbacks.Keys)
            {
                tmpListString = new List<string>(gameHints.wrongAnswerFeedbacks[key1].Keys);
                foreach(string key2 in tmpListString)
                {
                    tmpString = StringToAnswer(key2);
                    if (!gameHints.wrongAnswerFeedbacks[key1].ContainsKey(tmpString))
                    {
                        gameHints.wrongAnswerFeedbacks[key1].Add(tmpString, gameHints.wrongAnswerFeedbacks[key1][key2]);
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
            cooldownInitialWidth = cooldownRT.sizeDelta.x;
            cooldownText = f_askHelpButton.First().transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            actionPerformedHistory = new Dictionary<GameObject, Queue<KeyValuePair<string, string>>>();

            //set hint button colors values
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
        if (!this.Pause)
            noActionTimer = Time.time;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
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
            float progressionRatio = ((totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions) / ((Time.time - f_timer.First().GetComponent<Timer>().startingTime) / config.sessionDuration);
            labelCount += labelWeights["stagnation"] * progressionRatio;

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

        //check the time spent since the last time the system gave a feedback with countLabel to know if it can give another one
        if (Time.time - systemHintTimer > config.systemHintCooldownDuration)
        {
            //enigma done / total enigma
            float enigmaProgression = (totalWeightedMetaActions - GetWeightedNumberEnigmasLeft()) / totalWeightedMetaActions;
            //time spent / session duration
            float timeProgression = (Time.time - f_timer.First().GetComponent<Timer>().startingTime) / config.sessionDuration;
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
                //add the weight of each label to labelCount
                for (int j = 0; j < nbLabels; j++)
                {
                    if (labelWeights.ContainsKey(tmpTrace.labels[j]))
                    {
                        //if label weight is negative, simply add it (negative -> correct actions)
                        if (labelWeights[tmpTrace.labels[j]] < 0)
                        {
                            labelCount += labelWeights[tmpTrace.labels[j]] / progressionRatio;
                            //labelCount can't be negative
                            if (labelCount < 0)
                                labelCount = 0;
                        }
                        else
                        {
                            //if label weight isn't negative, add it and check if labelCount > config.labelCountStep
                            labelCount += labelWeights[tmpTrace.labels[j]] * progressionRatio;

                            //if labelCount reached the step calculate the feedback level and ask a hint
                            if (labelCount > config.labelCountStep)
                            {
                                //calculate numberFeedbackExpected to be proportional to the number of feedback given, the time spent and the time left
                                float numberFeedbackExpected = (config.sessionDuration - (Time.time - f_timer.First().GetComponent<Timer>().startingTime)) * nbFeedBackGiven / (Time.time - f_timer.First().GetComponent<Timer>().startingTime);
                                float feedbackRatio = numberFeedbackExpected / GetNumberFeedbackLeft();

                                //compare feedback ratio to feedback steps to know which feedback level to use
                                int feedbackLevel = 2;
                                if (feedbackRatio < config.feedbackStep1)
                                    feedbackLevel = 1;
                                else if (feedbackRatio > config.feedbackStep2)
                                    feedbackLevel = 3;

                                DisplayHint(room.roomNumber, feedbackLevel);
                            }
                        }
                    }
                }
            }
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
    /// Called when an ActionPerformed component has been processed and removed by ActionManager 
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
                    //if action performed is a player objective/end action, remove all hints linked to monitors of the same Petri net
                    if (cm.Value.isEndAction)
                        RemoveHintsByPN(cm.Key.fullPnSelected);
                }
            }
        }
    }

    /// <summary>
    /// Called when the player click on a hint button in the hint list in help tab of IAR
    /// </summary>
    /// <param name="b">The clicked button</param>
    private void OnClickHint(Button b)
    {
        if (selectedHint)
            //change the color of the previousliy selected button
            selectedHint.colors = colorHint;

        selectedHint = b;
        selectedHint.colors = colorSelectedHint;
        tmpHC = selectedHint.GetComponent<HintContent>();
        //hint name format is "name.MonitorID" so put only the "name" part in buttonName
        string[] tmpStringArray = tmpHC.hintName.Split('.');
        string buttonName = tmpStringArray[0];
        for (int i = 1; i < tmpStringArray.Length - 1; i++)
            buttonName = string.Concat(buttonName, ".", tmpStringArray[i]);
        //display hint info on the right part of the help tab in IAR
        hintTitle.text = buttonName;
        hintText.text = tmpHC.text;
        if(tmpHC.link != "")
        {
            //if link filled, display link button
            hintLink = tmpHC.link;
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, true);
        }
        else
            GameObjectManager.setGameObjectState(hintLinkButton.gameObject, false);

        if(selectedHint.GetComponent<NewHint>())
            GameObjectManager.removeComponent<NewHint>(selectedHint.gameObject);

        GameObjectManager.addComponent<ActionPerformedForLRS>(b.gameObject, new
        {
            verb = "read",
            objectType = "feedback",
            objectName = string.Concat("hint_", b.transform.GetChild(0).GetComponent<Text>().text),
            activityExtensions = new Dictionary<string, List<string>>() {
                { "type", new List<string>() { "hint" } },
                { "content", new List<string>() { b.GetComponent<HintContent>().text } }
            }
        });
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
            //calculate numberFeedbackExpected to be proportional to the number of feedback given, the time spent and the time left
            float numberFeedbackExpected = (config.sessionDuration - (Time.time - f_timer.First().GetComponent<Timer>().startingTime)) * nbFeedBackGiven / (Time.time - f_timer.First().GetComponent<Timer>().startingTime);
            float feedbackRatio = numberFeedbackExpected / GetNumberFeedbackLeft();

            //compare feedback ratio to feedback steps to know which feedback level to use
            int feedbackLevel = 2;
            if (feedbackRatio < config.feedbackStep1)
                feedbackLevel = 1;
            else if (feedbackRatio > config.feedbackStep2)
                feedbackLevel = 3;

            playerAskedHelp = true;
            if (DisplayHint(room.roomNumber, feedbackLevel))
            {
                //if the player received an hint, start the cooldown
                playerHintTimer = Time.time;
                GameObjectManager.setGameObjectState(cooldownRT.gameObject, true);
            }
        }
    }

    /// <summary>
    /// Called when the link button of an hint on the right part of help tab in IAR is pressed
    /// and open the link of the hint
    /// </summary>
    private void OnClickHintLinkButton()
    {
        try
        {
            Application.OpenURL(hintLink);

            GameObjectManager.addComponent<ActionPerformedForLRS>(hintLinkButton.gameObject, new
            {
                verb = "read",
                objectType = "viewable",
                objectName = "hintLink",
                activityExtensions = new Dictionary<string, List<string>>() {
                    { "link", new List<string>() { hintLink } }
                }
            });
        }
        catch (Exception)
        {
            Debug.LogError(string.Concat("Invalid hint link: \"", hintLink, "\""));
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid hint link: \"", hintLink, "\"."));
        }
    }

    /// <summary>
    /// Called when a new WrongAnswerInfo component is created and process it to give a feedback on the answer given.
    /// These components are created when the player makes an action "Wrong"
    /// </summary>
    /// <param name="go"></param>
    private void OnWrongAnswer(GameObject go)
    {
        WrongAnswerInfo[] wrongAnswerArray = go.GetComponents<WrongAnswerInfo>();
        int nbWrongAnswer = wrongAnswerArray.Length;
        int monitorID;
        string[] tmpStringArray;
        for (int i = 0; i < nbWrongAnswer; i++){
            //foreach wrong answer, check if the given answer is in the gameHints.wrongAnswerFeedbacks dictionary
            foreach (string key in gameHints.wrongAnswerFeedbacks.Keys)
            {
                tmpStringArray = key.Split('.');
                if (int.TryParse(tmpStringArray[tmpStringArray.Length - 1], out monitorID))
                {
                    //if the monitor id of the wrong answer and the given answer are in the dictionary
                    if(monitorID == wrongAnswerArray[i].componentMonitoringID && gameHints.wrongAnswerFeedbacks[key].ContainsKey(wrongAnswerArray[i].givenAnswer))
                    {
                        //display feedback in hint list in IAR
                        string hintText = "";
                        if (gameHints.wrongAnswerFeedbacks[key][wrongAnswerArray[i].givenAnswer].Value.Count > 0)
                            hintText = gameHints.wrongAnswerFeedbacks[key][wrongAnswerArray[i].givenAnswer].Value[(int)UnityEngine.Random.Range(0, gameHints.wrongAnswerFeedbacks[key][wrongAnswerArray[i].givenAnswer].Value.Count - 0.01f)];
                        Button hintButton = CreateHintButton(key, hintText, monitorID, gameHints.wrongAnswerFeedbacks[key][wrongAnswerArray[i].givenAnswer].Key);


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
        //remove WrongAnswerInfo components
        for (int i = wrongAnswerArray.Length - 1; i > -1; i--)
            GameObjectManager.removeComponent(wrongAnswerArray[i]);
    }

    /// <summary>
    /// Remove from gameHints.dictionary all hints linked to the given id.
    /// Return false if the id doesn't exist or if one of the hint name in availableComponentMonitoringIDs[id] wasn't found in the dictionary.
    /// </summary>
    /// <param name="id">The id of the ComponentMonitoring that will get its hints removed</param>
    /// <returns></returns>
    private bool RemoveHintsByComponentID(int id)
    {
        if (availableComponentMonitoringIDs.ContainsKey(id))
        {
            //get the list of all hint names linked to this monitor id (stored int availableComponentMonitoringIDs)
            List<string> nameList = availableComponentMonitoringIDs[id];
            bool allRemoved = true;
            foreach(string name in nameList)
            {
                //foreach name of the list, look for the name in the ddictionary and remove it
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
    /// <param name="feedbackLevel"></param>
    /// <returns></returns>
    private bool DisplayHint(int room, int feedbackLevel)
    {
        //check which feedback level contains hints
        int availableFeedback = CheckAvailableFeedback(room, feedbackLevel);
        //if the feedback level is valid
        if(availableFeedback != -1)
        {
            //get a hint corresponding to the room and the feedback level
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

                string hintText = "";
                if (tmpPair.Value.Count > 0)
                    hintText = tmpPair.Value[(int)UnityEngine.Random.Range(0, tmpPair.Value.Count - 0.01f)];
                Button hintButton = CreateHintButton(hintName, hintText, id, tmpPair.Key);

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

                //remove hint from dictionary
                if (hasHighlightTag)
                    gameHints.dictionary[key1].Remove(string.Concat(highlightTag, hintName));
                else
                    gameHints.dictionary[key1].Remove(hintName);

                if (gameHints.dictionary[key1].Count == 0)
                {
                    //Debug.Log(key1 + " removed with size " + gameHints.dictionary[key1].Count);
                    gameHints.dictionary.Remove(key1);
                }

                systemHintTimer = Time.time;
                nbFeedBackGiven++;
                labelCount = 0;
                playerAskedHelp = false;
                return true;
            }
            else
            {
                //Debug.Log("No hint found.");
                playerAskedHelp = false;
                return false;
            }
        }
        else
        {
            //Debug.Log(string.Concat("No hint found for the room ", room));
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
            Button b = tmpGO.GetComponent<Button>();
            b.onClick.AddListener(delegate { OnClickHint(b); });
            hintButtonsPool.Add(tmpGO);

            Debug.LogWarning("You should increase hintButtonsPool initial size");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - You should increase hintButtonsPool initial size."));
        }

        tmpGO = hintButtonsPool[0];
        hintButtonsPool.RemoveAt(0);
        Button hintButton = tmpGO.GetComponent<Button>();
        string[] tmpStringArray = hintName.Split('.');
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
        RectTransform[] tmpRTArray = scrollViewContent.GetComponentsInChildren<RectTransform>();
        for (int i = 0; i < tmpRTArray.Length; i++)
            if (tmpRTArray[i].GetComponent<Button>())
                tmpRTArray[i].anchoredPosition += Vector2.down * hintButton.GetComponent<RectTransform>().sizeDelta.y;
        tmpRT.anchoredPosition = new Vector2(0, -0.5f * hintButton.GetComponent<RectTransform>().sizeDelta.y);
        tmpHC = tmpGO.GetComponent<HintContent>();
        tmpHC.hintName = hintName;
        tmpHC.text = hintText;
        //change subtitle text tot display the hint
        //subtitles.text = tmpPair.Value[(int)UnityEngine.Random.Range(0, nbHintTexts - 0.01f)];
        //GameObjectManager.setGameObjectState(subtitles.gameObject, true);
        //subtitlesTimer = Time.time;
        tmpHC.link = hintLink;
        if (hintMonitorID != -1)
            tmpHC.monitor = MonitoringManager.getMonitorById(hintMonitorID);
        hintButton.onClick.AddListener(delegate { OnClickHint(hintButton); });
        return hintButton;
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
                if (weight > 0)
                    left += weight;
            }

            //Return the number action left multiplied by they weight to reach the last action of the meta Petri net
            return left;
        }
        else
            return -1;
    }

    private float GetNumberEnigmasLeft()
    {
        if (finalComponentMonitoring)
            return MonitoringManager.getNextActionsToReach(finalComponentMonitoring, "perform", int.MaxValue).Count;
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
        
        for (int i = 0; i < triggerableActions.Count; i++)
        {
            int pn = triggerableActions[i].Key.fullPnSelected;
            //if the petri net is in the list
            if (rdpIDs.Contains(pn))
            {
                if (checkEnigmaOrderMeta.ContainsKey(pn))
                {
                    //if the action corresponding to the enigma in the meta is triggerable, add the ComponentMonitoring id to the list
                    for (int j = 0; j < triggerableActions.Count; j++)
                    {
                        if(triggerableActions[j].Key.fullPnSelected == 0)
                        if (triggerableActions[j].Key.id == checkEnigmaOrderMeta[pn].id)
                        {
                            actionsIDs.Add(triggerableActions[i].Key.id);
                            break;
                        }
                    }
                }
                else
                    actionsIDs.Add(triggerableActions[i].Key.id);
            }
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

    private string StringToAnswer(string answer)
    {
        // format answer
        answer = answer.Replace('é', 'e');
        answer = answer.Replace('è', 'e');
        answer = answer.Replace('à', 'a');
        answer = answer.ToUpper();
        return answer;
    }
}