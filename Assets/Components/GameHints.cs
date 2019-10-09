using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameHints : MonoBehaviour
{
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// The dictionary has to follow this pattern:
    /// "[ComponentMonitoringId].[rdpActionName]": {
    /// 	"Comment": [{"Key": "", "Value": "Human text to easily identify the game object associted to these hints"}],
    /// 	"options": [
    /// 		{"Key": "", "Value": "?? => means force hint to display"},
    /// 		{"Key": "", "Value": "-- => means force hint to destroy"}
    /// 	],
    /// 	"[hintLevel]": [
    /// 		{
    /// 			"Key": "[externalLink1]",
    /// 			"Value": "hint content 1"
    /// 		},
    /// 		{
    /// 			"Key": "[externalLink2]",
    /// 			"Value": "hint content 2"
    /// 		}
    /// 	]
    /// },
    /// </summary>
    public Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>> dictionary;
    /// <summary>
    /// Dictionary used to store feedbacks for wrong answers
    /// The first parameter asked is a string of the format "x.y" with x the question name and y the ComponentMonitoring ID of the action "Wrong" performed
    /// The second parameter is the given answer
    /// Once a feedback is identified, a pair containing a link for more information and a list of different way to formulate the feedback is given
    /// If the link is filled, a button in the help tab will appear to open the link
    /// </summary>
    public Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> wrongAnswerFeedbacks;
}