using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;

public class HelpSystem : FSystem {

    private Family subtitlesFamily = FamilyManager.getFamily(new AnyOfTags("HelpSubtitles"));
    private Family monitoringComponents = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));

    public static bool monitoring = false;
    public static Queue<MonitoringTrace> traces;

    private Dictionary<int, List<KeyValuePair<ComponentMonitoring, string>>> endActions;
    private KeyValuePair<ComponentMonitoring, string> tmpEndAction;
    private List<MonitoringTrace> history;
    private Dictionary<int, int> uselessTagCount;
    private List<string> stringsError;
    private List<string> answersGONames;

    private TextMeshProUGUI subtitles;
    private float subtitlesTimer = float.MinValue;

    public HelpSystem()
    {
        if (Application.isPlaying)
        {
            traces = new Queue<MonitoringTrace>();
            history = new List<MonitoringTrace>();
            uselessTagCount = new Dictionary<int, int>();
            endActions = new Dictionary<int, List<KeyValuePair<ComponentMonitoring, string>>>();
            subtitles = subtitlesFamily.First().GetComponent<TextMeshProUGUI>();
            stringsError = new List<string>() { "useless", "erroneous", "too-early" };
            answersGONames = new List<string>() { "Objectifs","Methodes", "Evaluation"};

            int nb = monitoringComponents.Count;
            ComponentMonitoring tmpCM;
            for (int i = 0; i < nb; i++)
            {
                tmpCM = monitoringComponents.getAt(i).GetComponent<ComponentMonitoring>();
                if (!uselessTagCount.ContainsKey(tmpCM.fullPnSelected))
                {
                    uselessTagCount.Add(tmpCM.fullPnSelected, 0);
                }
                foreach (TransitionLink link in tmpCM.transitionLinks)
                {
                    //Debug.Log(string.Concat(tmpCM.fullPn, " ", link.transition.label, " ", link.isEndAction));
                    if (link.isEndAction)
                    {
                        if (!endActions.ContainsKey(tmpCM.fullPnSelected))
                        {
                            endActions.Add(tmpCM.fullPnSelected, new List<KeyValuePair<ComponentMonitoring, string>>());
                        }
                        KeyValuePair<ComponentMonitoring, string> pair = new KeyValuePair<ComponentMonitoring, string>(tmpCM, link.transition.label);
                        endActions[tmpCM.fullPnSelected].Add(pair);
                    }
                }
            }
        }
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (subtitles.gameObject.activeSelf && Time.time - subtitlesTimer > 2)
        {
            subtitles.text = string.Empty;
            GameObjectManager.setGameObjectState(subtitles.gameObject, false);
        }
        if (traces.Count != 0)
        {
            int nbTraces = traces.Count;
            MonitoringTrace trace;
            for (int i = 0; i < nbTraces; i++)
            {
                trace = traces.Dequeue();
                int nbTags = trace.result.Length;
                for (int j = 0; j < nbTags; j++)
                {
                    trace.result[j] = Regex.Replace(trace.result[j], @"\t|\n|\r", "");
                    if (stringsError.Contains(trace.result[j]))
                    {
                        uselessTagCount[trace.component.fullPnSelected]++;
                    }
                }
                history.Add(trace);
            }
            trace = history[history.Count - 1];
            if (uselessTagCount[trace.component.fullPnSelected] > 1)
            {
                GameObjectManager.setGameObjectState(subtitles.gameObject, true);
                tmpEndAction = endActions[trace.component.fullPnSelected][(int)(Random.value * endActions[trace.component.fullPnSelected].Count)];
                tmpEndAction = MonitoringManager.getNextActionsToReach(tmpEndAction.Key, tmpEndAction.Value, 1)[0];
                if (answersGONames.Contains(tmpEndAction.Key.gameObject.name))
                {
                    if(uselessTagCount[trace.component.fullPnSelected] > 3)
                    {
                        subtitles.text = string.Concat("Maintenant il faut que je ", tmpEndAction.Value, " ", tmpEndAction.Key.gameObject.name);
                        subtitlesTimer = Time.time;
                        uselessTagCount[trace.component.fullPnSelected] = 0;
                    }
                }
                else
                {
                    subtitles.text = string.Concat("Maintenant il faut que je ", tmpEndAction.Value, " ", tmpEndAction.Key.gameObject.name);
                    subtitlesTimer = Time.time;
                    uselessTagCount[trace.component.fullPnSelected] = 0;
                }
            }
        }
        /*.transitionLinks[0].transition.overridedLabel*/
    }
}

public struct MonitoringTrace
{
    public ComponentMonitoring component;
    public string action;
    public string[] result;

    public MonitoringTrace(ComponentMonitoring initCompenent, string initAction)
    {
        component = initCompenent;
        action = initAction;
        result = null;
    }
}