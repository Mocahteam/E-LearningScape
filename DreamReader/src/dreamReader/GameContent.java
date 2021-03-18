package dreamReader;

import java.util.List;

public class GameContent {
    /// <summary>
    /// The pedagogic theme of the game content
    /// </summary>
    public String theme;
    /// <summary>
    /// If true, traces with MonitoringManager and Laalys will be enabled
    /// </summary>
    public boolean trace = true;
    /// <summary>
    /// If true, HelpSystem will be enabled
    /// </summary>
    public boolean helpSystem = true;
    /// <summary>
    /// If true, it will be determined randomly if help system has to be enabled
    /// </summary>
    public boolean randomHelpSystemActivation = false;
    /// <summary>
    /// If true, traces will be sent to LRS
    /// </summary>
    public boolean traceToLRS = false;
    /// <summary>
    /// Frequency to which movement statements will be sent to LRS.
    /// If 0 or negative, no statment about movement sent
    /// </summary>
    public float traceMovementFrequency = 0;
    /// <summary>
    /// It true, dream fragments will be viewed in IAR. If false,
    /// dream fragments have to be printed inside envelops
    /// </summary>
    public boolean virtualDreamFragment = true;
    /// <summary>
    /// If true, puzzles will be included in the game.
    /// Else dream fragments will replace collectable puzzles
    /// </summary>
    public boolean virtualPuzzle = true;
    /// <summary>
    /// If false, after answering the last question and leaving IAR,
    /// the ending text will be displayed without going to the last room
    /// </summary>
    public boolean useEndRoom;
    /// <summary>
    /// If false, save and load button will be disabled
    /// </summary>
    public boolean saveAndLoadProgression;
    /// <summary>
    /// If false, no auto save will be made.
    /// autoSaveProgression will be set to false if saveAndLoadProgression is false
    /// </summary>
    public boolean autoSaveProgression;
    /// <summary>
    /// If true, remove extra geaometries. This is an additionnal option for very poor hardware
    /// </summary>
    public boolean removeExtraGeometries = true;

    //Paths for other config files
    public String lrsConfigPath;
    public String dreamFragmentLinksPath;
    public String dreamFragmentDocumentsPathFile;
    public String hintsPath;
    public String internalHintsPath;
    public String wrongAnswerFeedbacksPath;
    public String enigmasWeightPath;
    public String labelWeightsPath;
    public String helpSystemConfigPath;

    //Additional logos
    public String[] additionalLogosPath;

    //Texte intro, transition, fin
    public String[] storyTextIntro;
    public String[] storyTextransition;
    public String[] storyTextEnd;
    public String[] additionalCredit;
    public String scoreText;
    public String endExplainationText;
    public String endLink;
    public boolean concatIdToLink;

