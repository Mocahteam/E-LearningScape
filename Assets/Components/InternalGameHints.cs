using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class InternalGameHints : MonoBehaviour
{
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// The dictionary has to follow this pattern:
    /// "[ComponentMonitoringId].[rdpActionName]": {
	/// 	"Comment": ["Human text to easily identify the game object associted to these hints"],
	/// 	"options": [
	/// 		"?? => means force hint to display",
	/// 		"-- => means force hint to destroy"
	/// 	],
	/// 	"[hintLevel]": [
	/// 		"hint content 1",
	/// 		"hint content 2"
	/// 	]
    /// }
    /// </summary>
    public Dictionary<string, Dictionary<string, List<string>>> dictionary;
}