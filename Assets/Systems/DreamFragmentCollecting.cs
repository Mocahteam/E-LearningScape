using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.Monitoring;

public class DreamFragmentCollecting : FSystem {

    // Display Fragment UI when player select a fragment in game

    private Family dfFamily = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
    private Family dfUIFamily = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AnyOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD));
    private Family player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));

    private GameObject dfUI;
    private TextMeshProUGUI FragmentText;
    private RaycastHit hit;
    private GameObject selectedFragment;
    private DreamFragment tmpDFComponent;
    private bool[] fragmentsSeen;
    private bool enigmaSolved = false;

    public static DreamFragmentCollecting instance;

    public DreamFragmentCollecting()
    {
        if (Application.isPlaying)
        {
            dfUI = dfUIFamily.First();
            // Add listener on child button to close UI
            dfUI.GetComponentInChildren<Button>().onClick.AddListener(CloseWindow);
            // Get child text area
            FragmentText = dfUI.GetComponentInChildren<TextMeshProUGUI>();

            fragmentsSeen = new bool[6];
            for(int i = 0; i < fragmentsSeen.Length; i++)
                fragmentsSeen[i] = false;
        }
        instance = this;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // Compute Raycast only when mouse is clicked
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                // try to find a fragment touched by the raycast
                if (dfFamily.contains(hit.transform.gameObject.GetInstanceID()))
                {
                    // Show fragment UI
                    selectedFragment = hit.transform.gameObject;
                    GameObjectManager.setGameObjectState(dfUI, true);
                    tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                    // Set UI text depending on type and id
                    if (tmpDFComponent.type == 0)
                        FragmentText.text = string.Concat("Ouvrez le fragment de rêve numéro ", tmpDFComponent.id);
                    else if (tmpDFComponent.type == 1 || tmpDFComponent.type == 2)
                        FragmentText.text = string.Concat("\"", tmpDFComponent.itemName, "\"");
                    // Pause this system and dependant systems
                    this.Pause = true;
                    MovingSystem.instance.Pause = true;
                    IARTabNavigation.instance.Pause = true;
                    // TODO => Manage other dependant systems

                    // Manage help
                    if (HelpSystem.monitoring && selectedFragment.GetComponent<ComponentMonitoring>())
                    {
                        MonitoringTrace trace;
                        if (tmpDFComponent.type == 1 && !enigmaSolved)
                        {
                            switch (tmpDFComponent.itemName)
                            {
                                case "il":
                                    fragmentsSeen[0] = true;
                                    break;

                                case "faut":
                                    fragmentsSeen[1] = true;
                                    break;

                                case "savoir":
                                    fragmentsSeen[2] = true;
                                    break;

                                case "changer":
                                    fragmentsSeen[3] = true;
                                    break;

                                case "de":
                                    fragmentsSeen[4] = true;
                                    break;

                                case "posture":
                                    fragmentsSeen[5] = true;
                                    break;

                                default:
                                    break;
                            }
                            bool allSeen = true;
                            for (int j = 0; j < fragmentsSeen.Length; j++)
                            {
                                allSeen = fragmentsSeen[j] && allSeen;
                            }
                            if (allSeen)
                            {
                                enigmaSolved = true;
                                if (HelpSystem.monitoring)
                                {
                                    trace = new MonitoringTrace(MonitoringManager.getMonitorById(25), "perform");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                    HelpSystem.traces.Enqueue(trace);
                                    trace = new MonitoringTrace(MonitoringManager.getMonitorById(26), "perform");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                    HelpSystem.traces.Enqueue(trace);
                                }
                            }
                        }

                        trace = new MonitoringTrace(selectedFragment.GetComponent<ComponentMonitoring>(), "activate");
                        if (selectedFragment.transform.parent.gameObject.name.Contains("Chair"))
                        {
                            if (player.First().transform.localScale.x > 0.9f)
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER, true, "l2");
                            else
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER, true, "l1");
                        }
                        else
                            trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                        HelpSystem.traces.Enqueue(trace);
                    }
                }
            }
        }
    }

    private void CloseWindow()
    {
        if (selectedFragment.GetComponent<DreamFragment>().type != 2)
        {
            // disable particles
            if (selectedFragment.GetComponentInChildren<ParticleSystem>())
                GameObjectManager.setGameObjectState(selectedFragment.GetComponentInChildren<ParticleSystem>().gameObject,false);
            // disable glowing
            foreach (Transform child in selectedFragment.transform)
            {
                if (child.gameObject.tag == "DreamFragmentLight")
                {
                    child.gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                    break;
                }
            }
        }
        selectedFragment = null;
        // close UI
        GameObjectManager.setGameObjectState(dfUI,false);
        // Unpause this system and dependants systems
        this.Pause = false;
        MovingSystem.instance.Pause = false;
        IARTabNavigation.instance.Pause = false;
        // TODO => Manage other dependant systems
    }
}

//TESTER INTERDEPENDANCES ENTRE LES SYSTEMES SURTOUT DREAMFRAGMENTCOLLECT ET IARTABNAVIGATION ET MOVINGSYSTEM