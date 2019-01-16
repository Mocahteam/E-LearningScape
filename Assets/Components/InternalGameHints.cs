using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class InternalGameHints : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// Dictionary used to store hints
    /// The first parameter asked is a string of the format "x.y" with x the room number and y the feedback level
    /// The second parameter is a string of the format "x.y" with x the name of the hint and y the ComponentMonitoring id
    /// Once a hint is identified, a list of different way to formulate the hint is given
    /// </summary>
    public Dictionary<string, Dictionary<string, List<string>>> dictionary;
}