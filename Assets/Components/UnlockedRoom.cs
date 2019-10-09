using UnityEngine;

/// <summary>
/// Contains the id of the last room unlocked
/// </summary>
public class UnlockedRoom : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    /* 0 - Intro
     * 1 - Room 1
     * 2 - Room 2
     * 3 - Room 3
     * 4 - End room
     */
    public int roomNumber = 0;
}