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
    /// File used to set addresses to which statements will be sent
    /// </summary>
    public TextAsset lrsConfigFile;

    /// <summary>
    /// File containing hints about game content
    /// </summary>
    public TextAsset hintsJsonFile;
    /// <summary>
    /// File containing hints about game mechanics
    /// </summary>
    public TextAsset internalHintsJsonFile;
    /// <summary>
    /// File containing hints given when the player give a wrong answer
    /// </summary>
    public TextAsset wrongAnswerFeedbacks;
    /// <summary>
    /// File containing enigma weights used in the HelpSystem to calculate the player progression
    /// </summary>
    public TextAsset enigmasWeight;
    /// <summary>
    /// File containing label weights used in the HelpSystem to calculate the player trouble
    /// </summary>
    public TextAsset labelWeights;
    
    public TextAsset helpSystemConfig;

    /// <summary>
    /// File containing links for dreamFragments.
    /// If a link is filled, when the player accesses to the correcponding dreamFragment he can open the link from a button
    /// </summary>
    public TextAsset dreamFragmentlinks;
    /// <summary>
    /// File containing paths to dream fragments' documents for the virtual version
    /// Several paths can be given for a same dream fragment
    /// When the player clicks on a dream fragment button in the IAR, all documents corresponding to the dream fragment will be displayed
    /// </summary>
    public TextAsset dreamFragmentDocuments;
    /// <summary>
    /// Pictures for the dream fragments in IAR
    /// </summary>
    public Texture2D[] dreamFragmentPictures;

    /// <summary>
    /// Picture for the Login panel (mastermind)
    /// </summary>
    public Texture2D mastermindPicture;
    /// <summary>
    /// Pictures for the glasses enigma
    /// </summary>
    public Texture2D[] glassesPictures;
    /// <summary>
    /// Pictures for the lamp enigma
    /// </summary>
    public Texture2D[] lampPictures;
    /// <summary>
    /// Picture for the PlankAndMirror enigma
    /// </summary>
    public Texture2D plankPicture;
    /// <summary>
    /// Picture for the puzzle
    /// </summary>
    public Texture2D puzzlePicture;

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