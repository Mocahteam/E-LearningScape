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
using FYFY_plugins.PointerManager;
using System.Text.RegularExpressions;

public class LoadGameContent : FSystem {
    
    private Family f_defaultGameContent = FamilyManager.getFamily(new AllOfComponents(typeof(DefaultGameContent)));

    private Family f_logos = FamilyManager.getFamily(new AllOfComponents(typeof(ImgBank)));

    private Family f_storyText = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));

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
    private Family f_dreamFragmentsContentContainer = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)), new AnyOfTags("DreamFragmentContentContainer"));
    private Family f_dreamFragmentsContents = FamilyManager.getFamily(new AnyOfTags("DreamFragmentContent"));

    private Family f_gears = FamilyManager.getFamily(new AnyOfTags("Gears"));
    private Family f_gearComponent = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));

    private Family f_login = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(PointerSensitive)));

    private Family f_bagImage = FamilyManager.getFamily(new AllOfComponents(typeof(BagImage)));

    private Family f_scrollUI = FamilyManager.getFamily(new AnyOfTags("ScrollUI"));

    private Family f_mirrorImage = FamilyManager.getFamily(new AnyOfTags("MirrorPlankImage"), new AllOfComponents(typeof(Image)));

    private Family f_passwordRoom2 = FamilyManager.getFamily(new AnyOfTags("PasswordRoom2"));
    private Family f_lockRoom2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"), new AllOfComponents(typeof(Locker)));

    private Family f_puzzleUI = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(RectTransform)));
    private Family f_puzzles = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new NoneOfComponents(typeof(DreamFragment)));
    private Family f_puzzlesFragment = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new AllOfComponents(typeof(DreamFragment)));

    private Family f_lampPictures = FamilyManager.getFamily(new AllOfComponents(typeof(Lamp_Symbol)));

    private Family f_boardUnremovable = FamilyManager.getFamily(new AnyOfTags("BoardUnremovableWords"));
    private Family f_boardRemovable = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));

    private Family f_gameHints = FamilyManager.getFamily(new AllOfComponents(typeof(GameHints)));
    private Family f_internalGameHints = FamilyManager.getFamily(new AllOfComponents(typeof(InternalGameHints)));
    private Family f_labelWeights = FamilyManager.getFamily(new AllOfComponents(typeof(LabelWeights)));

    private Family f_inventoryElements = FamilyManager.getFamily(new AllOfComponents(typeof(Collected)));

    private Family f_uiTexts = FamilyManager.getFamily(new AllOfComponents(typeof(UIText)), new AnyOfComponents(typeof(TextMeshPro), typeof(TextMeshProUGUI)));

    private Family f_extraGeometries = FamilyManager.getFamily(new AllOfComponents(typeof(RemoveIfVeryVeryLow)));

    public static GameContent gameContent;
    private DefaultGameContent defaultGameContent;

    public static Dictionary<string, float> enigmasWeight;

    public TMP_FontAsset AccessibleFont;
    public TMP_FontAsset AccessibleFontUI;
    public TMP_FontAsset DefaultFont;
    public TMP_FontAsset DefaultFontUI;

    public static LoadGameContent instance;

    private bool loadContent = true;

    private Texture2D tmpTex;
    private byte[] tmpFileData;
    private string[] convertedBoardText; //0- unremovable, 1- removable
    private GameObject tmpGO;
    private UIText tmpUIText;
    private List<string> tmpStringList;
    private RectTransform tmpRectTransform;

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
                File.WriteAllText("Data/FragmentsPath.txt", defaultGameContent.dreamFragmentDocuments.text);
                File.WriteAllText("Data/HelpSystemConfig.txt", defaultGameContent.helpSystemConfig.text);

                gameContent = new GameContent();
                gameContent = JsonUtility.FromJson<GameContent>(defaultGameContent.jsonFile.text);

                int l = defaultGameContent.dreamFragmentPictures.Length;
                Directory.CreateDirectory("Data/Fragments");
                for (int i = 0; i < l; i++)
                {
                    File.WriteAllBytes(string.Concat("Data/Fragments/", defaultGameContent.dreamFragmentPictures[i].name, ".png"), defaultGameContent.dreamFragmentPictures[i].EncodeToPNG());
                }
                File.WriteAllBytes(string.Concat("Data/", defaultGameContent.mastermindPicture.name, ".png"), defaultGameContent.mastermindPicture.EncodeToPNG());
                l = defaultGameContent.glassesPictures.Length;
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

            instance = this;
        }
    }

    private void loadIARQuestion(GameObject question, string questionTexte, string answerFeedback, string answerFeedbackDesc, string placeHolder, List<string> andSolutions)
    {
        foreach (TextMeshProUGUI tmp in question.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp.gameObject.name == "Question")
                tmp.text = questionTexte;
            else if (tmp.gameObject.name == "Answer")
                tmp.text = answerFeedback;
            else if (tmp.gameObject.name == "Description")
                tmp.text = answerFeedbackDesc;
        }
        question.GetComponentInChildren<InputField>().transform.GetChild(0).GetComponent<TMP_Text>().text = placeHolder;
        question.GetComponent<QuerySolution>().andSolutions = new List<string>();
        foreach (string s in andSolutions)
            tmpGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
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

        if (gameContent.removeExtraGeometries)
            foreach (GameObject go in f_extraGeometries)
                GameObjectManager.setGameObjectState(go, false);

        // Load additional Logos
        if (gameContent.additionalLogosPath.Length > 0)
        {
            List<Sprite> logos = new List<Sprite>(f_logos.First().GetComponent<ImgBank>().bank);
            foreach (string path in gameContent.additionalLogosPath)
            {
                if (File.Exists(path))
                {
                    tmpTex = new Texture2D(1, 1);
                    tmpFileData = File.ReadAllBytes(path);
                    if (tmpTex.LoadImage(tmpFileData))
                        logos.Add(Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero));
                }
            }
            // Update bank of logos
            f_logos.First().GetComponent<ImgBank>().bank = logos.ToArray();
        }
        Debug.Log("Additional Logo loaded");

        // Set IAR tabs and HUD depending on gameContent.virtualDreamFragment value
        Transform tabParent = f_tabs.First().transform.parent;
        int tabCount = tabParent.childCount - 1; // -1 because of the under line among the children
        if (gameContent.virtualDreamFragment)
        {
            // IAR tabs
            tmpRectTransform = tabParent.GetChild(0).GetComponent<RectTransform>();
            tmpRectTransform.anchoredPosition = new Vector2(54.66f, -20);
            tmpRectTransform.sizeDelta = new Vector2(99, 40);
            // if dream fragment are set to virtual, do the same for the puzzles
            gameContent.virtualPuzzle = true;

            for (int i = 1; i < tabCount; i++)
            {
                tmpRectTransform = tabParent.GetChild(i).GetComponent<RectTransform>();
                tmpRectTransform.anchoredPosition = new Vector2(tabParent.GetChild(i - 1).GetComponent<RectTransform>().anchoredPosition.x + 109.32f, -20);
                tmpRectTransform.sizeDelta = new Vector2(99, 40);
            }
        }
        else
        {
            // IAR tabs
            tmpRectTransform = tabParent.GetChild(0).GetComponent<RectTransform>();
            tmpRectTransform.anchoredPosition = new Vector2(63.765f, -20);
            tmpRectTransform.sizeDelta = new Vector2(127.53f, 40);
            tmpRectTransform.anchoredPosition = new Vector2(63.765f, -20);
            GameObjectManager.setGameObjectState(tmpRectTransform.gameObject, false);

            for (int i = 2; i < tabCount; i++)
            {
                tmpRectTransform = tabParent.GetChild(i).GetComponent<RectTransform>();
                tmpRectTransform.anchoredPosition = new Vector2(tabParent.GetChild(i - 1).GetComponent<RectTransform>().anchoredPosition.x + 127.53f, -20);
                tmpRectTransform.sizeDelta = new Vector2(127.53f, 40);
            }
        }
        Debug.Log(string.Concat("Virtual dream fragments: ", gameContent.virtualDreamFragment));

        #region Story
        StoryText st = f_storyText.First().GetComponent<StoryText>();
        st.intro = gameContent.storyTextIntro;
        st.transition = gameContent.storyTextransition;
        st.end = gameContent.storyTextEnd;
        if (gameContent.additionalCredit.Length > 0)
        {
            List<string> newCredits = new List<string>(st.credit);
            newCredits.AddRange(gameContent.additionalCredit);
            st.credit = newCredits.ToArray();
        }
        Debug.Log("Story loaded");
        #endregion

        #region InventoryTexts
        Dictionary<string, List<string>> inventoryTexts = new Dictionary<string, List<string>>()
        {
            {"ScrollIntro", gameContent.inventoryScrollIntro},
            {"KeyBallBox", gameContent.inventoryKeyBallBox },
            {"Wire", gameContent.inventoryWire },
            {"KeySatchel", gameContent.inventoryKeySatchel },
            {"Scrolls", gameContent.inventoryScrolls },
            {"Glasses1", gameContent.inventoryGlasses1 },
            {"Glasses2", gameContent.inventoryGlasses2 },
            {"Mirror", gameContent.inventoryMirror },
            {"Lamp", gameContent.inventoryLamp },
            {"Puzzle", gameContent.inventoryPuzzle }
        };
        foreach (GameObject inventoryGo in f_inventoryElements)
        {
            if (inventoryTexts.ContainsKey(inventoryGo.name)){
                Collected coll = inventoryGo.GetComponent<Collected>();
                coll.itemName = inventoryTexts[inventoryGo.name][0];
                coll.description = inventoryTexts[inventoryGo.name][1];
                coll.info = inventoryTexts[inventoryGo.name][2];
            }
            else
            {
                Debug.LogWarning("No content found in config file for " + inventoryGo.name + " GameObject");
            }
        }
        Debug.Log("Inventory texts loaded");
        #endregion

        #region UI Texts
        foreach(GameObject go in f_uiTexts)
        {
            tmpUIText = go.GetComponent<UIText>();
            //write the content of the variable of gameContent corresponding to the uiText in its text mesh pro component
            if (tmpUIText.tmpUI)
                go.GetComponent<TextMeshProUGUI>().text = (string) typeof(GameContent).GetField(tmpUIText.gameContentVariable).GetValue(gameContent);
            else
                go.GetComponent<TextMeshPro>().text = (string)typeof(GameContent).GetField(tmpUIText.gameContentVariable).GetValue(gameContent);
        }
        #endregion

        #region Room 1
        int nbQueries = f_queriesR1.Count;
        for (int i = 0; i < nbQueries; i++)
        {
            tmpGO = f_queriesR1.getAt(i);
            switch (tmpGO.name)
            {
                case "Q1":
                    loadIARQuestion(tmpGO, gameContent.ballBoxQuestion, gameContent.ballBoxAnswerFeedback, gameContent.ballBoxAnswerFeedbackDesc, gameContent.ballBoxPlaceHolder, gameContent.ballBoxAnswer);
                    break;

                case "Q2":
                    loadIARQuestion(tmpGO, gameContent.plankAndWireQuestionIAR, gameContent.plankAndWireAnswerFeedback, gameContent.plankAndWireAnswerFeedbackDesc, gameContent.plankAndWirePlaceHolder, gameContent.plankAndWireCorrectNumbers);
                    break;

                case "Q3":
                    loadIARQuestion(tmpGO, gameContent.crouchQuestion, gameContent.crouchAnswerFeedback, gameContent.crouchAnswerFeedbackDesc, gameContent.crouchPlaceHolder, gameContent.crouchAnswer);
                    break;

                default:
                    break;
            }
        }
        Debug.Log("Room 1 queries loaded");

        if (File.Exists(gameContent.mastermindBackgroundPicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(gameContent.mastermindBackgroundPicturePath);
            if (tmpTex.LoadImage(tmpFileData))
                f_login.First().GetComponent<Image>().sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
        }
        // init question text and position
        TextMeshProUGUI textMP = f_login.First().transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        textMP.text = gameContent.mastermindQuestion;
        textMP.transform.localPosition = new Vector3(textMP.transform.localPosition.x, gameContent.mastermindQuestionYPos, textMP.transform.localPosition.z);
        LoginManager.passwordSolution = gameContent.mastermindAnswer;
        Debug.Log("Master mind picture loaded");


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
        Debug.Log("Ball box loaded");

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
                f_plankNumbers.getAt(i).GetComponent<TextMeshPro>().text = gameContent.plankAndWireCorrectNumbers[countCorrectNb];
                countCorrectNb++;
            }
            else
            {
                f_plankNumbers.getAt(i).GetComponent<TextMeshPro>().text = gameContent.plankAndWireOtherNumbers[countWrongNb];
                countWrongNb++;
            }
        }
        Debug.Log("Plank and wire loaded");

        //Green Dream Fragments
        int nbDreamFragments = f_dreamFragments.Count;
        DreamFragment tmpDF;
        int nbGreenFragments = 0;
        for (int i = 0; i < nbDreamFragments; i++)
        {
            tmpDF = f_dreamFragments.getAt(i).GetComponent<DreamFragment>();
            if (tmpDF.type == 1)
            {
                tmpDF.itemName = gameContent.crouchWords[nbGreenFragments];
                nbGreenFragments++;
                if (nbGreenFragments > 5)
                    break;
            }
        }
        Debug.Log("Green dream fragments loaded");

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
        Debug.Log("IAR Gears loaded");
        #endregion

        #region Room 2
        nbQueries = f_queriesR2.Count;
        for (int i = 0; i < nbQueries; i++)
        {
            tmpGO = f_queriesR2.getAt(i);
            switch (tmpGO.name)
            {
                case "Q1":
                    loadIARQuestion(tmpGO, gameContent.glassesQuestion, gameContent.glassesAnswerFeedback, gameContent.glassesAnswerFeedbackDesc, gameContent.glassesPlaceHolder, gameContent.glassesAnswer);
                    break;

                case "Q2":
                    loadIARQuestion(tmpGO, gameContent.enigma08Question, gameContent.enigma08AnswerFeedback, gameContent.enigma08AnswerFeedbackDesc, gameContent.enigma08PlaceHolder, gameContent.enigma08Answer);
                    break;

                case "Q3":
                    loadIARQuestion(tmpGO, gameContent.scrollsQuestion, gameContent.scrollsAnswerFeedback, gameContent.scrollsAnswerFeedbackDesc, gameContent.scrollsPlaceHolder, gameContent.scrollsAnswer);
                    break;

                case "Q4":
                    loadIARQuestion(tmpGO, gameContent.mirrorQuestion, gameContent.mirrorAnswerFeedback, gameContent.mirrorAnswerFeedbackDesc, gameContent.mirrorPlaceHolder, gameContent.mirrorAnswer);
                    break;

                case "Q5":
                    loadIARQuestion(tmpGO, gameContent.enigma11Question, gameContent.enigma11AnswerFeedback, gameContent.enigma11AnswerFeedbackDesc, gameContent.enigma11PlaceHolder, gameContent.enigma11Answer);
                    break;

                case "Q6":
                    loadIARQuestion(tmpGO, gameContent.enigma12Question, gameContent.enigma12AnswerFeedback, gameContent.enigma12AnswerFeedbackDesc, gameContent.enigma12PlaceHolder, gameContent.enigma12Answer);
                    break;

                default:
                    break;
            }
        }
        Debug.Log("Room 2 queries loaded");

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
                    mySprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
            }
            switch (i)
            {
                case 0:
                    bi.image0 = mySprite;
                    break;

                case 1:
                    bi.image1 = mySprite;
                    break;

                case 2:
                    bi.image2 = mySprite;
                    break;

                default:
                    bi.image3 = mySprite;
                    break;
            }
        }
        bi.gameObject.GetComponent<Image>().sprite = bi.image0;
        Debug.Log("Glasses loaded");

        //Scrolls
        int nbScroll = gameContent.scrollsWords.Length < f_scrollUI.Count ? gameContent.scrollsWords.Length : f_scrollUI.Count;
        for (int i = 0; i < nbScroll; i++)
        {
            if (gameContent.scrollsWords[i] == string.Empty)
                GameObjectManager.setGameObjectState(f_scrollUI.getAt(i), false);
            else
                f_scrollUI.getAt(i).GetComponentInChildren<TextMeshProUGUI>().text = gameContent.scrollsWords[i];
        }
        Debug.Log("Scrolls loaded");

        //Mirror
        f_mirrorImage.First().GetComponent<Image>().sprite = defaultGameContent.noPictureFound;
        if (File.Exists(gameContent.mirrorPicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(gameContent.mirrorPicturePath);
            if (tmpTex.LoadImage(tmpFileData))
                f_mirrorImage.First().GetComponent<Image>().sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
        }
        Debug.Log("Mirror loaded");

        //Lock Room 2
        Locker locker = f_lockRoom2.First().GetComponent<Locker>();
        locker.wheel1Solution = (gameContent.lockRoom2Password / 100) % 10;
        locker.wheel2Solution = (gameContent.lockRoom2Password / 10) % 10;
        locker.wheel3Solution = gameContent.lockRoom2Password % 10;
        f_passwordRoom2.First().GetComponentInChildren<TextMeshProUGUI>().text = (gameContent.lockRoom2Password % 1000).ToString();
        Debug.Log("Locker loaded");
        #endregion

        #region Room 3
        int nbQuestionRoom3 = f_queriesR3.Count;
        QuerySolution qs;
        for (int i = 0; i < nbQuestionRoom3; i++)
        {
            qs = f_queriesR3.getAt(i).GetComponent<QuerySolution>();
            qs.orSolutions = new List<string>();
            qs.orSolutions.Add(StringToAnswer(gameContent.puzzleAnswer));
            qs.orSolutions.Add(StringToAnswer(gameContent.enigma16Answer));
            qs.orSolutions.Add(StringToAnswer(gameContent.lampAnswer));
            qs.orSolutions.Add(StringToAnswer(gameContent.whiteBoardAnswer));
        }
        Debug.Log("Room 3 queries loaded");

        //Puzzles
        foreach (GameObject go in f_puzzles)
            GameObjectManager.setGameObjectState(go, gameContent.virtualPuzzle);
        foreach (GameObject go in f_puzzlesFragment)
            GameObjectManager.setGameObjectState(go, !gameContent.virtualPuzzle);

        Sprite puzzlePicture = defaultGameContent.noPictureFound;
        if (gameContent.virtualPuzzle && File.Exists(gameContent.puzzlePicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(gameContent.puzzlePicturePath);
            if (tmpTex.LoadImage(tmpFileData))
            {
                puzzlePicture = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
            }
        }
        int nbPuzzleUI = f_puzzleUI.Count;
        for (int i = 0; i < nbPuzzleUI; i++)
        {
            RectTransform rt = f_puzzleUI.getAt(i).GetComponent<RectTransform>();
            rt.GetChild(0).gameObject.GetComponent<Image>().sprite = puzzlePicture;
        }
        Debug.Log("Puzzle loaded");

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
        Debug.Log("Lamp loaded");

        //White Board
        convertedBoardText = new string[2];
        int nbBoardTexts = Mathf.Min(gameContent.whiteBoardWords.Length, f_boardUnremovable.Count, f_boardRemovable.Count);
        for (int i = 0; i < nbBoardTexts; i++)
        {
            ConvertBoardText(gameContent.whiteBoardWords[i]);
            f_boardUnremovable.getAt(i).GetComponent<TextMeshPro>().text = convertedBoardText[0];
            f_boardRemovable.getAt(i).GetComponent<TextMeshPro>().text = convertedBoardText[1];
        }
        Debug.Log("White board loaded");

        #endregion

        #region File Loading
        // Load LRS config file
        LoadJsonFile(gameContent.lrsConfigPath, defaultGameContent.lrsConfigFile, out GBL_Interface.lrsAddresses);
        if (GBL_Interface.lrsAddresses == null)
            GBL_Interface.lrsAddresses = new List<LRSAddress>();
        if (gameContent.traceToLRS)
            SendStatements.instance.initGBLXAPI();
        Debug.Log("LRS config file loaded ");

        // Load Hints config files
        GameHints gameHints = f_gameHints.First().GetComponent<GameHints>();
        LoadJsonFile(gameContent.hintsPath, defaultGameContent.hintsJsonFile, out gameHints.dictionary);
        if (gameHints.dictionary == null)
            gameHints.dictionary = new Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>> ();
        Debug.Log("Hints loaded");
        // Load Wrong answer feedback
        LoadJsonFile(gameContent.wrongAnswerFeedbacksPath, defaultGameContent.wrongAnswerFeedbacks, out gameHints.wrongAnswerFeedbacks);
        if (gameHints.wrongAnswerFeedbacks == null)
            gameHints.wrongAnswerFeedbacks = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>();
        Debug.Log("Wrong answer feedback loaded");

        // Load InternalHints config files
        InternalGameHints internalGameHints = f_internalGameHints.First().GetComponent<InternalGameHints>();
        LoadJsonFile(gameContent.internalHintsPath, defaultGameContent.internalHintsJsonFile, out internalGameHints.dictionary);
        if (internalGameHints.dictionary == null)
            internalGameHints.dictionary = new Dictionary<string, Dictionary<string, List<string>>>();
        Debug.Log("Internal hints loaded");

        // Load EnigmasWeight config files
        LoadJsonFile(gameContent.enigmasWeightPath, defaultGameContent.enigmasWeight, out enigmasWeight);
        if (enigmasWeight == null)
            enigmasWeight = new Dictionary<string, float>();
        Debug.Log("Enigmas weight loaded");

        // Load LabelWeights config files
        LabelWeights labelWeights = f_labelWeights.First().GetComponent<LabelWeights>();
        LoadJsonFile(gameContent.labelWeightsPath, defaultGameContent.labelWeights, out labelWeights.weights);
        if (labelWeights.weights == null)
            labelWeights.weights = new Dictionary<string, float>();
        Debug.Log("Labels weight loaded");

        // Load HelpSystem config files
        LoadJsonFile(gameContent.helpSystemConfigPath, defaultGameContent.helpSystemConfig, out HelpSystem.config);
        if (HelpSystem.config == null)
            HelpSystem.config = new HelpSystemConfig();
        Debug.Log("HelpSystem config file loaded");

        //Load dream fragment links config files
        Dictionary<string, string> dreamFragmentsLinks = null;
        LoadJsonFile(gameContent.dreamFragmentLinksPath, defaultGameContent.dreamFragmentlinks, out dreamFragmentsLinks);
        if (dreamFragmentsLinks == null)
            dreamFragmentsLinks = new Dictionary<string, string>();
        // Affects urlLinks to dream fragments
        foreach (GameObject dream_go in f_dreamFragments)
            dreamFragmentsLinks.TryGetValue(dream_go.name, out dream_go.GetComponent<DreamFragment>().urlLink);
        Debug.Log("Dream fragments links loaded");

        // Load dream fragment png config files
        FragmentFiles fragmentFilesPaths = null;
        LoadJsonFile(gameContent.dreamFragmentDocumentsPathFile, defaultGameContent.dreamFragmentDocuments, out fragmentFilesPaths);
        // Affects dream fragment pictures to documents gameobject in IAR
        if(f_dreamFragmentsContentContainer.Count > 0 && f_dreamFragmentsContentContainer.First().GetComponent<PrefabContainer>().prefab)
        {
            GameObject iarDocumentPrefab = f_dreamFragmentsContentContainer.First().GetComponent<PrefabContainer>().prefab;
            string variableNameBeginning = "fragmentPath";
            string variableName = "";
            int l, posID;
            float gap = 30;
            Image[] tmpImages;
            foreach(GameObject go in f_dreamFragmentsContents)
            {
                //get the name of the variable corresponding to the content
                variableName = string.Concat(variableNameBeginning, go.name.Substring(go.name.Length - 2, 2));
                //retrieve the list of paths with the variable name
                tmpStringList = (List<string>)typeof(FragmentFiles).GetField(variableName).GetValue(fragmentFilesPaths);
                if(tmpStringList != null)
                {
                    l = tmpStringList.Count;
                    for(int i = 0; i < l; i++)
                    {
                        //load each pictures of the list
                        if (File.Exists(tmpStringList[i]))
                        {
                            tmpTex = new Texture2D(1, 1);
                            tmpFileData = File.ReadAllBytes(tmpStringList[i]);
                            if (tmpTex.LoadImage(tmpFileData))
                            {
                                //if the picture is successfully loaded, create an instance of IARDocument and put the picture in it
                                tmpGO = GameObject.Instantiate(iarDocumentPrefab);
                                tmpGO.transform.SetParent(go.transform);
                                tmpRectTransform = tmpGO.GetComponent<RectTransform>();
                                tmpRectTransform.localScale = Vector3.one;
                                //if there are several document for on dream fragment, give them different position to make them visible
                                //(here we put a gap of 30 between each, alternating left and right)
                                posID = l - i - 1;
                                tmpRectTransform.anchoredPosition = new Vector2((l % 2 == 0 ? gap/2 : 0) + gap * (posID / 2 + posID % 2) * (posID % 2 == 0 ? 1 : -1), 0);
                                float width, height;
                                if (tmpTex.width > tmpTex.height)
                                {
                                    width = 300;
                                    height = width * tmpTex.height / tmpTex.width;
                                }
                                else
                                {
                                    height = 300;
                                    width = height * tmpTex.width / tmpTex.height;
                                }
                                tmpRectTransform.sizeDelta = new Vector2(width, height);
                                GameObjectManager.bind(tmpGO);
                                tmpImages = tmpGO.GetComponentsInChildren<Image>();
                                if (tmpImages.Length == 0)
                                {
                                    GameObjectManager.addComponent<Image>(tmpGO);
                                    tmpImages = tmpGO.GetComponentsInChildren<Image>();
                                }
                                foreach (Image img in tmpImages)
                                    img.sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
                            }
                        }
                    }
                }
            }
            Debug.Log("IAR dream fragments' pictures loaded");
        }
        else
        {
            Debug.LogError("Missing IARDocument prefab, pictures can't be loaded.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Missing IARDocument prefab, pictures can't be loaded."));
        }
        #endregion

        // Load fonts
        AccessibleFont = defaultGameContent.accessibleFontTMPro;
        AccessibleFontUI = defaultGameContent.accessibleFontTMProUI;
        DefaultFont = defaultGameContent.defaultFontTMPro;
        DefaultFontUI = defaultGameContent.defaultFontTMProUI;
        Debug.Log("Fonts loaded");

        Debug.Log("Data loaded");
        File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - Data loaded"));
    }

    private void LoadJsonFile<T>(string jsonPath, TextAsset defaultContent, out T target)
    {
        if (File.Exists(jsonPath))
        {
            try
            {
                target = JsonConvert.DeserializeObject <T>(File.ReadAllText(jsonPath));
            }
            catch (Exception)
            {
                target = JsonConvert.DeserializeObject<T>(defaultContent.text);
                Debug.LogError("Invalid content in file: " + jsonPath + ". Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in file: " + jsonPath + ". Default used"));
            }
        }
        else
        {
            // write default content
            File.WriteAllText(jsonPath, defaultContent.text);
            // load default content
            target = JsonConvert.DeserializeObject<T>(defaultContent.text);
            Debug.LogWarning(jsonPath+ " not found. Default used.");
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - "+ jsonPath + " not found. Default used"));
        }
    }

    public static string StringToAnswer(string answer)
    {
        // format answer, remove accents, upper case and multiple spaces
        string normalizedString = answer.Normalize(NormalizationForm.FormD);
        // remove multiple spaces
        normalizedString = new Regex("[ ]{2,}", RegexOptions.None).Replace(normalizedString, " ");
        // remove first space
        if (normalizedString.StartsWith(" "))
            normalizedString = normalizedString.Remove(0, 1);
        // remove last space
        if (normalizedString.EndsWith(" "))
            normalizedString = normalizedString.Remove(normalizedString.Length-1);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
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