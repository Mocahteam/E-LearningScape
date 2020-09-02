using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Content of the game loaded from the file "Data/Data_LearningScape.txt" by the system "LaodGameContent".
/// If the file is not found the default one set in "DefaultGameContent" is used
/// </summary>
[Serializable]
public class GameContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    
    /// <summary>
    /// The pedagogic theme of the game content
    /// </summary>
    public string theme;
    /// <summary>
    /// If true, puzzles will be included in the game.
    /// Else dream fragments will replace collectable puzzles
    /// </summary>
    public bool virtualPuzzle = true;
    /// <summary>
    /// If true, remove extra geaometries. This is an additionnal option for very poor hardware
    /// </summary>
    public bool removeExtraGeometries = true;

    //Paths for other config files
    public string dreamFragmentLinksPath;

    //Additional logos
    public string[] additionalLogosPath;

    //Texte intro, transition, fin
    public string[] storyTextIntro;
    public string[] storyTextransition;
    public string[] storyTextEnd;
    public string[] additionalCredit;
    public string scoreText;

    //Inventory texts 
    public List<string> inventoryScrollIntro;
    public List<string> inventoryWire;
    public List<string> inventoryScrolls;
    public List<string> inventoryGlasses1;
    public List<string> inventoryGlasses2;
    public List<string> inventoryMirror;

    //Plank And Wire
    public string plankAndWireQuestionIAR;
    public string plankAndWirePlaceHolder;
    public string plankAndWireAnswerFeedback;
    public string plankAndWireAnswerFeedbackDesc;
    public string plankAndWireQuestion;
    public string[] plankAndWireCorrectWords = new string[3];
    public List<string> plankAndWireCorrectNumbers;
    public string[] plankOtherWords = new string[10];
    public List<string> plankAndWireOtherNumbers;

    //Crouch enigma
    public string crouchQuestion;
    public string crouchPlaceHolder;
    public List<string> crouchAnswer;
    public string crouchAnswerFeedback;
    public string crouchAnswerFeedbackDesc;
    public string[] crouchWords = new string[6];

    //Gears
    public string gearsQuestion;
    public string gearTextUp;
    public string gearTextDown;
    public string[] gearMovableTexts = new string[4];
    public string gearAnswer;

    //mastermind room 1 -> 2
    public string mastermindQuestion;
    public int mastermindQuestionYPos;
    public int mastermindAnswer;
    public string mastermindBackgroundPicturePath;

    //Glasses
    public string glassesQuestion;
    public string glassesPlaceHolder;
    public List<string> glassesAnswer;
    public string glassesAnswerFeedback;
    public string glassesAnswerFeedbackDesc;
    public string glassesAnswerDescription;
    public string[] glassesPicturesPath = new string[4];

    //Scrolls
    public string scrollsQuestion;
    public string scrollsPlaceHolder;
    public List<string> scrollsAnswer;
    public string scrollsAnswerFeedback;
    public string scrollsAnswerFeedbackDesc;
    public string[] scrollsWords = new string[5];

    //Mirror
    public string mirrorQuestion;
    public string mirrorPlaceHolder;
    public List<string> mirrorAnswer;
    public string mirrorAnswerFeedback;
    public string mirrorAnswerFeedbackDesc;
    public string mirrorPicturePath;

    //Lock Room 2
    public int lockRoom2Password;
}