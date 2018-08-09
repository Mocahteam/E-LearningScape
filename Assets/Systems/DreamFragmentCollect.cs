using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.Monitoring;

public class DreamFragmentCollect : FSystem {

    private Family dfFamily = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
    private Family dfUIFamily = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AnyOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD));
    private Family player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));

    private GameObject dfUI;

    private RaycastHit hit;
    private GameObject selectedFragment;
    private DreamFragment tmpDFComponent;
    public static bool onFragment = false;
    private bool onIAR = false;
    private bool fragment0FirstActivation = true;
    private bool[] fragmentsSeen;
    private bool enigmaSolved = false;

    public DreamFragmentCollect()
    {
        if (Application.isPlaying)
        {
            dfUI = dfUIFamily.First();
            dfUI.GetComponentInChildren<Button>().onClick.AddListener(CloseWindow);
            fragmentsSeen = new bool[6];
            for(int i = 0; i < fragmentsSeen.Length; i++)
            {
                fragmentsSeen[i] = false;
            }
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit) && !onFragment)
        {
            int nb = dfFamily.Count;
            for(int i = 0; i < nb; i++)
            {
                if (Object.ReferenceEquals(hit.transform.gameObject, dfFamily.getAt(i)))
                {
                    if (Input.GetMouseButtonDown(0) && !onIAR && !StoryDisplaying.reading)
                    {
                        onFragment = true;
                        player.First().GetComponent<FirstPersonController>().enabled = false;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        selectedFragment = dfFamily.getAt(i);
                        GameObjectManager.setGameObjectState(dfUI,true);
                        tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                        if (HelpSystem.monitoring && selectedFragment.GetComponent<ComponentMonitoring>())
                        {
                            if(tmpDFComponent.type == 1 && !enigmaSolved)
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
                                for(int j = 0; j < fragmentsSeen.Length; j++)
                                {
                                    allSeen = fragmentsSeen[j] && allSeen;
                                }
                                if (allSeen)
                                {
                                    enigmaSolved = true;
                                    if (HelpSystem.monitoring)
                                    {
                                        MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(25), "perform");
                                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                        HelpSystem.traces.Enqueue(trace);
                                        trace = new MonitoringTrace(MonitoringManager.getMonitorById(26), "perform");
                                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.SYSTEM);
                                        HelpSystem.traces.Enqueue(trace);
                                    }
                                }
                            }
                            if (selectedFragment.transform.parent.gameObject.name.Contains("Chair"))
                            {
                                if(player.First().transform.localScale.x > 0.9f)
                                {
                                    MonitoringTrace trace = new MonitoringTrace(selectedFragment.GetComponent<ComponentMonitoring>(), "activate");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER, true, "l2");
                                    HelpSystem.traces.Enqueue(trace);
                                }
                                else
                                {
                                    MonitoringTrace trace = new MonitoringTrace(selectedFragment.GetComponent<ComponentMonitoring>(), "activate");
                                    trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER, true, "l1");
                                    HelpSystem.traces.Enqueue(trace);
                                }
                            }
                            else
                            {
                                MonitoringTrace trace = new MonitoringTrace(selectedFragment.GetComponent<ComponentMonitoring>(), "activate");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }
                        }
                        if (tmpDFComponent.type == 0)
                        {
                            dfUI.GetComponentInChildren<TextMeshProUGUI>().text = string.Concat("Ouvrez le fragment de rêve numéro ", tmpDFComponent.id);
                        }
                        else if (tmpDFComponent.type == 1 || tmpDFComponent.type == 2)
                        {
                            dfUI.GetComponentInChildren<TextMeshProUGUI>().text = string.Concat("\"", tmpDFComponent.itemName, "\"");
                        }
                    }
                    break;
                }
            }
        }
        onIAR = IARTab.onIAR;

    }

    private void CloseWindow()
    {
        onFragment = false;
        if (selectedFragment.GetComponent<DreamFragment>().type != 2)
        {
            if (selectedFragment.GetComponentInChildren<ParticleSystem>())
            {
                GameObjectManager.setGameObjectState(selectedFragment.GetComponentInChildren<ParticleSystem>().gameObject,false);
            }
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
        GameObjectManager.setGameObjectState(dfUI,false);
        player.First().GetComponent<FirstPersonController>().enabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}