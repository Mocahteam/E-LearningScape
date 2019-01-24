using UnityEngine;
using FYFY_plugins.Monitoring;

public class HintContent : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public string hintName;
    public string text;
    public string link;
    public ComponentMonitoring monitor;
}