using System.Collections.Generic;
using System;

/// <summary>
/// Content of the game loaded from the file "Data_LearningScape.txt" by the system "LaodGameContent".
/// </summary>
[Serializable]
public class GameContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /// <summary>
    /// The theme of the game content
    /// </summary>
    public string theme;

    //Additional logos
    public string[] additionalLogosPath;

    //Texte intro, transition, fin
    public string[] storyTextIntro;
    public string[] storyTextransition;
    public string[] storyTextEnd;
    public string[] additionalCredit;
    public string scoreText;
    public string endExplainationText;
    public string endLink;
    public string debriefingLink;
    public bool concatIdToLink;

    //Inventory texts 
    public List<string> inventoryScrollIntro;
    public List<string> inventoryKeyBallBox;
    public List<string> inventoryWire;
    public List<string> inventoryKeySatchel;
    public List<string> inventoryScrolls;
    public List<string> inventoryGlasses1;
    public List<string> inventoryGlasses2;
    public List<string> inventoryMirror;
    public List<string> inventoryLamp;
    public List<string> inventoryPuzzle;

    //Ball Box
    public bool ballRandomPositioning;
    public string ballBoxQuestion;
    public string ballBoxPlaceHolder;
    public string ballBoxAnswerFeedback;
    public string ballBoxAnswerFeedbackDesc;
    public List<string> ballBoxAnswer;
    public int[] ballBoxThreeUsefulBalls = new int[3];
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
    public string iarHelpGears;

    //mastermind room 1 -> 2
    public string mastermindQuestion;
    public string masterMindPlaceholder;
    public string masterMindPasswordText;
    public string masterMindValidation;
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

    //Enigma 8 (3 dream fragments)
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

    //Enigma 11 (1 dream fragment)
    public string enigma11Question;
    public string enigma11PlaceHolder;
    public List<string> enigma11Answer;
    public string enigma11AnswerFeedback;
    public string enigma11AnswerFeedbackDesc;

    //Enigma 12 (3 dream fragments)
    public string enigma12Question;
    public string enigma12PlaceHolder;
    public List<string> enigma12Answer;
    public string enigma12AnswerFeedback;
    public string enigma12AnswerFeedbackDesc;

    //Lock Room 2
    public int lockRoom2Password;

    //Puzzle (or 5 dream fragments)
    public string puzzleAnswer;
    public string puzzleAnswerFeedback;
    public string puzzleAnswerFeedbackDesc;
    public string puzzlePicturePath;

    //Enigma 13 (2 dream fragments)
    public string enigma16Answer;
    public string enigma16AnswerFeedback;
    public string enigma16AnswerFeedbackDesc;

    //Lamp
    public string lampAnswer;
    public string lampAnswerFeedback;
    public string lampAnswerFeedbackDesc;
    public string[] lampPicturesPath = new string[6];

    //White Board
    public string whiteBoardAnswer;
    public string whiteBoardAnswerFeedback;
    public string whiteBoardAnswerFeedbackDesc;
    public string[] whiteBoardWords = new string [12];
}