using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameTips : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// Dictionary used to store tips
    /// The first parameter asked is a string of the format "x.y" with x the enigma id number and y the feedback level
    /// The second parameter is a string that contains the name of the tip or "##Monitor##X" with X the id of the monitor concerned
    /// Once a tip is identified, a list of different way to formulate it is given
    /// </summary>
    public Dictionary<string, Dictionary<string, List<string>>> dictionary;
}