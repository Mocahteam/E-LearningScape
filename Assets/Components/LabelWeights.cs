using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains weights of every label of Laalys.
/// The HelpSystem increase/decrease a label count depending on the labels returned by Laalys and their weight.
/// When the label count reaches labelCountStep it is reset to 0 and a hint is given to the player
/// </summary>
public class LabelWeights : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    /// <summary>
    /// Contains weights of every label of Laalys (key: label, value: weight).
    /// The HelpSystem increase/decrease (depending on the sign of the weight) a label count depending on the labels returned by Laalys and their weight.
    /// A correct action should have a negative weight while others should be positive or 0.
    /// When the label count reaches labelCountStep it is reset to 0 and a hint is given to the player.
    /// </summary>
    public Dictionary<string, float> weights;
}