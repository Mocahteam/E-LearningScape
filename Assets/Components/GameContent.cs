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
    /// 
    /// </summary>
    public bool virtualDreamFragment = true;
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
    #region Config Files Paths
    public string lrsConfigPath;
    public string dreamFragmentLinksPath;
    public string dreamFragmentDocumentsPathFile;
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

    #region UI texts
    public string loadingText;
    public string mainMenuStart;
    public string mainMenuOption;
    public string mainMenuLeave;
    public string optionControlsMenu;
    public string optionMovingTexts;
    public string optionMoveSpeed;
    public string optionCameraSensitivity;
    public string optionLockWheelSpeed;
    public string optionInputs;
    public string optionSoundMenu;
    public string optionGameMusicVolume;
    public string optionSoundEffectsVolume;
    public string optionDisplayMenu;
    public string optionFont;
    public string optionCursorSize;
    public string optionLightIntensity;
    public string optionTransparency;
    public string optionValidateAndReturn;
    public string optionDefault;
    public string validateOptions;
    public string storyClickToContinue;
    public string hudObserve;
    public string hudMove;
    public string hudCrouch;
    public string hudInventory;
    public string hudDreamFragments;
    public string hudQuestions;
    public string hudHelp;
    public string hudMenu;
    public string dreamFragmentText;
    public string dreamFragmentValidation;
    public string dreamFragmentOpenURL;
    public string iarTabInventory;
    public string iarTabDreamFragments;
    public string iarTabHelp;
    public string iarTabMenu;
    public string iarTabQuestions1;
    public string iarTabQuestions2;
    public string iarTabQuestions3;
    public string iarHelpGears;
    public string masterMindPlaceholder;
    public string masterMindPasswordText;
    public string masterMindValidation;
    public string questionValidationButton;
    public string hintButtonText;
    public string getHintButton;
    public string hintOpenURL;
    public string gameMenuResumeButton;
    public string gameMenuOptionButton;
    public string gameMenuRestartButton;
    public string gameMenuLeaveButton;
    public string tutoText0;
    public string tutoText1;
    public string tutoText2;
    public string tutoText3;
    public string tutoText4;

    #region Input settings texts
    public string inputsSetTitle1;
    public string inputsSetTitle2;
    public string inputsSetTitle3;
    public string inputObserve;
    public string inputObserveKey1;
    public string inputObserveKey2;
    public string inputObserveKey3;
    public string inputForward;
    public string inputForwardKey1;
    public string inputForwardKey2;
    public string inputForwardKey3;
    public string inputBack;
    public string inputBackKey1;
    public string inputBackKey2;
    public string inputBackKey3;
    public string inputLeft;
    public string inputLeftKey1;
    public string inputLeftKey2;
    public string inputLeftKey3;
    public string inputRight;
    public string inputRightKey1;
    public string inputRightKey2;
    public string inputRightKey3;
    public string inputCrouch;
    public string inputCrouchKey1;
    public string inputCrouchKey2;
    public string inputCrouchKey3;
    public string inputInteract;
    public string inputInteractKey1;
    public string inputInteractKey2;
    public string inputInteractKey3;
    public string inputInventory;
    public string inputInventoryKey1;
    public string inputInventoryKey2;
    public string inputInventoryKey3;
    public string inputDreamFragments;
    public string inputDreamFragmentsKey1;
    public string inputDreamFragmentsKey2;
    public string inputDreamFragmentsKey3;
    public string inputQuestions;
    public string inputQuestionsKey1;
    public string inputQuestionsKey2;
    public string inputQuestionsKey3;
    public string inputHelp;
    public string inputHelpKey1;
    public string inputHelpKey2;
    public string inputHelpKey3;
    public string inputMenu;
    public string inputMenuKey1;
    public string inputMenuKey2;
    public string inputMenuKey3;
    public string inputView;
    public string inputViewKey1;
    public string inputViewKey2;
    public string inputViewKey3;
    public string inputTarget;
    public string inputTargetKey1;
    public string inputTargetKey2;
    public string inputTargetKey3;
    public string inputZoomIn;
    public string inputZoomInKey1;
    public string inputZoomInKey2;
    public string inputZoomInKey3;
    public string inputZoomOut;
    public string inputZoomOutKey1;
    public string inputZoomOutKey2;
    public string inputZoomOutKey3;
    #endregion
    #endregion

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