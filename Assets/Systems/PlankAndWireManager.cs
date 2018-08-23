using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using FYFY_plugins.Monitoring;

public class PlankAndWireManager : FSystem {
    
    // this system manage the plank and the wire

    //all selectable objects
    private Family f_plank = FamilyManager.getFamily(new AnyOfTags("Plank"));
    private Family f_focusedPlank = FamilyManager.getFamily(new AnyOfTags("Plank"), new AllOfComponents(typeof(ReadyToWork), typeof(LinkedWith)));
    private Family f_focusedWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerOver), typeof(TextMeshPro))); // focused words on the plank
    private Family f_allWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerSensitive), typeof(TextMeshPro))); // all clickable words on the plank
    private Family f_closePlank = FamilyManager.getFamily (new AnyOfTags ("Plank", "PlankText", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    //plank
    private GameObject selectedPlank = null;
    private LineRenderer lr;                //used to link words
    private List<Vector3> lrPositions;

    private GameObject currentFocusedWord;

    private GameObject tmpGO;

    public static PlankAndWireManager instance;

    public PlankAndWireManager()
    {
        if (Application.isPlaying)
        {
            //initialise vairables
            lr = f_plank.First().GetComponent<LineRenderer>();
            lrPositions = new List<Vector3>();

            f_focusedPlank.addEntryCallback(onReadyToWorkOnPlank);

            f_focusedWords.addEntryCallback(onWordMouseEnter);
            f_focusedWords.addExitCallback(onWordMouseExit);
        }
        instance = this;
    }

    private void onReadyToWorkOnPlank(GameObject go)
    {
        selectedPlank = go;

        // launch this system
        instance.Pause = false;
    }

    private void onWordMouseEnter(GameObject go)
    {
        //if mouse over a word and word doesn't already clicked
        if (go.GetComponent<TextMeshPro>().color != Color.red && f_iarBackground.Count == 0)
            //if the word isn't selected change its color to yellow
            go.GetComponent<TextMeshPro>().color = Color.yellow;
        currentFocusedWord = go;
    }

    private void onWordMouseExit(int instanceId)
    {
        if (currentFocusedWord && currentFocusedWord.GetInstanceID() == instanceId && currentFocusedWord.GetComponent<TextMeshPro>().color != Color.red)
            //if the word isn't selected change its color to black
            currentFocusedWord.GetComponent<TextMeshPro>().color = Color.black;
        currentFocusedWord = null;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

    // return true if wire is selected into inventory
    private GameObject wireSelected()
    {
        foreach (GameObject go in f_itemSelected)
            if (go.name == "Wire")
                return go;
        return null;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        if (selectedPlank)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closePlank.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)))
                ExitPlank();
            else
            {
                // Check if a word is clicked and wire is selected
                if (Input.GetMouseButtonDown(0) && wireSelected())
                {
                    int nbPlankWords = f_focusedWords.Count;
                    for (int i = 0; i < nbPlankWords; i++)
                    {
                        tmpGO = f_focusedWords.getAt(i);

                        //if the word is selected (color red)
                        if (tmpGO.GetComponent<TextMeshPro>().color == Color.red)
                        {
                            //unselect it, but we are still over (yellow)
                            tmpGO.GetComponent<TextMeshPro>().color = Color.yellow;
                            //remove the vertex from the linerenderer
                            lrPositions.Remove(tmpGO.transform.TransformPoint(Vector3.up * -4));
                            lr.positionCount--;
                            //set the new positions
                            lr.SetPositions(lrPositions.ToArray());
                            if (HelpSystem.monitoring && tmpGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringTrace trace = new MonitoringTrace(tmpGO.GetComponent<ComponentMonitoring>(), "turnOff");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }
                        }
                        else
                        {    //if the word wasn't selected
                            if (lr.positionCount > 2)
                            {
                                //if there is already 3 selected words, unselect them and select the new one
                                foreach (GameObject w in f_allWords)
                                {
                                    if (w.GetComponent<TextMeshPro>().color == Color.red)
                                    {
                                        if (HelpSystem.monitoring && w.GetComponent<ComponentMonitoring>())
                                        {
                                            MonitoringTrace trace = new MonitoringTrace(w.GetComponent<ComponentMonitoring>(), "turnOff");
                                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                            HelpSystem.traces.Enqueue(trace);
                                        }
                                    }
                                    w.GetComponent<TextMeshPro>().color = Color.black;
                                }
                                lr.positionCount = 0;
                                lrPositions.Clear();
                            }
                            tmpGO.GetComponent<TextMeshPro>().color = Color.red;
                            //update the linerenderer
                            lr.positionCount++;
                            lrPositions.Add(tmpGO.transform.TransformPoint(Vector3.up * -4));
                            lr.SetPositions(lrPositions.ToArray());

                            bool correct = true;
                            foreach (GameObject word in f_allWords)
                                if ((word.name == "Objectifs" || word.name == "Methodes" || word.name == "Evaluation") && word.GetComponent<TextMeshPro>().color != Color.red)
                                    correct = false;

                            if (HelpSystem.monitoring && tmpGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringTrace trace = new MonitoringTrace(tmpGO.GetComponent<ComponentMonitoring>(), "turnOn");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }
                            if (correct)
                            {
                                // remove the wire from inventory
                                LinkedWith lw = selectedPlank.GetComponent<LinkedWith>();
                                GameObjectManager.setGameObjectState(lw.link, false);
                                // notify player success
                                GameObjectManager.addComponent<PlayUIEffect>(selectedPlank, new { effectCode = 3});

                                if (HelpSystem.monitoring)
                                {
                                    MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(23), "perform");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                    HelpSystem.traces.Enqueue(trace);
                                    trace = new MonitoringTrace(MonitoringManager.getMonitorById(54), "perform");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                    HelpSystem.traces.Enqueue(trace);
                                }
                            }
                        }
                    }
                }

                if (!currentFocusedWord && Input.GetMouseButtonDown(0) && wireSelected())
                {
                    //if click over nothing unselect all
                    foreach (GameObject word in f_allWords)
                    {
                        if (word.GetComponent<TextMeshPro>().color == Color.red)
                        {
                            if (HelpSystem.monitoring && word.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringTrace trace = new MonitoringTrace(word.GetComponent<ComponentMonitoring>(), "turnOff");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }
                        }
                        word.GetComponent<TextMeshPro>().color = Color.black;
                        lr.positionCount = 0;
                        lrPositions.Clear();
                    }
                }
            }
        }
	}

    private void ExitPlank()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedPlank);

        selectedPlank = null;

        if (HelpSystem.monitoring)
        {
            MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(53), "turnOff");
            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
            HelpSystem.traces.Enqueue(trace);
        }

        // pause this system
        instance.Pause = true;
    }
}