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
    private Family f_idTexts = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("SessionIDText"));

    private Family f_queriesR1 = FamilyManager.getFamily(new AnyOfTags("Q-R1"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR3 = FamilyManager.getFamily(new AnyOfTags("Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_balls = FamilyManager.getFamily(new AnyOfTags("Ball"));

    private Family f_wrongWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro)), new NoneOfComponents(typeof(IsSolution)));
    private Family f_correctWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro), typeof(IsSolution)));
    private Family f_plankNumbers = FamilyManager.getFamily(new AnyOfTags("PlankNumbers"));

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
    private Family f_dreamFragmentsContentContainer = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)), new AnyOfTags("DreamFragmentContentContainer"));
    private Family f_dreamFragmentsContents = FamilyManager.getFamily(new AnyOfTags("DreamFragmentContent"));

    private Family f_gearComponent = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));

    private Family f_scrollUI = FamilyManager.getFamily(new AnyOfTags("ScrollUI"));

    private Family f_puzzleUI = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(RectTransform)));
    private Family f_puzzles = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new NoneOfComponents(typeof(DreamFragment)));
    private Family f_puzzlesFragment = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new AllOfComponents(typeof(DreamFragment)));

    private Family f_lampPictures = FamilyManager.getFamily(new AllOfComponents(typeof(Lamp_Symbol)));

    private Family f_boardUnremovable = FamilyManager.getFamily(new AnyOfTags("BoardUnremovableWords"));
    private Family f_boardRemovable = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));

    private Family f_inventoryElements = FamilyManager.getFamily(new AllOfComponents(typeof(Collected)));

    private Family f_uiTexts = FamilyManager.getFamily(new AllOfComponents(typeof(UIText)), new AnyOfComponents(typeof(TextMeshPro), typeof(TextMeshProUGUI)));

    private Family f_extraGeometries = FamilyManager.getFamily(new AllOfComponents(typeof(RemoveIfVeryVeryLow)));

    public static GameContent gameContent;
    public static InternalGameContent internalGameContent;
    public static Dictionary<string, List<string>> dreamFragmentsLinks;

    public static Dictionary<string, float> enigmasWeight;

    public static string sessionID;

    public ImgBank Logos;
    public StoryText storyText;
    public GameObject iarTabs;
    public GameObject gears;
    public GameObject dreamFragmentHUD;
    public GameObject login;
    public BagImage bagImage;
    public Image mirrorPlank;
    public Locker lockerRoom2;
    public GameHints gameHints;
    public InternalGameHints internalGameHints;
    public TextMeshProUGUI passwordRoom2;
    public LabelWeights labelWeights;
    public TextMeshProUGUI creditText;
    public TMP_FontAsset AccessibleFont;
    public TMP_FontAsset AccessibleFontUI;
    public TMP_FontAsset DefaultFont;
    public TMP_FontAsset DefaultFontUI;
    public TMP_Text GameType;

    public static LoadGameContent instance;

    private System.Random random;

    private Texture2D tmpTex;
    private byte[] tmpFileData;
    private string[] convertedBoardText; //0- unremovable, 1- removable
    private GameObject tmpGO;
    private UIText tmpUIText;
    private List<string> tmpStringList;
    private RectTransform tmpRectTransform;

    private bool dataAvailable = true;

    private string dataPath = ".";

    public LoadGameContent()
    {
        instance = this;
    }

    protected override void onStart()
    {
        Application.runInBackground = true; // do not pause game if application loses focus

        random = new System.Random();

        if (Application.isEditor && GameSelected.version == "")
            GameSelected.version = "Access";

        dataPath = Application.streamingAssetsPath + "/" + GameSelected.version;

        if (File.Exists(dataPath + "/InternalData.txt"))
        {
            //Load game content from the file
            try
            {
                LoadInternalData();
            }
            catch (Exception e)
            {
                Debug.LogError(dataPath + "/InternalData.txt is not consistent, please check content.");
                Debug.LogError(e);
                dataAvailable = false;
            }
        }
        else
        {
            Debug.LogError(dataPath + "/InternalData.txt doesn't exists or access is not authorized.");
            dataAvailable = false;
        }

        if (File.Exists(dataPath + "/Data_LearningScape.txt"))
        {
            //Load game content from the file
            try
            {
                Load();
            }
            catch (Exception e)
            {
                Debug.LogError(dataPath + "/Data_LearningScape.txt is not consistent, please check content.");
                Debug.LogError(e);
                dataAvailable = false;
            }
        }
        else
        {
            Debug.LogError(dataPath + "/Data_LearningScape.txt doesn't exists or access is not authorized.");
            dataAvailable = false;
        }

        #region UI Texts
        foreach (GameObject go in f_uiTexts)
        {
            tmpUIText = go.GetComponent<UIText>();
            //write the content of the variable of gameContent corresponding to the uiText in its text mesh pro component
            try
            {
                go.GetComponent<TMP_Text>().text = (string)typeof(InternalGameContent).GetField(tmpUIText.gameContentVariable).GetValue(internalGameContent);
            }
            catch (Exception)
            {
                try
                {
                    go.GetComponent<TMP_Text>().text = (string)typeof(GameContent).GetField(tmpUIText.gameContentVariable).GetValue(gameContent);
                }
                catch (Exception)
                {
                    Debug.LogError("The key \"" + tmpUIText.gameContentVariable + "\" is not defined in InternalData.txt or in Data_LearningScape.txt, please check content.");
                    dataAvailable = false;
                }
            }
        }
        // add the generated session id after the ui text has been set
        foreach (GameObject go in f_idTexts)
            go.GetComponent<TextMeshProUGUI>().text += sessionID;
        #endregion
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (Time.frameCount > 10 && !dataAvailable)
        { // Do not load scene at the first frame => Unity Crash !!! Something wrong with GPU...
            GameObjectManager.loadScene("DataError");
        }
        else if (Time.frameCount > 10)
            Pause = true;
    }

    private void loadIARQuestion(GameObject question, List<string> andSolutions)
    {
        question.GetComponent<QuerySolution>().andSolutions = new List<string>();
        foreach (string s in andSolutions)
            tmpGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
    }

    private void LoadInternalData()
    {
        //Load game content from the file
        internalGameContent = JsonUtility.FromJson<InternalGameContent>(File.ReadAllText(dataPath + "/InternalData.txt"));

        ActionsManager.instance.Pause = !internalGameContent.trace;
        Debug.Log(string.Concat("Trace: ", internalGameContent.trace));

        // if randomHelpSystemActivation is true, set gamecontent.helpsystem with a random value
        if (internalGameContent.randomHelpSystemActivation)
            internalGameContent.helpSystem = random.Next(4) <= 2; // HelpSystem is enabled 75%
        HelpSystem.shouldPause = !internalGameContent.trace || !internalGameContent.helpSystem || !MonitoringManager.Instance.inGameAnalysis;
        Debug.Log(string.Concat("Help system: ", internalGameContent.helpSystem, "; Laalys in game analysis: ", MonitoringManager.Instance.inGameAnalysis));

        SendStatements.shouldPause = !internalGameContent.traceToLRS;
        SendStatements.instance.Pause = !internalGameContent.traceToLRS;
        MovingSystem_FPSMode.instance.traceMovementFrequency = internalGameContent.traceMovementFrequency;
        Debug.Log(string.Concat("Trace to LRS: ", internalGameContent.traceToLRS));

        if (internalGameContent.removeExtraGeometries)
            foreach (GameObject go in f_extraGeometries)
                GameObjectManager.setGameObjectState(go, false);

        #region File Loading
        // Load LRS config file
        LoadJsonFile(dataPath + "/" + internalGameContent.lrsConfigPath, out GBL_Interface.lrsAddresses);
        if (GBL_Interface.lrsAddresses == null)
            GBL_Interface.lrsAddresses = new List<DIG.GBLXAPI.GBLConfig>();
        if (internalGameContent.traceToLRS)
            SendStatements.instance.initGBLXAPI();
        Debug.Log("LRS config file loaded ");

        // Load Hints config files
        LoadJsonFile(dataPath + "/" + internalGameContent.hintsPath, out gameHints.dictionary);
        if (gameHints.dictionary == null)
            gameHints.dictionary = new Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>>();
        Debug.Log("Hints loaded");
        // Load Wrong answer feedback
        LoadJsonFile(dataPath + "/" + internalGameContent.wrongAnswerFeedbacksPath, out gameHints.wrongAnswerFeedbacks);
        if (gameHints.wrongAnswerFeedbacks == null)
            gameHints.wrongAnswerFeedbacks = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>();
        Debug.Log("Wrong answer feedback loaded");

        // Load InternalHints config files
        LoadJsonFile(dataPath + "/" + internalGameContent.internalHintsPath, out internalGameHints.dictionary);
        if (internalGameHints.dictionary == null)
            internalGameHints.dictionary = new Dictionary<string, Dictionary<string, List<string>>>();
        Debug.Log("Internal hints loaded");

        // Load EnigmasWeight config files
        LoadJsonFile(dataPath + "/" + internalGameContent.enigmasWeightPath, out enigmasWeight);
        if (enigmasWeight == null)
            enigmasWeight = new Dictionary<string, float>();
        Debug.Log("Enigmas weight loaded");

        // Load LabelWeights config files
        LoadJsonFile(dataPath + "/" + internalGameContent.labelWeightsPath, out labelWeights.weights);
        if (labelWeights.weights == null)
            labelWeights.weights = new Dictionary<string, float>();
        Debug.Log("Labels weight loaded");

        // Load HelpSystem config files
        LoadJsonFile(dataPath + "/" + internalGameContent.helpSystemConfigPath, out HelpSystem.config);
        if (HelpSystem.config == null)
            HelpSystem.config = new HelpSystemConfig();
        Debug.Log("HelpSystem config file loaded");

        //Load dream fragment links config files
        LoadJsonFile(dataPath + "/" + internalGameContent.dreamFragmentLinksPath, out dreamFragmentsLinks);
        if (dreamFragmentsLinks == null)
            dreamFragmentsLinks = new Dictionary<string, List<string>>();
        // Affects urlLinks to dream fragments
        foreach (GameObject dream_go in f_dreamFragments)
            if (dreamFragmentsLinks.ContainsKey(dream_go.name))
            {
                if (dreamFragmentsLinks[dream_go.name] != null && dreamFragmentsLinks[dream_go.name].Count > 1)
                {
                    dream_go.GetComponent<DreamFragment>().urlLink = dreamFragmentsLinks[dream_go.name][0];
                    dream_go.GetComponent<DreamFragment>().linkButtonText = dreamFragmentsLinks[dream_go.name][1];
                }
            }
        Debug.Log("Dream fragments links loaded");

        // Load dream fragment png config files
        FragmentFiles fragmentFilesPaths = null;
        LoadJsonFile(dataPath + "/" + internalGameContent.dreamFragmentDocumentsPathFile, out fragmentFilesPaths);
        // Affects dream fragment pictures to documents gameobject in IAR
        if (f_dreamFragmentsContentContainer.Count > 0 && f_dreamFragmentsContentContainer.First().GetComponent<PrefabContainer>().prefab)
        {
            GameObject iarDocumentPrefab = f_dreamFragmentsContentContainer.First().GetComponent<PrefabContainer>().prefab;
            string variableNameBeginning = "fragmentPath";
            string variableName = "";
            int l, posID;
            float gap = 30;
            Image[] tmpImages;
            foreach (GameObject go in f_dreamFragmentsContents)
            {
                //get the name of the variable corresponding to the content
                variableName = string.Concat(variableNameBeginning, go.name.Substring(go.name.Length - 2, 2));
                //retrieve the list of paths with the variable name
                tmpStringList = (List<string>)typeof(FragmentFiles).GetField(variableName).GetValue(fragmentFilesPaths);
                if (tmpStringList != null)
                {
                    l = tmpStringList.Count;
                    for (int i = 0; i < l; i++)
                    {
                        //load each pictures of the list
                        if (File.Exists(dataPath + "/" + tmpStringList[i]))
                        {
                            tmpTex = new Texture2D(1, 1);
                            tmpFileData = File.ReadAllBytes(dataPath + "/" + tmpStringList[i]);
                            if (tmpTex.LoadImage(tmpFileData))
                            {
                                //if the picture is successfully loaded, create an instance of IARDocument and put the picture in it
                                tmpGO = GameObject.Instantiate(iarDocumentPrefab);
                                tmpGO.name = Path.GetFileNameWithoutExtension(tmpStringList[i]);
                                tmpGO.transform.SetParent(go.transform);
                                tmpRectTransform = tmpGO.GetComponent<RectTransform>();
                                tmpRectTransform.localScale = Vector3.one;
                                //if there are several document for one dream fragment, give them different position to make them visible
                                //(here we put a gap of 30 between each, alternating left and right)
                                posID = l - i - 1;
                                tmpRectTransform.anchoredPosition = new Vector2((l % 2 == 0 ? gap / 2 : 0) + gap * (posID / 2 + posID % 2) * (posID % 2 == 0 ? 1 : -1), 0);
                                float width, height;
                                if (tmpTex.width > tmpTex.height)
                                {
                                    width = 400;
                                    height = width * tmpTex.height / tmpTex.width;
                                }
                                else
                                {
                                    height = 400;
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
                                {
                                    img.sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
                                    img.alphaHitTestMinimumThreshold = 0.5f;
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("IAR dream fragments' pictures loaded");
        }
        else
            Debug.LogError("Missing IARDocument prefab, pictures can't be loaded.");
        #endregion

        // Load playerPrefs
        SettingsManager.instance.LoadSettings();

        // disable auto save if save and load are disabled
        internalGameContent.autoSaveProgression = internalGameContent.saveAndLoadProgression && internalGameContent.autoSaveProgression;

        Debug.Log("Internal Data loaded");
    }

    private void Load()
    {
        //Load game content from the file
        gameContent = JsonUtility.FromJson<GameContent>(File.ReadAllText(dataPath + "/Data_LearningScape.txt"));

        GameType.text = "Version : "+gameContent.theme;

        // Load additional Logos
        if (gameContent.additionalLogosPath.Length > 0)
        {
            List<Sprite> _logos = new List<Sprite>(Logos.bank);
            foreach (string path in gameContent.additionalLogosPath)
            {
                if (File.Exists(dataPath + "/" + path))
                {
                    tmpTex = new Texture2D(1, 1);
                    tmpFileData = File.ReadAllBytes(dataPath + "/" +path);
                    if (tmpTex.LoadImage(tmpFileData))
                        _logos.Add(Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero));
                }
            }
            // Update bank of logos
            Logos.bank = _logos.ToArray();
        }
        Debug.Log("Additional Logo loaded");

        // Generate session ID (the id is added to ui only after ui texts are set)
        if(sessionID == null || sessionID == "")
        {
            sessionID = string.Concat(Environment.MachineName, "-", DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss"));

            // create a hash with sessionID
            sessionID = String.Format("{0:X}", sessionID.GetHashCode());

            SendStatements.instance.initPlayerName(sessionID);

            Debug.Log(string.Concat("Session ID generated: ", sessionID));
        }
        else
        {
            SendStatements.instance.initPlayerName(sessionID);

            Debug.Log(string.Concat("Previous session ID kept: ", sessionID));
        }

        #region Story
        storyText.intro = gameContent.storyTextIntro;
        storyText.transition = gameContent.storyTextransition;
        storyText.end = gameContent.storyTextEnd;
        storyText.endLink = gameContent.endLink;
        if (gameContent.endLink != "" && gameContent.concatIdToLink)
            storyText.endLink = string.Concat(storyText.endLink, sessionID);
        Debug.Log("Story loaded");
        #endregion

        #region Credit
        foreach (string newCredit in gameContent.additionalCredit)
            creditText.text += newCredit + "\n\n";
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

        #region Room 1
        int nbQueries = f_queriesR1.Count;
        for (int i = 0; i < nbQueries; i++)
        {
            tmpGO = f_queriesR1.getAt(i);
            switch (tmpGO.name)
            {
                case "R1-Q1":
                    loadIARQuestion(tmpGO, gameContent.ballBoxAnswer);
                    break;

                case "R1-Q2":
                    loadIARQuestion(tmpGO, gameContent.plankAndWireCorrectNumbers);
                    break;

                case "R1-Q3":
                    loadIARQuestion(tmpGO, gameContent.crouchAnswer);
                    break;

                default:
                    break;
            }
        }
        Debug.Log("Room 1 queries loaded");

        if (File.Exists(dataPath + "/" + gameContent.mastermindBackgroundPicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(dataPath + "/" + gameContent.mastermindBackgroundPicturePath);
            if (tmpTex.LoadImage(tmpFileData))
                login.GetComponent<Image>().sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
        }
        // init question text and position
        TextMeshProUGUI textMP = login.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        textMP.transform.localPosition = new Vector3(textMP.transform.localPosition.x, gameContent.mastermindQuestionYPos, textMP.transform.localPosition.z);
        LoginManager.passwordSolution = gameContent.mastermindAnswer;

        Debug.Log("Master mind picture loaded");


        //Ball Box
        int nbBalls = f_balls.Count;
        Ball b = null;
        Ball b2 = null;
        string tmpString;
        int answer;
        //initialise position and texts for all balls
        List<int> idList = new List<int>();
        if (gameContent.ballRandomPositioning)
            for (int i = 0; i < nbBalls; i++)
                idList.Add(i);
        for (int i = 0; i < nbBalls; i++)
        {
            b = f_balls.getAt(i).GetComponent<Ball>();

            if (gameContent.ballRandomPositioning)
            {
                //change randomly the position of the ball
                b.id = idList[(int)UnityEngine.Random.Range(0, idList.Count - 0.001f)];
                idList.Remove(b.id);
            }
            else
                b.id = i;

            if(b.number < gameContent.ballTexts.Length)
                b.text = gameContent.ballTexts[b.number];
        }
        //Exchange texts and numbers to set Json solution balls (3 balls required)
        //Default solution balls are 1, 2 and 3
        for (int j = 1; j <= 3 && j <= gameContent.ballBoxThreeUsefulBalls.Length ; j++)
        {
            // Look for the current ball answer
            b = null;
            for (int i = 0; i < nbBalls; i++)
            {
                Ball bTmp = f_balls.getAt(i).GetComponent<Ball>();
                if (bTmp.number == j)
                {
                    b = bTmp;
                    break;
                }
            }
            //get current answer
            answer = gameContent.ballBoxThreeUsefulBalls[j-1];
            //Look for the new solution ball
            b2 = null;
            for (int i = 0; i < nbBalls; i++)
            {
                Ball bTmp = f_balls.getAt(i).GetComponent<Ball>();
                if (bTmp.number == answer)
                {
                    b2 = bTmp;
                    break;
                }
            }
            //If the two balls were found
            if (b != null && b2 != null)
            {
                //exchange numbers, texts and id
                if (b.number == j)
                {
                    b.number = answer;
                    b2.number = j;
                }
                else
                {
                    b.number = j;
                    b2.number = answer;
                }
                tmpString = b.text;
                int tmpID = b.id;
                b.text = b2.text;
                b.id = b2.id;
                b2.text = tmpString;
                b2.id = tmpID;
            }
            else
                Debug.LogWarning(string.Concat("The answer ", j, " of BallBox enigma should be between 0 and 9 included."));
        }
        for (int i = 0; i < nbBalls; i++)
        {
            b = f_balls.getAt(i).GetComponent<Ball>();
            b.GetComponentInChildren<TextMeshPro>().text = b.number.ToString();
        }

        Debug.Log("Ball box loaded");

        //Plank and Wire
        int nbWrongWords = f_wrongWords.Count;
        int nbCorrectWords = f_correctWords.Count;
        // Set Wrong words in a random position
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
        int nbAnswerGears = 0;
        bool answerGearFound = false;
        foreach (Transform child in gears.transform)
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
            }
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
                case "R2-Q1":
                    loadIARQuestion(tmpGO, gameContent.glassesAnswer);
                    break;

                case "R2-Q2":
                    loadIARQuestion(tmpGO, gameContent.enigma08Answer);
                    break;

                case "R2-Q3":
                    loadIARQuestion(tmpGO, gameContent.scrollsAnswer);
                    break;

                case "R2-Q4":
                    loadIARQuestion(tmpGO, gameContent.mirrorAnswer);
                    break;

                case "R2-Q5":
                    loadIARQuestion(tmpGO, gameContent.enigma11Answer);
                    break;

                case "R2-Q6":
                    loadIARQuestion(tmpGO, gameContent.enigma12Answer);
                    break;

                default:
                    break;
            }
        }
        Debug.Log("Room 2 queries loaded");

        //Glasses
        Sprite mySprite;
        for (int i = 0; i < 4; i++)
        {
            mySprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            if (File.Exists(dataPath + "/" + gameContent.glassesPicturesPath[i]))
            {
                tmpTex = new Texture2D(1, 1);
                tmpFileData = File.ReadAllBytes(dataPath + "/" + gameContent.glassesPicturesPath[i]);
                if (tmpTex.LoadImage(tmpFileData))
                    mySprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
            }
            switch (i)
            {
                case 0:
                    bagImage.image0 = mySprite;
                    break;

                case 1:
                    bagImage.image1 = mySprite;
                    break;

                case 2:
                    bagImage.image2 = mySprite;
                    break;

                default:
                    bagImage.image3 = mySprite;
                    break;
            }
        }
        bagImage.gameObject.GetComponent<Image>().sprite = bagImage.image0;
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
        mirrorPlank.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        if (File.Exists(dataPath + "/" + gameContent.mirrorPicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(dataPath + "/" + gameContent.mirrorPicturePath);
            if (tmpTex.LoadImage(tmpFileData))
                mirrorPlank.sprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
        }
        Debug.Log("Mirror loaded");

        //Lock Room 2
        lockerRoom2.wheel1Solution = (gameContent.lockRoom2Password / 100) % 10;
        lockerRoom2.wheel2Solution = (gameContent.lockRoom2Password / 10) % 10;
        lockerRoom2.wheel3Solution = gameContent.lockRoom2Password % 10;
        passwordRoom2.text = (gameContent.lockRoom2Password % 1000).ToString();
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
        // if dream fragment are set to virtual, do the same for the puzzles
        foreach (GameObject go in f_puzzles)
            GameObjectManager.setGameObjectState(go, internalGameContent.virtualPuzzle);
        foreach (GameObject go in f_puzzlesFragment)
            GameObjectManager.setGameObjectState(go, !internalGameContent.virtualPuzzle);

        Sprite puzzlePicture = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        if (internalGameContent.virtualPuzzle && File.Exists(dataPath + "/" + gameContent.puzzlePicturePath))
        {
            tmpTex = new Texture2D(1, 1);
            tmpFileData = File.ReadAllBytes(dataPath + "/" + gameContent.puzzlePicturePath);
            if (tmpTex.LoadImage(tmpFileData))
            {
                puzzlePicture = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
            }
        }
        int nbPuzzleUI = f_puzzleUI.Count;
        for (int i = 0; i < nbPuzzleUI; i++)
        {
            RectTransform rt = f_puzzleUI.getAt(i).GetComponent<RectTransform>();
            rt.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
            rt.GetChild(0).gameObject.GetComponent<Image>().sprite = puzzlePicture;
        }
        Debug.Log("Puzzle loaded");

        //Lamp
        int nbLampPictures = f_lampPictures.Count;
        nbLampPictures = nbLampPictures > gameContent.lampPicturesPath.Length ? gameContent.lampPicturesPath.Length : nbLampPictures;
        for (int i = 0; i < nbLampPictures; i++)
        {
            mySprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            if (File.Exists(dataPath + "/" + gameContent.lampPicturesPath[i]))
            {
                tmpTex = new Texture2D(1, 1);
                tmpFileData = File.ReadAllBytes(dataPath + "/" + gameContent.lampPicturesPath[i]);
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

        Debug.Log("Data loaded");
    }

    private void LoadJsonFile<T>(string jsonPath, out T target)
    {
        target = JsonConvert.DeserializeObject<T>("");
        if (File.Exists(jsonPath))
        {
            try
            {
                target = JsonConvert.DeserializeObject <T>(File.ReadAllText(jsonPath));
            }
            catch (Exception e)
            {
                Debug.LogError(jsonPath+" is not consistent, please check content.");
                Debug.LogError(e);
                dataAvailable = false;
            }
        }
        else
        {
            Debug.LogError(jsonPath + " doesn't exists or access is not authorized.");
            dataAvailable = false;
        }
    }

    /// <summary>
    /// Set dream fragments as virtual or not.
    /// </summary>
    /// <param name="virtualDreamFragment">If true, dream fragments will be set to virtual</param>
    public void SetFragments(bool virtualDreamFragment)
    {
        // Set HUD depending on virtualDreamFragment value
        GameObjectManager.setGameObjectState(
            dreamFragmentHUD,
            virtualDreamFragment && IARNewDreamFragmentAvailable.instance != null && IARNewDreamFragmentAvailable.instance.firstFragmentOccurs
        );
        // Set IAR tabs and HUD depending on virtualDreamFragment value
        int tabCount = iarTabs.transform.childCount - 1; // -1 because of the under line among the children
        if (virtualDreamFragment)
        {
            // IAR tabs
            tmpRectTransform = iarTabs.transform.GetChild(0).GetComponent<RectTransform>();
            tmpRectTransform.anchoredPosition = new Vector2(54.66f, -20);
            tmpRectTransform.sizeDelta = new Vector2(99, 40);

            bool viewTab = !DebugModeSystem.instance.Pause;
            if (DreamFragmentCollecting.instance != null) // require this test because DreamFragmentCollecting is initialized after LoadGameContent inside MainLoop
                viewTab = viewTab || DreamFragmentCollecting.instance.firstFragmentFound;
            GameObjectManager.setGameObjectState(iarTabs.transform.GetChild(1).gameObject, viewTab);
            for (int i = 1; i < tabCount; i++)
            {
                tmpRectTransform = iarTabs.transform.GetChild(i).GetComponent<RectTransform>();
                tmpRectTransform.anchoredPosition = new Vector2(iarTabs.transform.GetChild(i - 1).GetComponent<RectTransform>().anchoredPosition.x + 109.32f, -20);
                tmpRectTransform.sizeDelta = new Vector2(99, 40);
            }
        }
        else
        {
            GameObjectManager.setGameObjectState(iarTabs.transform.GetChild(1).gameObject, false);

            // IAR tabs
            tmpRectTransform = iarTabs.transform.GetChild(0).GetComponent<RectTransform>();
            tmpRectTransform.anchoredPosition = new Vector2(63.765f, -20);
            tmpRectTransform.sizeDelta = new Vector2(127.53f, 40);

            tmpRectTransform = iarTabs.transform.GetChild(2).GetComponent<RectTransform>();
            tmpRectTransform.anchoredPosition = new Vector2(iarTabs.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x + 127.53f, -20);
            tmpRectTransform.sizeDelta = new Vector2(127.53f, 40);

            for (int i = 3; i < tabCount; i++)
            {
                tmpRectTransform = iarTabs.transform.GetChild(i).GetComponent<RectTransform>();
                tmpRectTransform.anchoredPosition = new Vector2(iarTabs.transform.GetChild(i - 1).GetComponent<RectTransform>().anchoredPosition.x + 127.53f, -20);
                tmpRectTransform.sizeDelta = new Vector2(127.53f, 40);
            }
        }
        IARDreamFragmentManager.virtualDreamFragment = virtualDreamFragment;
        Debug.Log(string.Concat("Virtual dream fragments: ", virtualDreamFragment));
    }

    /// <summary>
    /// Used to copy sessionID to clipboard.
    /// Called when sessionID is clicked in main menu
    /// </summary>
    public void CopySessionID()
    {
        if(sessionID != null && sessionID != "")
        {
            GUIUtility.systemCopyBuffer = sessionID;
        }
    }

    public static string StringToAnswer(string answer)
    {
        // format answer, remove accents, upper case and multiple spaces
        string normalizedString = answer.Normalize(NormalizationForm.FormD);
        // remove multiple spaces
        normalizedString = normalizedString.Replace(" ", "");
        //normalizedString = new Regex("[ ]{2,}", RegexOptions.None).Replace(normalizedString, " ");
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
}