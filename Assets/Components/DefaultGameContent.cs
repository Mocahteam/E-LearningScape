using UnityEngine;
using System.IO;
using TMPro;

/// <summary>
/// Contains the default settings file of the game.
/// </summary>
public class DefaultGameContent : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// Default picture used when the picture given in the config file is invalid
    /// </summary>
    public Sprite noPictureFound;

    /// <summary>
    /// Accessible font (World)
    /// </summary>
    public TMP_FontAsset accessibleFontTMPro;
    /// <summary>
    /// Accessible font (UI)
    /// </summary>
    public TMP_FontAsset accessibleFontTMProUI;
    /// <summary>
    /// Default font (World)
    /// </summary>
    public TMP_FontAsset defaultFontTMPro;
    /// <summary>
    /// Default font (UI)
    /// </summary>
    public TMP_FontAsset defaultFontTMProUI;
}