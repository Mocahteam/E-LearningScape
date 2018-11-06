using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    public string theme;

    //Texte intro, transition, fin
    public string[] storyTextIntro;
    public string[] storyTextransition;
    public string[] storyTextEnd;

    //Ball Box
    public string ballBoxQuestion;
    public List<string> ballBoxAnswer;
    public string[] ballTexts = new string[15];

    //Plank And Wire
    public string plankAndWireQuestion;
    public string plankAndWireQuestionIAR;
    public string[] plankAndWireAnswers = new string[3]; //words
    public string[] plankOtherWords = new string[10];
    //chiffres sur le tableau + réponse dans l'iar + chiffres incorrectes

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
    /*<color=red> to set the color to red. Several other colors are pre-defined.
<#FF8000> or any other hexadecimal code to define a color.
</color> to end the color tag.*/

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
    public string lampWord;

    //White Board
    public string[] whiteBoardRemovable; //"AMENAGER L'ESPAC##E##"
    public string[] whiteBoardUnremovable;

    public string[] room3Answers = new string[4];
}