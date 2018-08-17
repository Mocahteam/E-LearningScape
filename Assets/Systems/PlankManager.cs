using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using FYFY_plugins.Monitoring;

public class PlankManager : FSystem {
    
    // this system manage the plank and the wire

    //all selectable objects
    private Family plank = FamilyManager.getFamily(new AnyOfTags("Plank"));
    private Family focusedPlank = FamilyManager.getFamily(new AnyOfTags("Plank"), new AllOfComponents(typeof(ReadyToWork), typeof(LinkedWith)));
    private Family focusedWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerOver), typeof(TextMeshPro))); // focused words on the plank
    private Family allWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerSensitive), typeof(TextMeshPro))); // all clickable words on the plank
    private Family closePlank = FamilyManager.getFamily (new AnyOfTags ("Plank", "PlankText", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    //information for animations
    private float speed;
    private float speedRotation;
    private float oldDT;
    private float dist = -1;
    private Vector3 objectPos = Vector3.zero;
    private int tmpCount = -1;
    private Vector3 camNewDir;
    private Vector3 newDir;
    private Vector3 playerLocalScale;

    //plank
    private GameObject selectedPlank = null;
    private bool onPlank = false;           //true when the player is using the plank
    private bool moveToPlank = false;       //true during the animation to move the player in front of the plank
    private Vector3 plankPos;               //position of the player when using the plank
    private LineRenderer lr;                //used to link words
    private List<Vector3> lrPositions;
    private GameObject plankSubtitle;

    private GameObject currentFocusedWord;

    private GameObject forGO, forGO2;

    public static PlankManager instance;

    public PlankManager()
    {
        if (Application.isPlaying)
        {
            //initialise vairables
            lr = plank.First().GetComponent<LineRenderer>();
            lrPositions = new List<Vector3>();

            foreach (Transform child in plank.First().transform)
            {
                if (child.gameObject.name == "SubTitles")
                {
                    plankSubtitle = child.gameObject;
                }
            }

            focusedPlank.addEntryCallback(onReadyToWorkOnPlank);

            focusedWords.addEntryCallback(onWordMouseEnter);
            focusedWords.addExitCallback(onWordMouseExit);
        }
        instance = this;
    }

    private void onReadyToWorkOnPlank(GameObject go)
    {
        selectedPlank = go;
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
        foreach (GameObject go in itemSelected)
            if (go.name == "Wire")
                return go;
        return null;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        speed = 8f * Time.deltaTime;
        speedRotation *= Time.deltaTime / oldDT;
        oldDT = Time.deltaTime;

        if (selectedPlank)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((closePlank.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)))
                ExitPlank();
            else
            {
                // Check if a word is clicked and wire is selected
                if (Input.GetMouseButtonDown(0) && wireSelected())
                {
                    int nbPlankWords = focusedWords.Count;
                    for (int i = 0; i < nbPlankWords; i++)
                    {
                        forGO = focusedWords.getAt(i);

                        //if the word is selected (color red)
                        if (forGO.GetComponent<TextMeshPro>().color == Color.red)
                        {
                            //unselect it, but we are still over (yellow)
                            forGO.GetComponent<TextMeshPro>().color = Color.yellow;
                            //remove the vertex from the linerenderer
                            lrPositions.Remove(forGO.transform.TransformPoint(Vector3.up * -4));
                            lr.positionCount--;
                            //set the new positions
                            lr.SetPositions(lrPositions.ToArray());
                            if (HelpSystem.monitoring && forGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringTrace trace = new MonitoringTrace(forGO.GetComponent<ComponentMonitoring>(), "turnOff");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }
                        }
                        else
                        {    //if the word wasn't selected
                            if (lr.positionCount > 2)
                            {
                                //if there is already 3 selected words, unselect them and select the new one
                                foreach (GameObject w in allWords)
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
                            forGO.GetComponent<TextMeshPro>().color = Color.red;
                            //update the linerenderer
                            lr.positionCount++;
                            lrPositions.Add(forGO.transform.TransformPoint(Vector3.up * -4));
                            lr.SetPositions(lrPositions.ToArray());

                            bool correct = true;
                            foreach (GameObject word in allWords)
                                if ((word.name == "Objectifs" || word.name == "Methodes" || word.name == "Evaluation") && word.GetComponent<TextMeshPro>().color != Color.red)
                                    correct = false;

                            if (HelpSystem.monitoring && forGO.GetComponent<ComponentMonitoring>())
                            {
                                MonitoringTrace trace = new MonitoringTrace(forGO.GetComponent<ComponentMonitoring>(), "turnOn");
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
                    foreach (GameObject word in allWords)
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
    }
}