using UnityEngine;
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
    private Family f_monitoringComponents = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));
    private Family f_traces = FamilyManager.getFamily(new AllOfComponents(typeof(Trace)));
    private Family f_gameHints = FamilyManager.getFamily(new AllOfComponents(typeof(GameHints)));
    private Family f_internalGameHints = FamilyManager.getFamily(new AllOfComponents(typeof(InternalGameHints)));
    private Family f_defaultGameContent = FamilyManager.getFamily(new AllOfComponents(typeof(DefaultGameContent)));

    private GameHints gameHints;
    private InternalGameHints internalGameHints;

    private float sessionDuration = 3600; //in Seconds
    private Dictionary<int, int> uselessTagCount;
    private List<string> stringsError;

    private TextMeshProUGUI subtitles;
    private float subtitlesTimer = float.MinValue;

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

            string[] tmpStringArray;
            //add internal game hints to the dictionary of the component GameHints
            //
            //changer ca
            //
            foreach(string key1 in internalGameHints.dictionary.Keys)
            {
                if (!gameHints.dictionary.ContainsKey(key1))
                    gameHints.dictionary.Add(key1, new Dictionary<string, KeyValuePair<string, List<string>>>());
                foreach (string key2 in internalGameHints.dictionary[key1].Keys)
                {
                    tmpStringArray = key2.Split('.');
                    gameHints.dictionary[key1].Add(string.Concat("##monitor##",tmpStringArray[1]), new KeyValuePair<string, List<string>>("", internalGameHints.dictionary[key1][key2]));
                }
            }

            sessionDuration = LoadGameContent.gameContent.sessionDuration * 60; //convert to seconds

            uselessTagCount = new Dictionary<int, int>();
            subtitles = f_subtitlesFamily.First().GetComponent<TextMeshProUGUI>();
            stringsError = new List<string>() { "useless", "erroneous", "too-early" };

            int nb = f_monitoringComponents.Count;
            ComponentMonitoring tmpCM;
            for (int i = 0; i < nb; i++)
            {
                tmpCM = f_monitoringComponents.getAt(i).GetComponent<ComponentMonitoring>();
                if (!uselessTagCount.ContainsKey(tmpCM.fullPnSelected))
                {
                    uselessTagCount.Add(tmpCM.fullPnSelected, 0);
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
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        if (subtitles.gameObject.activeSelf && Time.time - subtitlesTimer > 2)
        {
            subtitles.text = string.Empty;
            GameObjectManager.setGameObjectState(subtitles.gameObject, false);
        }
        //if (traces.Count != 0)
        //{
        //    int nbTraces = traces.Count;
        //    Trace trace;
        //    for (int i = 0; i < nbTraces; i++)
        //    {
        //        trace = traces.Dequeue();
        //        int nbTags = trace.labels.Length;
        //        for (int j = 0; j < nbTags; j++)
        //        {
        //            //trace.labels[j] = Regex.Replace(trace.labels[j], @"\t|\n|\r", "");
        //            if (stringsError.Contains(trace.labels[j]))
        //            {
        //                uselessTagCount[trace.componentMonitoring.fullPnSelected]++;
        //            }
        //        }
        //        history.Add(trace);
        //    }
        //    trace = history[history.Count - 1];
        //    if (uselessTagCount[trace.componentMonitoring.fullPnSelected] > 1)
        //    {
        //        GameObjectManager.setGameObjectState(subtitles.gameObject, true);
        //        tmpEndAction = endActions[trace.componentMonitoring.fullPnSelected][(int)(Random.value * endActions[trace.componentMonitoring.fullPnSelected].Count)];
        //        tmpEndAction = MonitoringManager.getNextActionsToReach(tmpEndAction.Key, tmpEndAction.Value, 1)[0];
        //        if (answersGONames.Contains(tmpEndAction.Key.gameObject.name))
        //        {
        //            if(uselessTagCount[trace.componentMonitoring.fullPnSelected] > 3)
        //            {
        //                subtitles.text = string.Concat("Maintenant il faut que je ", tmpEndAction.Value, " ", tmpEndAction.Key.gameObject.name);
        //                subtitlesTimer = Time.time;
        //                uselessTagCount[trace.componentMonitoring.fullPnSelected] = 0;
        //            }
        //        }
        //        else
        //        {
        //            subtitles.text = string.Concat("Maintenant il faut que je ", tmpEndAction.Value, " ", tmpEndAction.Key.gameObject.name);
        //            subtitlesTimer = Time.time;
        //            uselessTagCount[trace.componentMonitoring.fullPnSelected] = 0;
        //        }
        //    }
        //}
        /*.transitionLinks[0].transition.overridedLabel*/
    }
}