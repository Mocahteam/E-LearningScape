using UnityEngine;
using System.IO;
using TMPro;

/// <summary>
/// Contains the default settings file of the game.
/// If the configuration file is not found, these files will be generated LoadGameContent.cs to their default path (defined in "jsonFile")
/// </summary>
public class DefaultGameContent : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    /// <summary>
    /// General config file containing all game content and path to other config files
    /// </summary>
    public TextAsset jsonFile;

    /// <summary>
    /// File containing links for dreamFragments.
    /// If a link is filled, when the player accesses to the correcponding dreamFragment he can open the link from a button
    /// </summary>
    public TextAsset dreamFragmentlinks;

    /// <summary>
    /// Picture for the Login panel (mastermind)
    /// </summary>
    public Texture2D mastermindPicture;
    /// <summary>
    /// Pictures for the glasses enigma
    /// </summary>
    public Texture2D[] glassesPictures;
    /// <summary>
    /// Picture for the PlankAndMirror enigma
    /// </summary>
    public Texture2D plankPicture;

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