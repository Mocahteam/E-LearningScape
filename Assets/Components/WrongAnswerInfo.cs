using UnityEngine;

/// <summary>
/// Component containing information about a wrong answer given by the player.
/// The component is created when the player answers and is used by HelpSystem to give him a feedback about the given answer
/// </summary>
public class WrongAnswerInfo : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    /// <summary>
    /// The wrong answer given by the player
    /// </summary>
    public string givenAnswer;
}