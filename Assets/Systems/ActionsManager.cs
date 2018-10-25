using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;

public class ActionsManager : FSystem {

    private Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformed)));
    private Family f_mainloop = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));

    public static ActionsManager instance;

    private GameObject traces;
    private string tmpString;

    private string tmpPerformer;
    private string[] tmpLabels;

    public ActionsManager()
    {
        if (Application.isPlaying)
        {
            traces = new GameObject("Traces");
            GameObjectManager.bind(traces);
            f_actions.addEntryCallback(ActionProcessing);
        }
        instance = this;
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
	protected override void onProcess(int familiesUpdateCount){
    }

    private void ActionProcessing(GameObject go)
    {
        if (/*go.GetComponent<ComponentMonitoring>() &&*/ !this.Pause)
        {
            foreach(ActionPerformed ap in go.GetComponents<ActionPerformed>())
            {
                ComponentMonitoring cMonitoring = null;
                string tmpActionName = "";
                if (ap.name != "" && ap.overrideName != "")
                {
                    //look for the ComponentMonitoring corresponding to name and overridename
                    bool matched = false;
                    foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                    {
                        foreach (TransitionLink tl in cm.transitionLinks)
                        {
                            if (tl.transition.label == ap.name && tl.transition.overridedLabel == ap.overrideName)
                            {
                                cMonitoring = cm;
                                tmpActionName = ap.name;
                                matched = true;
                                break;
                            }
                        }
                        if (matched)
                        {
                            break;
                        }
                    }
                    if (!matched)
                    {
                        Debug.LogError(string.Concat("Unable to trace action on \"", go.name, "\" because \"", ap.name, "\" and \"", ap.overrideName, "\" in its ActionPerformed don't correspond to any ComponentMonitoring."));
                    }
                }
                else if (ap.name != "")
                {
                    //look for the ComponentMonitoring corresponding to the name
                    bool matched = false;
                    foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                    {
                        foreach (TransitionLink tl in cm.transitionLinks)
                        {
                            if (tl.transition.label == ap.name)
                            {
                                cMonitoring = cm;
                                tmpActionName = ap.name;
                                matched = true;
                                break;
                            }
                        }
                        if (matched)
                        {
                            break;
                        }
                    }
                    if (!matched)
                    {
                        Debug.LogError(string.Concat("Unable to trace action on \"", go.name, "\" because \"", ap.name, "\" in its ActionPerformed doesn't correspond to any ComponentMonitoring."));
                    }
                }
                else if (ap.overrideName != "")
                {
                    //look for the ComponentMonitoring corresponding to the overridename
                    bool matched = false;
                    foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                    {
                        foreach (TransitionLink tl in cm.transitionLinks)
                        {
                            if (tl.transition.overridedLabel == ap.overrideName)
                            {
                                cMonitoring = cm;
                                tmpActionName = tl.transition.label;
                                matched = true;
                                break;
                            }
                        }
                        if (matched)
                        {
                            break;
                        }
                    }
                    if (!matched)
                    {
                        Debug.LogError(string.Concat("Unable to trace action on \"", go.name, "\" because \"", ap.overrideName, "\" in its ActionPerformed don't correspond to any ComponentMonitoring."));
                    }
                }
                else
                {
                    Debug.LogError(string.Concat("Unable to trace action on \"", go.name, "\" because no name given in its ActionPerformed component."));
                }
                if (cMonitoring)
                {
                    if (ap.performedBy == "system")
                    {
                        tmpPerformer = ap.performedBy;
                    }
                    else
                    {
                        tmpPerformer = "player";
                    }
                    if (ap.orLabels == null)
                        tmpLabels = MonitoringManager.trace(cMonitoring, tmpActionName, tmpPerformer);
                    else
                        tmpLabels = MonitoringManager.trace(cMonitoring, tmpActionName, tmpPerformer, true, ap.orLabels);
                    tmpString = tmpLabels[0];
                    for(int i = 1; i < tmpLabels.Length; i++)
                    {
                        tmpString = string.Concat(tmpString, " ", tmpLabels[i]);
                    }
                    GameObjectManager.addComponent<Trace>(traces, new { actionName = tmpActionName, componentMonitoring = cMonitoring,
                        performedBy = tmpPerformer, time = Time.time, orLabels = ap.orLabels, labels = tmpLabels });
                    Debug.Log(string.Concat(tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine , tmpString));
                }
                else if (ap.familyMonitoring)
                {
                    if (ap.performedBy == "system")
                    {
                        tmpPerformer = ap.performedBy;
                    }
                    else
                    {
                        tmpPerformer = "player";
                    }
                    if (ap.orLabels == null)
                        tmpLabels = MonitoringManager.trace(ap.familyMonitoring, tmpActionName, tmpPerformer);
                    else
                        tmpLabels = MonitoringManager.trace(ap.familyMonitoring, tmpActionName, tmpPerformer, true, ap.orLabels);
                    tmpString = tmpLabels[0];
                    for (int i = 1; i < tmpLabels.Length; i++)
                    {
                        tmpString = string.Concat(tmpString, " ", tmpLabels[i]);
                    }
                    GameObjectManager.addComponent<Trace>(traces, new
                    {
                        actionName = tmpActionName,
                        componentMonitoring = ap.familyMonitoring,
                        performedBy = tmpPerformer,
                        time = Time.time,
                        orLabels = ap.orLabels,
                        labels = tmpLabels
                    });
                    Debug.Log(string.Concat(tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
                }
                GameObjectManager.removeComponent(ap);
            }
        }
    }
}