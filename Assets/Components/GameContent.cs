using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    public string theme;
    public bool trace = true;
    public bool helpSystem = true;
    public bool traceToLRS = false;
    public float traceMovementFrequency = 0;
    public bool virtualPuzzle = true;
    public string lrsConfigPath;
    public string dreamFragmentLinksPath;
    public string hintsPath;
    public string internalHintsPath;
    public string wrongAnswerFeedbacksPath;
    public string enigmasWeightPath;
    public string labelWeightsPath;
    public string helpSystemConfigPath;

    //Texte intro, transition, fin
    public string[] storyTextIntro;
    public string[] storyTextransition;
    public string[] storyTextEnd;

    //Ball Box
    public string ballBoxQuestion;
    public string ballBoxPlaceHolder;
    public List<string> ballBoxAnswer;
    public string[] ballTexts = new string[10];

    //Plank And Wire
    public string plankAndWireQuestion;
    public string plankAndWirePlaceHolder;
    public string plankAndWireQuestionIAR;
    public string[] plankAndWireCorrectWords = new string[3];
    public int[] plankAndWireCorrectNumbers = new int[3];
    public string[] plankOtherWords = new string[10];
    public int[] plankAndWireOtherNumbers = new int[6];

    //Green Fragments
    public string greenFragmentsQuestion;
    public string greenFragmentPlaceHolder;
    public string[] greenFragmentsWords = new string[6];
    public List<string> greenFragmentAnswer;

    //Gears
    public string gearsQuestion;
    public string gearTextUp;
    public string gearTextDown;
    public string[] gearMovableTexts = new string[4];
    public string gearAnswer;

    //mdp login room 1 -> 2
    public int mdpLogin;

    //Glasses
    public string glassesQuestion;
    public string glassesPlaceHolder;
    public string[] glassesPicturesPath = new string[4];
    public List<string> glassesAnswer;

    //Enigma 6 (3 dream fragments)
    public string enigma6Question;
    public string enigma6PlaceHolder;
    public List<string> enigma6Answer;
    public string enigma6AnswerDescription;

    //Scrolls
    public string scrollsQuestion;
    public string scrollsPlaceHolder;
    public List<string> scrollsAnswer;
    public string[] scrollsWords = new string[5];

    //Mirror
    public string mirrorQuestion;
    public string mirrorPlaceHolder;
    public List<string> mirrorAnswer;
    public string mirrorPicturePath;

    //Enigma 9 (1 dream fragment)
    public string enigma9Question;
    public string enigma9PlaceHolder;
    public List<string> enigma9Answer;

    //Enigma 10 (3 dream fragments)
    public string enigma10Question;
    public string enigma10PlaceHolder;
    public List<string> enigma10Answer;

    //Lock Room 2
    public int lockRoom2Password;

    //Puzzle (or 5 dream fragments)
    public bool puzzle;
    public string puzzlePicturePath;
    public string puzzleAnswer;

    //Enigma 12 (2 dream fragments)
    public string enigma12Answer;

    //Lamp
    public string[] lampPicturesPath = new string[6];
    public string lampAnswer;

    //White Board
    public string[] whiteBoardWords = new string [12]; //"AMENAGER L'ESPAC##E##"
    public string whiteBoardAnswer;
}