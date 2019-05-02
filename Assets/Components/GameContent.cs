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
    /// If true, traces with MonitoringManager and Laalys will be enabled
    /// </summary>
    public bool trace = true;
    /// <summary>
    /// If true, HelpSystem will be enabled
    /// </summary>
    public bool helpSystem = true;
    /// <summary>
    /// If true, traces will be sent to LRS
    /// </summary>
    public bool traceToLRS = false;
    /// <summary>
    /// Frequency to which movement statements will be sent to LRS.
    /// If 0 or negative, no statment about movement sent
    /// </summary>
    public float traceMovementFrequency = 0;
    /// <summary>
    /// If true, puzzles will be included in the game.
    /// Else dream fragments will replace collectable puzzles
    /// </summary>
    public bool virtualPuzzle = true;

    //Paths for other config files
    #region Config Files Paths
    public string lrsConfigPath;
    public string dreamFragmentLinksPath;
    public string hintsPath;
    public string internalHintsPath;
    public string wrongAnswerFeedbacksPath;
    public string enigmasWeightPath;
    public string labelWeightsPath;
    public string helpSystemConfigPath;
    #endregion

    //Additional logos
    public string[] additionalLogosPath;

    //Texte intro, transition, fin
    public string[] storyTextIntro;
    public string[] storyTextransition;
    public string[] storyTextEnd;
    public string[] additionalCredit;
    public string scoreText;

    //Ball Box
    public string ballBoxQuestion;
    public string ballBoxPlaceHolder;
    public string ballBoxAnswerFeedback;
    public string ballBoxAnswerFeedbackDesc;
    public List<string> ballBoxAnswer;
    public string[] ballTexts = new string[10];

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

    //Green Fragments
    public string greenFragmentsQuestion;
    public string greenFragmentPlaceHolder;
    public List<string> greenFragmentAnswer;
    public string greenFragmentAnswerFeedback;
    public string greenFragmentAnswerFeedbackDesc;
    public string[] greenFragmentsWords = new string[6];

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

    //Enigma 6 (3 dream fragments)
    public string enigma08Question;
    public string enigma08PlaceHolder;
    public List<string> enigma08Answer;
    public string enigma08AnswerFeedback;
    public string enigma08AnswerFeedbackDesc;

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

    //Enigma 9 (1 dream fragment)
    public string enigma11Question;
    public string enigma11PlaceHolder;
    public List<string> enigma11Answer;
    public string enigma11AnswerFeedback;
    public string enigma11AnswerFeedbackDesc;

    //Enigma 10 (3 dream fragments)
    public string enigma12Question;
    public string enigma12PlaceHolder;
    public List<string> enigma12Answer;
    public string enigma12AnswerFeedback;
    public string enigma12AnswerFeedbackDesc;

    //Lock Room 2
    public int lockRoom2Password;

    //Puzzle (or 5 dream fragments)
    public string puzzleAnswer;
    public string puzzlePicturePath;

    //Enigma 13 (2 dream fragments)
    public string enigma16Answer;

    //Lamp
    public string lampAnswer;
    public string[] lampPicturesPath = new string[6];

    //White Board
    public string whiteBoardAnswer;
    public string[] whiteBoardWords = new string [12];
}