using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using FYFY_plugins.PointerManager;
using System.Text.RegularExpressions;

public class LoadGameContent : FSystem {
    
    private Family f_defaultGameContent = FamilyManager.getFamily(new AllOfComponents(typeof(DefaultGameContent)));

    private Family f_logos = FamilyManager.getFamily(new AllOfComponents(typeof(ImgBank)));

    private Family f_storyText = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private Family f_queriesR1 = FamilyManager.getFamily(new AnyOfTags("Q-R1"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_plankAndWireRule = FamilyManager.getFamily(new AnyOfTags("PlankAndWireRule"));
    private Family f_wrongWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro)), new NoneOfComponents(typeof(IsSolution)));
    private Family f_correctWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro), typeof(IsSolution)));
    private Family f_plankNumbers = FamilyManager.getFamily(new AnyOfTags("PlankNumbers"));

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

    private Family f_gears = FamilyManager.getFamily(new AnyOfTags("Gears"));
    private Family f_gearComponent = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));

    private Family f_login = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(PointerSensitive)));

    private Family f_bagImage = FamilyManager.getFamily(new AllOfComponents(typeof(BagImage)));

    private Family f_scrollUI = FamilyManager.getFamily(new AnyOfTags("ScrollUI"));

    private Family f_mirrorImage = FamilyManager.getFamily(new AnyOfTags("MirrorPlankImage"), new AllOfComponents(typeof(Image)));

    private Family f_passwordRoom2 = FamilyManager.getFamily(new AnyOfTags("PasswordRoom2"));
    private Family f_lockRoom2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"), new AllOfComponents(typeof(Locker)));

    private Family f_inventoryElements = FamilyManager.getFamily(new AllOfComponents(typeof(Collected)));

    private Family f_extraGeometries = FamilyManager.getFamily(new AllOfComponents(typeof(RemoveIfVeryVeryLow)));

    public static GameContent gameContent;
    private DefaultGameContent defaultGameContent;

    public TMP_FontAsset AccessibleFont;
    public TMP_FontAsset AccessibleFontUI;
    public TMP_FontAsset DefaultFont;
    public TMP_FontAsset DefaultFontUI;

    public static LoadGameContent instance;

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
                File.WriteAllText("Data/DreamFragmentLinks.txt", defaultGameContent.dreamFragmentlinks.text);

                gameContent = new GameContent();
                gameContent = JsonUtility.FromJson<GameContent>(defaultGameContent.jsonFile.text);

                File.WriteAllBytes(string.Concat("Data/", defaultGameContent.mastermindPicture.name, ".png"), defaultGameContent.mastermindPicture.EncodeToPNG());
                int l = defaultGameContent.glassesPictures.Length;
                for(int i = 0; i < l; i++)
                {
                    File.WriteAllBytes(string.Concat("Data/", defaultGameContent.glassesPictures[i].name, ".png"), defaultGameContent.glassesPictures[i].EncodeToPNG());
                }
                File.WriteAllBytes(string.Concat("Data/", defaultGameContent.plankPicture.name, ".png"), defaultGameContent.plankPicture.EncodeToPNG());

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
            forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(s));
    }

    private void Load()
    {
        //Load game content from the file
        gameContent = JsonUtility.FromJson<GameContent>(File.ReadAllText("Data/Data_LearningScape.txt"));

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
            {"Wire", gameContent.inventoryWire },
            {"Scrolls", gameContent.inventoryScrolls },
            {"Glasses1", gameContent.inventoryGlasses1 },
            {"Glasses2", gameContent.inventoryGlasses2 },
            {"Mirror", gameContent.inventoryMirror }
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
            forGO = f_queriesR1.getAt(i);
            switch (forGO.name)
            {
                case "Q2":
                    loadIARQuestion(forGO, gameContent.plankAndWireQuestionIAR, gameContent.plankAndWireAnswerFeedback, gameContent.plankAndWireAnswerFeedbackDesc, gameContent.plankAndWirePlaceHolder, gameContent.plankAndWireCorrectNumbers);
                    break;

                case "Q3":
                    loadIARQuestion(forGO, gameContent.crouchQuestion, gameContent.crouchAnswerFeedback, gameContent.crouchAnswerFeedbackDesc, gameContent.crouchPlaceHolder, gameContent.crouchAnswer);
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
            forGO = f_queriesR2.getAt(i);
            switch (forGO.name)
            {
                case "Q1":
                    loadIARQuestion(forGO, gameContent.glassesQuestion, gameContent.glassesAnswerFeedback, gameContent.glassesAnswerFeedbackDesc, gameContent.glassesPlaceHolder, gameContent.glassesAnswer);
                    break;

                case "Q3":
                    loadIARQuestion(forGO, gameContent.scrollsQuestion, gameContent.scrollsAnswerFeedback, gameContent.scrollsAnswerFeedbackDesc, gameContent.scrollsPlaceHolder, gameContent.scrollsAnswer);
                    break;

                case "Q4":
                    loadIARQuestion(forGO, gameContent.mirrorQuestion, gameContent.mirrorAnswerFeedback, gameContent.mirrorAnswerFeedbackDesc, gameContent.mirrorPlaceHolder, gameContent.mirrorAnswer);
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

        #region File Loading

        //Load dream fragment links config files
        Dictionary<string, string> dreamFragmentsLinks = null;
        LoadJsonFile(gameContent.dreamFragmentLinksPath, defaultGameContent.dreamFragmentlinks, out dreamFragmentsLinks);
        if (dreamFragmentsLinks == null)
            dreamFragmentsLinks = new Dictionary<string, string>();
        // Affects urlLinks to dream fragments
        foreach (GameObject dream_go in f_dreamFragments)
            dreamFragmentsLinks.TryGetValue(dream_go.name, out dream_go.GetComponent<DreamFragment>().urlLink);
        Debug.Log("Dream fragments links loaded");
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

                target = JsonUtility.FromJson<T>(File.ReadAllText(jsonPath));
                //target = JsonConvert.DeserializeObject <T>(File.ReadAllText(jsonPath));
            }
            catch (Exception)
            {
                target = JsonUtility.FromJson<T>(defaultContent.text);
                //target = JsonConvert.DeserializeObject<T>(defaultContent.text);
                Debug.LogError("Invalid content in file: " + jsonPath + ". Default used.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid content in file: " + jsonPath + ". Default used"));
            }
        }
        else
        {
            // write default content
            File.WriteAllText(jsonPath, defaultContent.text);
            // load default content
            target = JsonUtility.FromJson<T>(defaultContent.text);
            //target = JsonConvert.DeserializeObject<T>(defaultContent.text);
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