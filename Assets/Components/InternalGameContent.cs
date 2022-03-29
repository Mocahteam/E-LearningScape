using System;

/// <summary>
/// Content of the game loaded from the file "InternalData.txt" by the system "LaodGameContent".
/// </summary>
[Serializable]
public class InternalGameContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    
    /// <summary>
    /// If true, traces with MonitoringManager and Laalys will be enabled
    /// </summary>
    public bool trace = true;
    /// <summary>
    /// If true, HelpSystem will be enabled
    /// </summary>
    public bool helpSystem = true;
    /// <summary>
    /// If true, it will be determined randomly if help system has to be enabled
    /// </summary>
    public bool randomHelpSystemActivation = false;
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
    /// It true, dream fragments will be viewed in IAR. If false,
    /// dream fragments have to be printed inside envelops
    /// </summary>
    public bool virtualDreamFragment = true;
    /// <summary>
    /// If true, puzzles will be included in the game.
    /// Else dream fragments will replace collectable puzzles
    /// </summary>
    public bool virtualPuzzle = true;
    /// <summary>
    /// If false, after answering the last question and leaving IAR,
    /// the ending text will be displayed without going to the last room
    /// </summary>
    public bool useEndRoom;
    /// <summary>
    /// If false, save and load button will be disabled
    /// </summary>
    public bool saveAndLoadProgression;
    /// <summary>
    /// If false, no auto save will be made.
    /// autoSaveProgression will be set to false if saveAndLoadProgression is false
    /// </summary>
    public bool autoSaveProgression;
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

    #region UI texts
    public string loadingText;
    public string mainMenuTuto;
    public string mainMenuStart;
    public string mainMenuLoad;
    public string mainMenuOption;
    public string mainMenuCredits;
    public string mainMenuLeave;
    public string MovingModeTitle;
    public string MovingModeFPSButton;
    public string MovingModeTeleportButton;
    public string MovingModeUIButton;
    public string MovingModeFPSComment;
    public string MovingModeTeleportComment;
    public string MovingModeUIComment;
    public string MovingModeFPSNotif;
    public string MovingModeTeleportNotif;
    public string MovingModeUINotif;
    public string ViewingModeTitle;
    public string ViewingModeFPSButton;
    public string ViewingModeTPSButton;
    public string ViewingModeFPSComment;
    public string ViewingModeTPSComment;
    public string sessionIDText;
    public string sessionIDPopup;
    public string endLinkButtonText;
    public string endLeaveButtonText;
    public string optionControlsMenu;
    public string optionVirtualFragments;
    public string optionMovingTexts;
    public string optionMoveSpeed;
    public string optionCameraSensitivity;
    public string optionLockWheelSpeed;
    public string optionInputs;
    public string optionSoundMenu;
    public string optionGameMusicVolume;
    public string optionSoundEffectsVolume;
    public string optionDisplayMenu;
    public string optionResolution;
    public string optionQuality;
    public string optionWindowed;
    public string optionFont;
    public string optionCursorSize;
    public string optionLightIntensity;
    public string optionTransparency;
    public string optionValidateAndReturn;
    public string optionDefault;
    public string loadPopupLoadButton;
    public string loadPopupCancelButton;
    public string validateOptions;
    public string storyClickToContinue;
    public string hudMovingMode;
    public string hudZoom;
    public string hudObserve;
    public string hudMove;
    public string hudCrouch;
    public string hudTeleport;
    public string hudTurnLeft;
    public string hudMoveForward;
    public string hudMoveBackward;
    public string hudTurnRight;
    public string hudInventory;
    public string hudDreamFragments;
    public string hudQuestions;
    public string hudHelp;
    public string hudMenu;
    public string disableTimer;
    public string EscapeToQuit;
    public string dreamFragmentText;
    public string dreamFragmentValidation;
    public string dreamFragmentVirtualReset;
    public string iarTabInventory;
    public string iarTabDreamFragments;
    public string iarTabHelp;
    public string iarTabMenu;
    public string iarTabQuestions1;
    public string iarTabQuestions2;
    public string iarTabQuestions3;
    public string questionValidationButton;
    public string hintButtonText;
    public string NoHintAvailable;
    public string getHintButton;
    public string hintOpenURL;
    public string gameMenuResumeButton;
    public string gameMenuSaveButton;
    public string gameMenuSaveNotice;
    public string gameMenuOptionButton;
    public string gameMenuRestartButton;
    public string gameMenuLeaveButton;
    public string savePopupSaveButton;
    public string savePopupCancelButton;
    public string savePopupPlaceholder;
    public string savePopupInvalidText;
    public string savePopupInvalidButton;
    public string savePopupOverrideText;
    public string savePopupOverrideYesButton;
    public string savePopupOverrideNoButton;
    public string savePopupDoneText;
    public string savePopupDoneButton;
    public string CreditCloseButton;
    public string tutoText0;
    public string tutoText1;
    public string tutoText2;
    public string tutoText3;
    public string tutoText4;
    #endregion
	
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
}