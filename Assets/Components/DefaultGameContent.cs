using UnityEngine;
using System.IO;

public class DefaultGameContent : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public TextAsset jsonFile;

    public TextAsset lrsConfigFile;

    public TextAsset hintsJsonFile;
    public TextAsset internalHintsJsonFile;
    public TextAsset wrongAnswerFeedbacks;
    public TextAsset enigmasWeight;
    public TextAsset labelWeights;

    public TextAsset helpSystemConfig;

    public TextAsset dreamFragmentlinks;

    public Texture2D[] glassesPictures;
    public Texture2D[] lampPictures;
    public Texture2D plankPicture;
    public Texture2D puzzlePicture;

    public Sprite noPictureFound;
}