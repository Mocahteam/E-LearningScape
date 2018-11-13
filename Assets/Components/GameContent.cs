using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    public string theme;
    public bool trace = true;

    //Texte intro, transition, fin
    public string[] storyTextIntro;
    public string[] storyTextransition;
    public string[] storyTextEnd;

    //Ball Box
    public string ballBoxQuestion;
    public List<string> ballBoxAnswer;
    public string[] ballCorrectTexts = new string[3];
    public string[] ballWrongTexts = new string[12];

    //Plank And Wire
    public string plankAndWireQuestion;
    public string plankAndWireQuestionIAR;
    public string[] plankAndWireCorrectWords = new string[3];
    public int[] plankAndWireCorrectNumbers = new int[3];
    public string[] plankOtherWords = new string[10];
    public int[] plankAndWireOtherNumbers = new int[6];

    //Green Fragments
    public string greenFragmentsQuestion;
    public string[] greenFragmentsWords = new string[6];
    public string greenFragmentAnswer; //list

    //Gears
    public string gearsQuestion;
    public string gearTextUp;
    public string gearTextDown;
    public string[] gearMovableTexts = new string[4];
    public string gearAnswer;

    //mdp login room 1 -> 2
    public int mdpLogin;

    //Glasses
    //Question
    public string[] glassesPicturesPath = new string[4];
    public string glassesAnswer; //list

    //Enigma 6 (3 dream fragments)
    public string enigma6Question;
    public List<string> enigma6Answer;
    public string enigma6AnswerDescription;

    //Scrolls
    public string scrollsQuestion;
    public List<string> scrollsAnswer;
    public string[] scrollsWords = new string[5];

    //Mirror
    public string mirrorQuestion;
    public List<string> mirrorAnswer;
    public string mirrorPicturePath;

    //Enigma 9 (1 dream fragment)
    public string enigma9Question;
    public List<string> enigma9Answer;

    //Enigma 10 (3 dream fragments)
    public string enigma10Question;
    public List<string> enigma10Answer;

    //Lock Room 2
    public int lockRoom2Password;

    //Puzzle (or 5 dream fragments)
    public bool puzzle;
    public string puzzlePicturePath;

    //Enigma 12 (2 dream fragments)

    //Lamp
    public string[] lampPicturesPath = new string[6];

    //White Board
    public string[] whiteBoardWords = new string [12]; //"AMENAGER L'ESPAC##E##"

    public string[] room3Answers = new string[4];
}