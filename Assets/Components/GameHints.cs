using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameHints : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// Dictionary used to store hints
    /// The first parameter asked is a string of the format "x.y" with x the room number and y the feedback level
    /// The second parameter is a string of the format "x.y" with x the name of the hint and y the ComponentMonitoring id
    /// If the second parameter starts with "##HL##", the gameobject of the ComponentMonitoring will be highlighted
    /// Once a hint is identified, a pair containing a link for more information and a list of different way to formulate the hint is given
    /// If the link is filled, a button in the help tab will appear to open the link
    /// </summary>
    public Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>> dictionary;
    /// <summary>
    /// Dictionary used to store feedbacks for wrong answers
    /// The first parameter asked is a string of the format "x.y" with x the question name and y the ComponentMonitoring ID of the action "Wrong" performed
    /// The second parameter is the given answer
    /// Once a feedback is identified, a pair containing a link for more information and a list of different way to formulate the feedback is given
    /// If the link is filled, a button in the help tab will appear to open the link
    /// </summary>
    public Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>> wrongAnswerFeedbacks;
}