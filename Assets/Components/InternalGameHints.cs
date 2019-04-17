using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class InternalGameHints : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// Dictionary used to store hints
    /// The first key is a string of the format "x.y.z" with x the room number, y the name of the hint and z the ComponentMonitoring id
    /// The second key is the feedback level
    /// Once a hint is identified, a list of different way to formulate the hint is given
    /// </summary>
    public Dictionary<string, Dictionary<string, List<string>>> dictionary;
}