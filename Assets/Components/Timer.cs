using UnityEngine;

/// <summary>
/// Component containing the starting time of the game (when the player starts to read intro text) used to calculate game duration
/// </summary>
public class Timer : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    /// <summary>
    /// Starting time of the game (when the player starts to read intro text) used to calculate game duration
    /// </summary>
    public float startingTime = float.MaxValue;
}