    // UI texts
    public String loadingText;
    public String mainMenuStart;
    public String mainMenuLoad;
    public String mainMenuOption;
    public String mainMenuCredits;
    public String mainMenuLeave;
    public String sessionIDText;
    public String sessionIDPopup;
    public String endLinkButtonText;
    public String endLeaveButtonText;
    public String optionControlsMenu;
    public String optionVirtualFragments;
    public String optionMovingTexts;
    public String optionMoveSpeed;
    public String optionCameraSensitivity;
    public String optionLockWheelSpeed;
    public String optionInputs;
    public String optionSoundMenu;
    public String optionGameMusicVolume;
    public String optionSoundEffectsVolume;
    public String optionDisplayMenu;
    public String optionFont;
    public String optionCursorSize;
    public String optionLightIntensity;
    public String optionTransparency;
    public String optionValidateAndReturn;
    public String optionDefault;
    public String loadPopupLoadButton;
    public String loadPopupCancelButton;
    public String validateOptions;
    public String storyClickToContinue;
    public String hudObserve;
    public String hudMove;
    public String hudCrouch;
    public String hudInventory;
    public String hudDreamFragments;
    public String hudQuestions;
    public String hudHelp;
    public String hudMenu;
    public String dreamFragmentText;
    public String dreamFragmentValidation;
    public String dreamFragmentVirtualReset;
    public String iarTabInventory;
    public String iarTabDreamFragments;
    public String iarTabHelp;
    public String iarTabMenu;
    public String iarTabQuestions1;
    public String iarTabQuestions2;
    public String iarTabQuestions3;
    public String iarHelpGears;
    public String masterMindPlaceholder;
    public String masterMindPasswordText;
    public String masterMindValidation;
    public String questionValidationButton;
    public String hintButtonText;
    public String getHintButton;
    public String hintOpenURL;
    public String gameMenuResumeButton;
    public String gameMenuSaveButton;
    public String gameMenuSaveNotice;
    public String gameMenuOptionButton;
    public String gameMenuRestartButton;
    public String gameMenuLeaveButton;
    public String savePopupSaveButton;
    public String savePopupCancelButton;
    public String savePopupPlaceholder;
    public String savePopupInvalidText;
    public String savePopupInvalidButton;
    public String savePopupOverrideText;
    public String savePopupOverrideYesButton;
    public String savePopupOverrideNoButton;
    public String savePopupDoneText;
    public String savePopupDoneButton;
    public String CreditCloseButton;
    public String tutoText0;
    public String tutoText1;
    public String tutoText2;
    public String tutoText3;
    public String tutoText4;

    // Inputs texts
    public String inputsSetTitle1;
    public String inputsSetTitle2;
    public String inputsSetTitle3;
    public String inputObserve;
    public String inputObserveKey1;
    public String inputObserveKey2;
    public String inputObserveKey3;
    public String inputForward;
    public String inputForwardKey1;
    public String inputForwardKey2;
    public String inputForwardKey3;
    public String inputBack;
    public String inputBackKey1;
    public String inputBackKey2;
    public String inputBackKey3;
    public String inputLeft;
    public String inputLeftKey1;
    public String inputLeftKey2;
    public String inputLeftKey3;
    public String inputRight;
    public String inputRightKey1;
    public String inputRightKey2;
    public String inputRightKey3;
    public String inputCrouch;
    public String inputCrouchKey1;
    public String inputCrouchKey2;
    public String inputCrouchKey3;
    public String inputInteract;
    public String inputInteractKey1;
    public String inputInteractKey2;
    public String inputInteractKey3;
    public String inputInventory;
    public String inputInventoryKey1;
    public String inputInventoryKey2;
    public String inputInventoryKey3;
    public String inputDreamFragments;
    public String inputDreamFragmentsKey1;
    public String inputDreamFragmentsKey2;
    public String inputDreamFragmentsKey3;
    public String inputQuestions;
    public String inputQuestionsKey1;
    public String inputQuestionsKey2;
    public String inputQuestionsKey3;
    public String inputHelp;
    public String inputHelpKey1;
    public String inputHelpKey2;
    public String inputHelpKey3;
    public String inputMenu;
    public String inputMenuKey1;
    public String inputMenuKey2;
    public String inputMenuKey3;
    public String inputView;
    public String inputViewKey1;
    public String inputViewKey2;
    public String inputViewKey3;
    public String inputTarget;
    public String inputTargetKey1;
    public String inputTargetKey2;
    public String inputTargetKey3;
    public String inputZoomIn;
    public String inputZoomInKey1;
    public String inputZoomInKey2;
    public String inputZoomInKey3;
    public String inputZoomOut;
    public String inputZoomOutKey1;
    public String inputZoomOutKey2;
    public String inputZoomOutKey3;

    //Inventory texts
    public List<String> inventoryScrollIntro;
    public List<String> inventoryKeyBallBox;
    public List<String> inventoryWire;
    public List<String> inventoryKeySatchel;
    public List<String> inventoryScrolls;
    public List<String> inventoryGlasses1;
    public List<String> inventoryGlasses2;
    public List<String> inventoryMirror;
    public List<String> inventoryLamp;
    public List<String> inventoryPuzzle;

