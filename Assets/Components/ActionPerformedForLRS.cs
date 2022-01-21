using UnityEngine;
using System.Collections.Generic;

public class ActionPerformedForLRS : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System software architecture).

    public string verb;
    public string objectType;

    public bool result = false;
    /// <summary>
    /// Negative: false, 0: null, Positive: true
    /// </summary>
    public int completed = 0;
    /// <summary>
    /// Negative: false, 0: null, Positive: true
    /// </summary>
    public int success = 0;
    public string response = null;
    public int? score = null;
    public float duration = 0;
    /// <summary>
    /// Keys are extention's fields' names and values are lists of values of each field
    /// </summary>
    public Dictionary<string, string> activityExtensions = null;
    /// <summary>
    /// Keys are extention's fields' names and values are lists of values of each field
    /// </summary>
    public Dictionary<string, string> resultExtensions = null;
}