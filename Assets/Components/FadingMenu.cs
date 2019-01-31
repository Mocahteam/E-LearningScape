using UnityEngine;

/// <summary>
/// Component used to show the menu at the beginning after logos by fading
/// </summary>
public class FadingMenu : MonoBehaviour {
    // Advice: FYFY component aimsto contain only public members (according to Entity-Component-System paradigm).
    /// <summary>
    /// Final alpha of the UI to which this component is attached.
    /// The value is reached at the end of the fading
    /// </summary>
    public float finalAlpha = 255;
}