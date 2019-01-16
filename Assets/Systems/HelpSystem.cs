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

    private GameHints gameHints;
    private InternalGameHints internalGameHints;
    private Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>> initialGameHints;
    private string highlightTag = "##HL##";

    private float sessionDuration = 3600; //in Seconds
    private int totalMetaActions = 20;
    private float playerHintCooldownDuration = 300;
    private float systemHintCooldownDuration = 15;
    private float playerHintTimer = float.MinValue;
    private float systemHintTimer = float.MinValue;

    private float labelCount = 0;
    private int nbFeedBackGiven = 0;
    private List<string> correctLabels;
    private float correctCoef = -2;
    private List<string> errorLabels;
    private float errorCoef = 2;
    private List<string> otherLabels;
    private float otherCoef = 1;
    private float errorStep = 50;
    private float otherStep = 75;

    private TextMeshProUGUI subtitles;
    private float subtitlesTimer = float.MinValue;

    private float noActionTimer = float.MaxValue;

    private ComponentMonitoring finalComponentMonitoring;

    public static HelpSystem instance;

    public HelpSystem()
    {
        if (Application.isPlaying)
        {
            //get game hints filled with tips loaded from "Data/Hints_LearningScape.txt"
            gameHints = f_gameHints.First().GetComponent<GameHints>();
            //get internal game hints
            internalGameHints = f_internalGameHints.First().GetComponent<InternalGameHints>();
            //Dictionary<string, Dictionary<string, List<string>>>
            internalGameHints.dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(f_defaultGameContent.First().GetComponent<DefaultGameContent>().internalHintsJsonFile.text);
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
            initialGameHints = new Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>(gameHints.dictionary);

            sessionDuration = LoadGameContent.gameContent.sessionDuration * 60; //convert to seconds
            
            subtitles = f_subtitlesFamily.First().GetComponent<TextMeshProUGUI>();
            correctLabels = new List<string>() { "correct" };
            errorLabels = new List<string>() { "useless", "erroneous", "too-early" };
            otherLabels = new List<string>() { "stagnation", "already-seen" };

            f_traces.addEntryCallback(OnNewTraces);
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
            float progressionRatio = ((totalMetaActions - GetNumberEnigmasLeft()) / totalMetaActions) / ((Time.time - f_timer.First().GetComponent<Timer>().startingTime) / sessionDuration);
            labelCount += otherCoef * progressionRatio;

        }
    }

    private void OnNewTraces(GameObject go)
    {
        noActionTimer = Time.time;

        if (Time.time - systemHintTimer > systemHintCooldownDuration)
        {
            float enigmaProgression = (totalMetaActions - GetNumberEnigmasLeft()) / totalMetaActions;
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
                        int feedbackLevel = 1; //à caluler
                        //DisplayHint()
                    }
                    else if (otherLabels.Contains(tmpTrace.labels[j]))
                    {
                        labelCount += otherCoef * progressionRatio;
                        int feedbackLevel = 1;
                    }
                }
            }
        }
    }

    private void DisplayHint(int room, int feedbackLevel)
    {
        string hintName = GetHintName(room, feedbackLevel);
        string key1 = string.Concat(room, ".", feedbackLevel);

        if (gameHints.dictionary.ContainsKey(key1))
        {
            if (gameHints.dictionary[key1].ContainsKey(hintName))
            {
                int nbHintTexts = gameHints.dictionary[key1][hintName].Value.Count;
                if(nbHintTexts > 0)
                {
                    //change subtitle text tot display the hint
                    subtitles.text = gameHints.dictionary[key1][hintName].Value[(int)Random.Range(0, nbHintTexts - 0.01f)];
                    GameObjectManager.setGameObjectState(subtitles.gameObject, true);
                    subtitlesTimer = Time.time;
                }

                if(hintName.Substring(0, highlightTag.Length) == highlightTag)
                {
                    //if hint name starts with highlightTag, highlight the gameobject
                }

                //remove hint from dictionary
                gameHints.dictionary[key1].Remove(hintName);

                systemHintTimer = Time.time;
                nbFeedBackGiven++;
            }
        }
    }

    private int GetNumberEnigmasLeft()
    {
        if (finalComponentMonitoring)
        {
            //Return the number action left to reach the last action of the meta Petri net
            return MonitoringManager.getNextActionsToReach(finalComponentMonitoring, "perform", int.MaxValue).Count;
        }
        else
            return -1;
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
}