    //Ball Box
    public boolean ballRandomPositioning;
    public String ballBoxQuestion;
    public String ballBoxPlaceHolder;
    public String ballBoxAnswerFeedback;
    public String ballBoxAnswerFeedbackDesc;
    public List<String> ballBoxAnswer;
    public int[] ballBoxThreeUsefulBalls = new int[3];
    public String[] ballTexts = new String[10];

    //Plank And Wire
    public String plankAndWireQuestionIAR;
    public String plankAndWirePlaceHolder;
    public String plankAndWireAnswerFeedback;
    public String plankAndWireAnswerFeedbackDesc;
    public String plankAndWireQuestion;
    public String[] plankAndWireCorrectWords = new String[3];
    public List<String> plankAndWireCorrectNumbers;
    public String[] plankOtherWords = new String[10];
    public List<String> plankAndWireOtherNumbers;

    //Crouch enigma
    public String crouchQuestion;
    public String crouchPlaceHolder;
    public List<String> crouchAnswer;
    public String crouchAnswerFeedback;
    public String crouchAnswerFeedbackDesc;
    public String[] crouchWords = new String[6];

    //Gears
    public String gearsQuestion;
    public String gearTextUp;
    public String gearTextDown;
    public String[] gearMovableTexts = new String[4];
    public String gearAnswer;

    //mastermind room 1 -> 2
    public String mastermindQuestion;
    public int mastermindQuestionYPos;
    public int mastermindAnswer;
    public String mastermindBackgroundPicturePath;

    //Glasses
    public String glassesQuestion;
    public String glassesPlaceHolder;
    public List<String> glassesAnswer;
    public String glassesAnswerFeedback;
    public String glassesAnswerFeedbackDesc;
    public String[] glassesPicturesPath = new String[4];

    //Enigma 8 (3 dream fragments)
    public String enigma08Question;
    public String enigma08PlaceHolder;
    public List<String> enigma08Answer;
    public String enigma08AnswerFeedback;
    public String enigma08AnswerFeedbackDesc;

    //Scrolls
    public String scrollsQuestion;
    public String scrollsPlaceHolder;
    public List<String> scrollsAnswer;
    public String scrollsAnswerFeedback;
    public String scrollsAnswerFeedbackDesc;
    public String[] scrollsWords = new String[5];

    //Mirror
    public String mirrorQuestion;
    public String mirrorPlaceHolder;
    public List<String> mirrorAnswer;
    public String mirrorAnswerFeedback;
    public String mirrorAnswerFeedbackDesc;
    public String mirrorPicturePath;

    //Enigma 11 (1 dream fragment)
    public String enigma11Question;
    public String enigma11PlaceHolder;
    public List<String> enigma11Answer;
    public String enigma11AnswerFeedback;
    public String enigma11AnswerFeedbackDesc;

    //Enigma 12 (3 dream fragments)
    public String enigma12Question;
    public String enigma12PlaceHolder;
    public List<String> enigma12Answer;
    public String enigma12AnswerFeedback;
    public String enigma12AnswerFeedbackDesc;

    //Lock Room 2
    public int lockRoom2Password;

    //Puzzle (or 5 dream fragments)
    public String puzzleAnswer;
    public String puzzleAnswerFeedback;
    public String puzzleAnswerFeedbackDesc;
    public String puzzlePicturePath;

    //Enigma 13 (2 dream fragments)
    public String enigma16Answer;
    public String enigma16AnswerFeedback;
    public String enigma16AnswerFeedbackDesc;

    //Lamp
    public String lampAnswer;
    public String lampAnswerFeedback;
    public String lampAnswerFeedbackDesc;
    public String[] lampPicturesPath = new String[6];

    //White Board
    public String whiteBoardAnswer;
    public String whiteBoardAnswerFeedback;
    public String whiteBoardAnswerFeedbackDesc;
    public String[] whiteBoardWords = new String [12];
}
