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
    private Family f_monitoringComponents = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));
    private Family f_traces = FamilyManager.getFamily(new AllOfComponents(typeof(Trace)));
    private Family f_gameTips = FamilyManager.getFamily(new AllOfComponents(typeof(GameTips)));
    private Family f_internalGameTips = FamilyManager.getFamily(new AllOfComponents(typeof(InternalGameTips)));
    private Family f_defaultGameContent = FamilyManager.getFamily(new AllOfComponents(typeof(DefaultGameContent)));

    private GameTips gameTips;
    private InternalGameTips internalGameTips;

    private Dictionary<int, int> uselessTagCount;
    private List<string> stringsError;

    private TextMeshProUGUI subtitles;
    private float subtitlesTimer = float.MinValue;

    public static HelpSystem instance;

    public HelpSystem()
    {
        if (Application.isPlaying)
        {
            //get game tips filled with tips loaded from "Data/Tips_LearningScape.txt"
            gameTips = f_gameTips.First().GetComponent<GameTips>();
            //get internal game tips
            internalGameTips = f_internalGameTips.First().GetComponent<InternalGameTips>();
            internalGameTips.dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(f_defaultGameContent.First().GetComponent<DefaultGameContent>().internalTipsJsonFile.text);

            string[] tmpStringArray;
            //add internal game tips to the dictionary of the component GameTips
            //
            //changer ca
            //
            foreach(string key1 in internalGameTips.dictionary.Keys)
            {
                if (!gameTips.dictionary.ContainsKey(key1))
                    gameTips.dictionary.Add(key1, new Dictionary<string, List<string>>());
                foreach (string key2 in internalGameTips.dictionary[key1].Keys)
                {
                    tmpStringArray = key2.Split('.');
                    gameTips.dictionary[key1].Add(string.Concat("##monitor##",tmpStringArray[1]), internalGameTips.dictionary[key1][key2]);
                }
            }

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