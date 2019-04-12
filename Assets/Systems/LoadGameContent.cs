using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using FYFY_plugins.Monitoring;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;

public class LoadGameContent : FSystem {
    
    private Family f_defaultGameContent = FamilyManager.getFamily(new AllOfComponents(typeof(DefaultGameContent)));

    private Family f_storyText = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private Family f_queriesR1 = FamilyManager.getFamily(new AnyOfTags("Q-R1"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR3 = FamilyManager.getFamily(new AnyOfTags("Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family f_ballBoxTop = FamilyManager.getFamily(new AnyOfTags("BallBoxTop"));

    private Family f_plankAndWireRule = FamilyManager.getFamily(new AnyOfTags("PlankAndWireRule"));
    private Family f_wrongWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro)), new NoneOfComponents(typeof(IsSolution)));
    private Family f_correctWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro), typeof(IsSolution)));
    private Family f_plankNumbers = FamilyManager.getFamily(new AnyOfTags("PlankNumbers"));

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

    private Family f_gears = FamilyManager.getFamily(new AnyOfTags("Gears"));
    private Family f_gearComponent = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));

    private Family f_bagImage = FamilyManager.getFamily(new AllOfComponents(typeof(BagImage)));

    private Family f_scrollUI = FamilyManager.getFamily(new AnyOfTags("ScrollUI"));

    private Family f_mirrorImage = FamilyManager.getFamily(new AnyOfTags("MirrorPlankImage"), new AllOfComponents(typeof(Image)));

    private Family f_passwordRoom2 = FamilyManager.getFamily(new AnyOfTags("PasswordRoom2"));
    private Family f_lockRoom2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"), new AllOfComponents(typeof(Locker)));

    private Family f_puzzleUI = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(RectTransform)));
    private Family f_puzzles = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new NoneOfComponents(typeof(DreamFragment)));
    private Family f_puzzlesFragment = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new AllOfComponents(typeof(DreamFragment)));

    private Family f_lampPictures = FamilyManager.getFamily(new AllOfComponents(typeof(E12_Symbol)));

    private Family f_boardUnremovable = FamilyManager.getFamily(new AnyOfTags("BoardUnremovableWords"));
    private Family f_boardRemovable = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));

    private Family f_gameHints = FamilyManager.getFamily(new AllOfComponents(typeof(GameHints)));
    private Family f_internalGameHints = FamilyManager.getFamily(new AllOfComponents(typeof(InternalGameHints)));
    private Family f_labelWeights = FamilyManager.getFamily(new AllOfComponents(typeof(LabelWeights)));


    public FSystem instance;

    public static GameContent gameContent;
    public static DefaultGameContent defaultGameContent;
    public static Dictionary<string, float> enigmasWeight;
    private bool loadContent = true;

    private Texture2D tmpTex;
    private byte[] tmpFileData;
    private string[] convertedBoardText; //0- unremovable, 1- removable
    private GameObject forGO;

    public LoadGameContent()
    {
        if (Application.isPlaying && loadContent)
        {
            defaultGameContent = f_defaultGameContent.First().GetComponent<DefaultGameContent>();
            if (File.Exists("Data/Data_LearningScape.txt"))
            {
                //Load game content from the file
                Load();
            }
            else
            {
                //create default data files
                Directory.CreateDirectory("Data");
                File.WriteAllText("Data/Data_LearningScape.txt", defaultGameContent.jsonFile.text);
                File.WriteAllText("Data/LRSConfig.txt", defaultGameContent.lrsConfigFile.text);
                File.WriteAllText("Data/Hints_LearningScape.txt", defaultGameContent.hintsJsonFile.text);
                File.WriteAllText("Data/InternalHints_LearningScape.txt", defaultGameContent.internalHintsJsonFile.text);
                File.WriteAllText("Data/WrongAnswerFeedbacks.txt", defaultGameContent.wrongAnswerFeedbacks.text);
                File.WriteAllText("Data/EnigmasWeight.txt", defaultGameContent.enigmasWeight.text);
                File.WriteAllText("Data/LabelWeights.txt", defaultGameContent.labelWeights.text);
                File.WriteAllText("Data/DreamFragmentLinks.txt", defaultGameContent.dreamFragmentlinks.text);
                File.WriteAllText("Data/HelpSystemConfig.txt", defaultGameContent.helpSystemConfig.text);

                gameContent = new GameContent();
                gameContent = JsonUtility.FromJson<GameContent>(defaultGameContent.jsonFile.text);

                int l = defaultGameContent.glassesPictures.Length;
                for(int i = 0; i < l; i++)
                {
                    File.WriteAllBytes(string.Concat("Data/", defaultGameContent.glassesPictures[i].name, ".png"), defaultGameContent.glassesPictures[i].EncodeToPNG());
                }
                l = defaultGameContent.lampPictures.Length;
                for (int i = 0; i < l; i++)
                {
                    File.WriteAllBytes(string.Concat("Data/", defaultGameContent.lampPictures[i].name, ".png"), defaultGameContent.lampPictures[i].EncodeToPNG());
                }
                File.WriteAllBytes(string.Concat("Data/", defaultGameContent.plankPicture.name, ".png"), defaultGameContent.plankPicture.EncodeToPNG());
                File.WriteAllBytes(string.Concat("Data/", defaultGameContent.puzzlePicture.name, ".png"), defaultGameContent.puzzlePicture.EncodeToPNG());

                Debug.Log("Data created");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - Data created"));

                Load();
            }

            this.Pause = true;
        }
        instance = this;
    }

    private void Load()
    {
        //Load game content from the file
        gameContent = JsonUtility.FromJson<GameContent>(File.ReadAllText("Data/Data_LearningScape.txt"));

        ActionsManager.instance.Pause = !gameContent.trace;
        Debug.Log(string.Concat("Trace: ", gameContent.trace));
        File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - Trace: ", gameContent.trace));

        HelpSystem.shouldPause = !gameContent.helpSystem || !MonitoringManager.Instance.inGameAnalysis;
        Debug.Log(string.Concat("Help system: ", gameContent.helpSystem, "; Laalys in game analysis: ", MonitoringManager.Instance.inGameAnalysis));
        File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - Help system: ", gameContent.helpSystem));

        SendStatements.shouldPause = !gameContent.traceToLRS;
        SendStatements.instance.Pause = !gameContent.traceToLRS;
        MovingSystem.instance.traceMovementFrequency = gameContent.traceMovementFrequency;
        Debug.Log(string.Concat("Trace to LRS: ", gameContent.traceToLRS));
        File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - Trace to LRS: ", gameContent.traceToLRS));

        foreach (GameObject go in f_puzzles)
            GameObjectManager.setGameObjectState(go, gameContent.virtualPuzzle);
        foreach (GameObject go in f_puzzlesFragment)
            GameObjectManager.setGameObjectState(go, !gameContent.virtualPuzzle);

        #region Story
        StoryText st = f_storyText.First().GetComponent<StoryText>();
        st.intro = gameContent.storyTextIntro;
        st.transition = gameContent.storyTextransition;
        st.end = gameContent.storyTextEnd;
        #endregion

        #region Room 1
        int nbQueries = f_queriesR1.Count;
        for (int i = 0; i < nbQueries; i++)
        {
            forGO = f_queriesR1.getAt(i);
            switch (forGO.name)
            {
                case "Q1":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.ballBoxQuestion;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.ballBoxPlaceHolder;
                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Description")
                        {
                            int nbAnswers = gameContent.ballBoxAnswer.Count > 3 ? 3 : gameContent.ballBoxAnswer.Count;
                            int answerID;
                            int startingID = 1;
                            for(int j = 0; j < nbAnswers; j++)
                            {
                                int.TryParse(gameContent.ballBoxAnswer[j], out answerID);
                                if (answerID != 0)
                                {
                                    tmp.text = gameContent.ballTexts[answerID];
                                    startingID = j + 1;
                                    break;
                                }
                            }
                            for (int j = startingID; j < nbAnswers; j++)
                            {
                                int.TryParse(gameContent.ballBoxAnswer[j], out answerID);
                                if (answerID != 0)
                                    tmp.text = string.Concat(tmp.text, " - ", gameContent.ballTexts[answerID]);
                            }
                        }
                        else if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.ballBoxAnswer[0];
                            for (int j = 1; j < gameContent.ballBoxAnswer.Count; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.ballBoxAnswer[j]);
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.ballBoxAnswer)
                    {
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    }
                    break;

                case "Q2":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.plankAndWireQuestionIAR;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.plankAndWirePlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Description")
                        {
                            tmp.text = string.Concat(gameContent.plankAndWireCorrectWords[0], " - ", gameContent.plankAndWireCorrectWords[1], " - ", gameContent.plankAndWireCorrectWords[2]);
                        }
                        else if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.plankAndWireCorrectNumbers[0].ToString();
                            int l = gameContent.plankAndWireCorrectNumbers.Length;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.plankAndWireCorrectNumbers[j].ToString());
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    for (int j = 0; j < gameContent.plankAndWireCorrectNumbers.Length; j++)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(gameContent.plankAndWireCorrectNumbers[j].ToString());
                    break;

                case "Q3":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.greenFragmentsQuestion;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.greenFragmentPlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.greenFragmentsWords[0];
                            int l = gameContent.greenFragmentsWords.Length;
                            for (int j = 1; j < l; j++)
                            {
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.greenFragmentsWords[j]);
                            }
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.greenFragmentAnswer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                default:
                    break;
            }
        }

        LoginManager.passwordSolution = gameContent.mdpLogin;

        //Ball Box
        int nbBalls = f_balls.Count;
        Ball b = null;
        Ball b2 = null;
        string tmpString;
        int nbBallSeen = 0;
        int answer;
        //initialise position and texts for all balls
        List<int> idList = new List<int>();
        for (int i = 0; i < nbBalls; i++)
            idList.Add(i);
        for (int i = 0; i < nbBalls; i++)
        {
            b = f_balls.getAt(i).GetComponent<Ball>();

            //change randomly the position of the ball
            b.id = idList[(int)UnityEngine.Random.Range(0, idList.Count - 0.001f)];
            idList.Remove(b.id);

            if(b.number < gameContent.ballTexts.Length)
                b.text = gameContent.ballTexts[b.number];
        }
        //Exchange texts and numbers to set solution balls
        for(int j = 0; j < 3; j++)
        {
            //If there is still unprocessed answers
            if (gameContent.ballBoxAnswer.Count > j)
            {
                //If the answer given was integer
                if (int.TryParse(gameContent.ballBoxAnswer[j], out answer))
                {
                    //If the answer given is different than the default answer
                    if(answer != j + 1)
                    {
                        //Look for a default solution ball (j+1 can be 1, 2 or 3) and a new solution ball (number stored in "answer")
                        for (int i = 0; i < nbBalls; i++)
                        {
                            b = f_balls.getAt(i).GetComponent<Ball>();
                            if (b.number == j + 1 || b.number == answer)
                            {
                                if (nbBallSeen == 1)
                                {
                                    nbBallSeen++;
                                    break;
                                }
                                else
                                {
                                    b2 = b;
                                    nbBallSeen++;
                                }
                            }
                        }
                        //If the two balls were found
                        if (nbBallSeen == 2)
                        {
                            //exchange numbers and texts
                            if (b.number == j + 1)
                            {
                                b.number = answer;
                                b2.number = j + 1;
                            }
                            else
                            {
                                b.number = j + 1;
                                b2.number = answer;
                            }
                            tmpString = b.text;
                            b.text = b2.text;
                            b2.text = tmpString;
                        }
                        else
                        {
                            Debug.LogWarning(string.Concat("The answer ", j + 1, " of BallBox enigma should be between 1 and 15 included."));
                            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - The answer ", j + 1, " of BallBox enigma should be between 1 and 15 included"));
                        }

                        nbBallSeen = 0;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Concat("The answer ", j + 1, " of BallBox enigma should be an integer."));
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - The answer ", j + 1, " of BallBox enigma should be an integer"));
                }
            }
        }
        for (int i = 0; i < nbBalls; i++)
        {
            b = f_balls.getAt(i).GetComponent<Ball>();
            b.GetComponentInChildren<TextMeshPro>().text = b.number.ToString();
        }

            foreach (TextMeshPro tmp in f_ballBoxTop.First().GetComponentsInChildren<TextMeshPro>())
        {
            tmp.text = gameContent.ballBoxQuestion;
        }

        //Plank and Wire
        int nbWrongWords = f_wrongWords.Count;
        int nbCorrectWords = f_correctWords.Count;
        // Set Wrong words in a random position
        System.Random random = new System.Random();
        List<string> words = new List<string>(gameContent.plankOtherWords);
        for (int i = 0; i < nbWrongWords; i++)
        {
            int randPos = random.Next(words.Count);
            f_wrongWords.getAt(i).GetComponent<TextMeshPro>().text = words[randPos];
            words.RemoveAt(randPos);
        }
        // Set Correct word in a random position
        words = new List<string>(gameContent.plankAndWireCorrectWords);
        for (int i = 0; i < nbCorrectWords; i++)
        {
            int randPos = random.Next(words.Count);
            f_correctWords.getAt(i).GetComponent<TextMeshPro>().text = words[randPos];
            words.RemoveAt(randPos);
        }
        f_plankAndWireRule.First().GetComponent<TextMeshPro>().text = gameContent.plankAndWireQuestion;
        int nbPlankNumbers = f_plankNumbers.Count;
        int countCorrectNb = 0;
        int countWrongNb = 0;
        for (int i = 0; i < nbPlankNumbers; i++)
        {
            if (f_plankNumbers.getAt(i).name == "SolutionNumberA" || f_plankNumbers.getAt(i).name == "SolutionNumberB" || f_plankNumbers.getAt(i).name == "SolutionNumberC")
            {
                f_plankNumbers.getAt(i).GetComponent<TextMeshPro>().text = gameContent.plankAndWireCorrectNumbers[countCorrectNb].ToString();
                countCorrectNb++;
            }
            else
            {
                f_plankNumbers.getAt(i).GetComponent<TextMeshPro>().text = gameContent.plankAndWireOtherNumbers[countWrongNb].ToString();
                countWrongNb++;
            }
        }
        forGO = f_wrongWords.First().transform.parent.parent.gameObject;

        // Two valid rotations to avoid words and numbers overlaping
        float angle = 0f;
        if (UnityEngine.Random.value > 0.5f)
            angle = 158f;
        forGO.transform.Rotate(0, angle, 0);
        foreach (Transform child in forGO.transform)
            if (child.name != "Numbers")
                child.Rotate(0, -angle, 0);
        foreach (Transform child in forGO.transform.GetChild(0))
            child.Rotate(0, 0, -angle);

        //Green Dream Fragments
        int nbDreamFragments = f_dreamFragments.Count;
        DreamFragment tmpDF;
        int nbGreenFragments = 0;
        for (int i = 0; i < nbDreamFragments; i++)
        {
            tmpDF = f_dreamFragments.getAt(i).GetComponent<DreamFragment>();
            if (tmpDF.type == 1)
            {
                tmpDF.itemName = gameContent.greenFragmentsWords[nbGreenFragments];
                nbGreenFragments++;
                if (nbGreenFragments > 5)
                    break;
            }
        }

        //Gears
        int nbQuestionGears = 0;
        int nbAnswerGears = 0;
        bool answerGearFound = false;
        foreach (Transform child in f_gears.First().transform)
        {
            if (child.gameObject.name.Contains("Gear"))
            {
                if (child.gameObject.name.Length == 5 && nbAnswerGears < 4)
                {
                    child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = gameContent.gearMovableTexts[nbAnswerGears];
                    if (gameContent.gearMovableTexts[nbAnswerGears] == gameContent.gearAnswer)
                    {
                        child.gameObject.tag = "RotateGear";
                        child.gameObject.GetComponent<Gear>().isSolution = true;
                        answerGearFound = true;
                    }
                    else
                    {
                        child.gameObject.tag = "Untagged";
                        child.gameObject.GetComponent<Gear>().isSolution = false;
                    }
                    nbAnswerGears++;
                }
                else
                {
                    if (child.gameObject.name != "TransparentGear")
                    {
                        if (nbQuestionGears == 0)
                            child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = gameContent.gearTextUp;
                        else
                            child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = gameContent.gearTextDown;
                        nbQuestionGears++;
                    }
                }
            }
            else if (child.gameObject.name == "Q4")
                child.gameObject.GetComponent<TextMeshProUGUI>().text = gameContent.gearsQuestion;
        }
        if (!answerGearFound)
        {
            int nbGears = f_gearComponent.Count;
            Gear gear = f_gearComponent.getAt((int)UnityEngine.Random.Range(0, nbGears - 1)).GetComponent<Gear>();
            gear.isSolution = true;
            gear.tag = "RotateGear";
        }
        #endregion

        #region Room 2
        nbQueries = f_queriesR2.Count;
        for (int i = 0; i < nbQueries; i++)
        {
            forGO = f_queriesR2.getAt(i);
            switch (forGO.name)
            {
                case "Q1":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.glassesQuestion;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.glassesPlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.glassesAnswer[0];
                            int l = gameContent.glassesAnswer.Count;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.glassesAnswer[j]);
                        }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.glassesAnswer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                case "Q2":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.enigma6Question;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.enigma6PlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.enigma6Answer[0];
                            int l = gameContent.enigma6Answer.Count;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.enigma6Answer[j]);
                        }
                        else if (tmp.gameObject.name == "Description")
                            tmp.text = gameContent.enigma6AnswerDescription;
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.enigma6Answer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                case "Q3":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.scrollsQuestion;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.scrollsPlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.scrollsAnswer[0];
                            int l = gameContent.scrollsAnswer.Count;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.scrollsAnswer[j]);
                        }
                        else if (tmp.gameObject.name == "Description")
                        {
                            tmp.text = gameContent.scrollsWords[0];
                            int l = gameContent.scrollsWords.Length;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.scrollsWords[j]);
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.scrollsAnswer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                case "Q4":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.mirrorQuestion;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.mirrorPlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.mirrorAnswer[0];
                            int l = gameContent.mirrorAnswer.Count;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.mirrorAnswer[j]);
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.mirrorAnswer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                case "Q5":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.enigma9Question;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.enigma9PlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.enigma9Answer[0];
                            int l = gameContent.enigma9Answer.Count;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.enigma9Answer[j]);
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.enigma9Answer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                case "Q6":
                    foreach (TextMeshProUGUI tmp in forGO.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Question")
                        {
                            tmp.text = gameContent.enigma10Question;
                            break;
                        }
                    }
                    forGO.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<Text>().text = gameContent.enigma10PlaceHolder;

                    foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (tmp.gameObject.name == "Answer")
                        {
                            tmp.text = gameContent.enigma10Answer[0];
                            int l = gameContent.enigma10Answer.Count;
                            for (int j = 1; j < l; j++)
                                tmp.text = string.Concat(tmp.text, " - ", gameContent.enigma10Answer[j]);
                        }
                    }
                    forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                    foreach (string s in gameContent.enigma10Answer)
                        forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
                    break;

                default:
                    break;
            }
        }

        //Glasses
        BagImage bi = f_bagImage.First().GetComponent<BagImage>();
        Sprite mySprite;
        for (int i = 0; i < 4; i++)
        {
            mySprite = defaultGameContent.noPictureFound;
            if (File.Exists(gameContent.glassesPicturesPath[i]))
            {
                tmpTex = new Texture2D(1, 1);
                tmpFileData = File.ReadAllBytes(gameContent.glassesPicturesPath[i]);
                if (tmpTex.LoadImage(tmpFileData))
                {
                    mySprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
                }
            }
            switch (i)
            {
                case 0:
                    bi.image1 = mySprite;
                    break;

                case 1:
                    bi.image2 = mySprite;
                    break;

                case 2:
                    bi.image3 = mySprite;
                    break;

                case 3:
                    bi.image4 = mySprite;
                    break;

                default:
                    break;
            }
        }
        bi.gameObject.GetComponent<Image>().sprite = bi.image1;

        //Scrolls
        int nbScroll = gameContent.scrollsWords.Length < f_scrollUI.Count ? gameContent.scrollsWords.Length : f_scrollUI.Count;
        for (int i = 0; i < nbScroll; i++)
        {
            if (gameContent.scrollsWords[i] == string.Empty)
                GameObjectManager.setGameObjectState(f_scrollUI.getAt(i), false);
            else
                f_scrollUI.getAt(i).GetComponentInChildren<TextMeshProUGUI>().text = gameContent.scrollsWords[i];
        }

        //Mirror
        f_mirrorImage.First().GetComponent<Image>().sprite = defaultGameContent.noPictureFound;
        if (File.Exists(gameContent.mirrorPicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(gameContent.mirrorPicturePath);
            if (tmpTex.LoadImage(tmpFileData))
                f_mirrorImage.First().GetComponent<Image>().sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
        }

        //Lock Room 2
        Locker locker = f_lockRoom2.First().GetComponent<Locker>();
        locker.wheel1Solution = (gameContent.lockRoom2Password / 100) % 10;
        locker.wheel2Solution = (gameContent.lockRoom2Password / 10) % 10;
        locker.wheel3Solution = gameContent.lockRoom2Password % 10;
        f_passwordRoom2.First().GetComponentInChildren<TextMeshProUGUI>().text = (gameContent.lockRoom2Password % 1000).ToString();
        #endregion

        #region Room 3
        int nbQuestionRoom3 = f_queriesR3.Count;
        QuerySolution qs;
        for (int i = 0; i < nbQuestionRoom3; i++)
        {
            qs = f_queriesR3.getAt(i).GetComponent<QuerySolution>();
            qs.orSolutions = new List<string>();
            qs.orSolutions.Add(StringToAnswer(gameContent.puzzleAnswer));
            qs.orSolutions.Add(StringToAnswer(gameContent.enigma12Answer));
            qs.orSolutions.Add(StringToAnswer(gameContent.lampAnswer));
            qs.orSolutions.Add(StringToAnswer(gameContent.whiteBoardAnswer));
        }

        //Puzzles
        Sprite puzzlePicture = defaultGameContent.noPictureFound;
        if (gameContent.puzzle && File.Exists(gameContent.puzzlePicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(gameContent.puzzlePicturePath);
            if (tmpTex.LoadImage(tmpFileData))
            {
                puzzlePicture = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
            }
        }
        Rect rect;
        if (puzzlePicture.texture.width > puzzlePicture.texture.height)
            rect = new Rect(0, 0, 935, puzzlePicture.texture.height * 935 / puzzlePicture.texture.width);
        else
            rect = new Rect(0, 0, puzzlePicture.texture.width * 935 / puzzlePicture.texture.height, 935);

        int nbPuzzleUI = f_puzzleUI.Count;
        RectTransform rt = f_puzzleUI.getAt(0).GetComponent<RectTransform>();
        Vector3 newPuzzleScale = new Vector3(rect.width * rt.localScale.x / 935, rect.width * rt.localScale.y / 935, rt.localScale.z);
        for (int i = 0; i < nbPuzzleUI; i++)
        {
            rt = f_puzzleUI.getAt(i).GetComponent<RectTransform>();
            rt.localScale = newPuzzleScale;
            rt.GetChild(0).gameObject.GetComponent<Image>().sprite = puzzlePicture;
        }

        //Lamp
        int nbLampPictures = f_lampPictures.Count;
        nbLampPictures = nbLampPictures > gameContent.lampPicturesPath.Length ? gameContent.lampPicturesPath.Length : nbLampPictures;
        for (int i = 0; i < nbLampPictures; i++)
        {
            mySprite = defaultGameContent.noPictureFound;
            if (File.Exists(gameContent.lampPicturesPath[i]))
            {
                tmpTex = new Texture2D(1, 1);
                tmpFileData = File.ReadAllBytes(gameContent.lampPicturesPath[i]);
                if (tmpTex.LoadImage(tmpFileData))
                {
                    mySprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
                }
            }
            f_lampPictures.getAt(i).GetComponent<Image>().sprite = mySprite;
        }

        //White Board
        convertedBoardText = new string[2];
        int nbBoardTexts = Mathf.Min(gameContent.whiteBoardWords.Length, f_boardUnremovable.Count, f_boardRemovable.Count);
        for (int i = 0; i < nbBoardTexts; i++)
        {
            ConvertBoardText(gameContent.whiteBoardWords[i]);
            f_boardUnremovable.getAt(i).GetComponent<TextMeshPro>().text = convertedBoardText[0];
            f_boardRemovable.getAt(i).GetComponent<TextMeshPro>().text = convertedBoardText[1];
        }

        #endregion

        #region File Loading
        if (File.Exists(gameContent.lrsConfigPath))
            GBL_Interface.lrsAddresses = JsonConvert.DeserializeObject<List<LRSAddress>>(File.ReadAllText("Data/LRSConfig.txt"));
        else
        {
            if (!File.Exists("Data/LRSConfig.txt"))
                File.WriteAllText("Data/LRSConfig.txt", defaultGameContent.lrsConfigFile.text);
            Debug.LogWarning("LRS configuration file not found. Default LRS used (Lip6).");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - LRS configuration file not found. Default LRS used (Lip6)"));
        }

        GameHints gameHints = f_gameHints.First().GetComponent<GameHints>();
        if (File.Exists(gameContent.hintsPath))
        {
            try
            {
                gameHints.dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<KeyValuePair<string, List<string>>>>>>(File.ReadAllText(gameContent.hintsPath));
            }
            catch (Exception)
            {
                Debug.LogError("Invalid content in the file containting hints.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in the file containting hints"));
            }
            if (gameHints.dictionary == null)
                gameHints.dictionary = new Dictionary<string, Dictionary<string, List<KeyValuePair<string, List<string>>>>>();
        }
        else
        {
            if(!File.Exists("Data/Hints_LearningScape.txt"))
                File.WriteAllText("Data/Hints_LearningScape.txt", defaultGameContent.hintsJsonFile.text);
            Debug.LogWarning("File containting hints not found.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting hints not found"));
        }

        if (File.Exists(gameContent.internalHintsPath))
        {
            InternalGameHints internalGameHints = f_internalGameHints.First().GetComponent<InternalGameHints>();
            try
            {
                internalGameHints.dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(File.ReadAllText(gameContent.internalHintsPath));
                if (internalGameHints.dictionary == null || internalGameHints.dictionary.Count == 0)
                {
                    internalGameHints.dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(defaultGameContent.internalHintsJsonFile.text);
                    Debug.LogWarning("File containting internal hints empty. Default used.");
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting internal hints empty. Default used"));
                }
            }
            catch (Exception)
            {
                if(!File.Exists("Data/InternalHints_LearningScape.txt"))
                    File.WriteAllText("Data/InternalHints_LearningScape.txt", defaultGameContent.internalHintsJsonFile.text);
                internalGameHints.dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(defaultGameContent.internalHintsJsonFile.text);
                Debug.LogError("Invalid content in the file containting internal hints. Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in the file containting internal hints. Default used"));
            }
        }
        else
        {
            if (!File.Exists("Data/InternalHints_LearningScape.txt"))
                File.WriteAllText("Data/InternalHints_LearningScape.txt", defaultGameContent.internalHintsJsonFile.text);
            f_internalGameHints.First().GetComponent<InternalGameHints>().dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(defaultGameContent.internalHintsJsonFile.text);
            Debug.LogWarning("File containting internal hints not found. Default used.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting internal hints not found. Default used"));
        }

        if (File.Exists(gameContent.wrongAnswerFeedbacksPath))
        {
            try
            {
                gameHints.wrongAnswerFeedbacks = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>>(File.ReadAllText(gameContent.wrongAnswerFeedbacksPath));
                if (gameHints.wrongAnswerFeedbacks == null || gameHints.wrongAnswerFeedbacks.Count == 0)
                {
                    gameHints.wrongAnswerFeedbacks = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>>(File.ReadAllText(defaultGameContent.wrongAnswerFeedbacks.text));
                    Debug.LogWarning("File containting feedbacks for wrong answers empty. Default used.");
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting feedbacks for wrong answers empty. Default used"));
                }
            }
            catch (Exception)
            {
                if(!File.Exists("Data/WrongAnswerFeedbacks.txt"))
                    File.WriteAllText("Data/WrongAnswerFeedbacks.txt", defaultGameContent.wrongAnswerFeedbacks.text);
                gameHints.wrongAnswerFeedbacks = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>>(File.ReadAllText(defaultGameContent.wrongAnswerFeedbacks.text));
                Debug.LogError("Invalid content in the file containting feedbacks for wrong answers. Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in the file containting feedbacks for wrong answers. Default used"));
            }
        }
        else
        {
            if (!File.Exists("Data/WrongAnswerFeedbacks.txt"))
                File.WriteAllText("Data/WrongAnswerFeedbacks.txt", defaultGameContent.wrongAnswerFeedbacks.text);
            gameHints.wrongAnswerFeedbacks = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, KeyValuePair<string, List<string>>>>>(File.ReadAllText(defaultGameContent.wrongAnswerFeedbacks.text));
            Debug.LogWarning("File containting feedbacks for wrong answers not found. Default used.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting feedbacks for wrong answers not found. Default used"));
        }

        if (File.Exists(gameContent.enigmasWeightPath))
        {
            try
            {
                enigmasWeight = JsonConvert.DeserializeObject<Dictionary<string, float>>(File.ReadAllText(gameContent.enigmasWeightPath));
                if (enigmasWeight == null)
                {
                    enigmasWeight = JsonConvert.DeserializeObject<Dictionary<string, float>>(defaultGameContent.enigmasWeight.text);
                    Debug.LogWarning("File containting enigmas weights empty. Default used.");
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting enigmas weights empty. Default used"));
                }
            }
            catch (Exception)
            {
                if (!File.Exists("Data/EnigmasWeight.txt"))
                    File.WriteAllText("Data/EnigmasWeight.txt", defaultGameContent.internalHintsJsonFile.text);
                enigmasWeight = JsonConvert.DeserializeObject<Dictionary<string, float>>(defaultGameContent.enigmasWeight.text);
                Debug.LogError("Invalid content in the file containting enigmas weights. Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in the file containting enigmas weights. Default used"));
            }
        }
        else
        {
            if (!File.Exists("Data/EnigmasWeight.txt"))
                File.WriteAllText("Data/EnigmasWeight.txt", defaultGameContent.internalHintsJsonFile.text);
            enigmasWeight = JsonConvert.DeserializeObject<Dictionary<string, float>>(defaultGameContent.enigmasWeight.text);
            Debug.LogWarning("File containting enigmas weights not found. Default used.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting enigmas weights not found. Default used"));
        }

        if (File.Exists(gameContent.labelWeightsPath))
        {
            LabelWeights labelWeights = f_labelWeights.First().GetComponent<LabelWeights>();
            try
            {
                labelWeights.weights = JsonConvert.DeserializeObject<Dictionary<string, float>>(File.ReadAllText(gameContent.labelWeightsPath));
                if (labelWeights.weights == null || labelWeights.weights.Count == 0)
                {
                    labelWeights.weights = JsonConvert.DeserializeObject<Dictionary<string, float>>(defaultGameContent.labelWeights.text);
                    Debug.LogWarning("File containting label weights empty. Default used.");
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting label weights empty. Default used"));
                }
            }
            catch (Exception)
            {
                if (!File.Exists("Data/LabelWeights.txt"))
                    File.WriteAllText("Data/LabelWeights.txt", defaultGameContent.labelWeights.text);
                labelWeights.weights = JsonConvert.DeserializeObject<Dictionary<string, float>>(defaultGameContent.labelWeights.text);
                Debug.LogError("Invalid content in the file containting label weights. Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in the file containting label weights. Default used"));
            }
        }
        else
        {
            if (!File.Exists("Data/LabelWeights.txt"))
                File.WriteAllText("Data/LabelWeights.txt", defaultGameContent.labelWeights.text);
            f_labelWeights.First().GetComponent<LabelWeights>().weights = JsonConvert.DeserializeObject<Dictionary<string, float>>(defaultGameContent.labelWeights.text);
            Debug.LogWarning("File containting label weights not found. Default used.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - File containting label weights not found. Default used"));
        }

        if (File.Exists(gameContent.helpSystemConfigPath))
        {
            try
            {
                HelpSystem.config = JsonConvert.DeserializeObject<HelpSystemConfig>(File.ReadAllText(gameContent.helpSystemConfigPath));
                if (HelpSystem.config == null)
                {
                    HelpSystem.config = JsonConvert.DeserializeObject<HelpSystemConfig>(defaultGameContent.helpSystemConfig.text);
                    Debug.LogWarning("HelpSystem config file empty. Default used.");
                    File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - HelpSystem config file empty empty. Default used"));
                }
            }
            catch (Exception)
            {
                if (!File.Exists("Data/HelpSystemConfig.txt"))
                    File.WriteAllText("Data/HelpSystemConfig.txt", defaultGameContent.helpSystemConfig.text);
                HelpSystem.config = JsonConvert.DeserializeObject<HelpSystemConfig>(defaultGameContent.helpSystemConfig.text);
                Debug.LogError("Invalid content in the HelpSystem config file. Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in the HelpSystem config file. Default used"));
            }
        }
        else
        {
            if (!File.Exists("Data/HelpSystemConfig.txt"))
                File.WriteAllText("Data/HelpSystemConfig.txt", defaultGameContent.helpSystemConfig.text);
            HelpSystem.config = JsonConvert.DeserializeObject<HelpSystemConfig>(defaultGameContent.helpSystemConfig.text);
            Debug.LogWarning("HelpSystem config file not found. Default used.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - HelpSystem config file not found. Default used"));
        }
        #endregion

        Debug.Log("Data loaded");
        File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - Data loaded"));
    }

    public static string StringToAnswer(string answer)
    {
        // format answer, remove accents and upper case
        var normalizedString = answer.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToUpper();
    }

    /// <summary>
    /// Convert "##" codes to removable/unremovable texts
    /// </summary>
    /// <param name="text"></param>
    private void ConvertBoardText(string text)
    {
        if(convertedBoardText == null)
            convertedBoardText = new string[2];

        convertedBoardText[0] = "<alpha=#00>";
        convertedBoardText[1] = "";

        int l = text.Length;
        bool hash = false;
        bool removable = true;
        for(int i = 0; i < l; i++)
        {
            if(text[i] == '#')
            {
                if (hash)
                {
                    removable = !removable;
                    if (removable)
                    {
                        convertedBoardText[0] = string.Concat(convertedBoardText[0], "<alpha=#00>");
                        convertedBoardText[1] = string.Concat(convertedBoardText[1], "<alpha=#ff>");
                    }
                    else
                    {
                        convertedBoardText[0] = string.Concat(convertedBoardText[0], "<alpha=#ff>");
                        convertedBoardText[1] = string.Concat(convertedBoardText[1], "<alpha=#00>");
                    }
                    hash = false;
                }
                else
                    hash = true;
            }
            else
            {
                if (hash)
                {
                    convertedBoardText[0] = string.Concat(convertedBoardText[0], '#');
                    convertedBoardText[1] = string.Concat(convertedBoardText[1], '#');
                }
                convertedBoardText[0] = string.Concat(convertedBoardText[0], text[i]);
                convertedBoardText[1] = string.Concat(convertedBoardText[1], text[i]);
                hash = false;
            }
        }
        if (hash)
        {
            convertedBoardText[0] = string.Concat(convertedBoardText[0], '#');
            convertedBoardText[1] = string.Concat(convertedBoardText[1], '#');
        }
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame)
    {   
        
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

    }
}