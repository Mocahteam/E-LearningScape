using UnityEngine;
using FYFY_plugins.Monitoring;

/// <summary>
/// Contains information about a hint.
/// This component is attached to buttons in the hint list in IAR
/// </summary>
public class HintContent : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public string hintName;
    public string text;
    public string link;
    public ComponentMonitoring monitor;
}