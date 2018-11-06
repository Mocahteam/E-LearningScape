using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class LoadGameContent : FSystem {

    private Family f_gameContent = FamilyManager.getFamily(new AllOfComponents(typeof(GameContent)));

    private Family f_storyText = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private Family f_queriesR1 = FamilyManager.getFamily(new AnyOfTags("Q-R1"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"), new AllOfComponents(typeof(QuerySolution)));
    private Family f_queriesR3 = FamilyManager.getFamily(new AnyOfTags("Q-R3"), new AllOfComponents(typeof(QuerySolution)));

    private Family f_balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family f_ballBoxTop = FamilyManager.getFamily(new AnyOfTags("BallBoxTop"));

    private Family f_plankAndWireRule = FamilyManager.getFamily(new AnyOfTags("PlankAndWireRule"));
    private Family f_wrongWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro)), new NoneOfComponents(typeof(SolutionWord)));
    private Family f_correctWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(TextMeshPro), typeof(SolutionWord)));

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

    private Family f_gears = FamilyManager.getFamily(new AnyOfTags("Gears"));
    private Family f_gearComponent = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));

    private Family f_bagImage = FamilyManager.getFamily(new AllOfComponents(typeof(BagImage)));

    private Family f_scrollUI = FamilyManager.getFamily(new AnyOfTags("ScrollUI"));

    private Family f_mirrorImage = FamilyManager.getFamily(new AnyOfTags("MirrorPlankImage"), new AllOfComponents(typeof(Image)));

    private Family f_passwordRoom2 = FamilyManager.getFamily(new AnyOfTags("PasswordRoom2"));
    private Family f_lockRoom2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"), new AllOfComponents(typeof(Locker)));

    private Family f_puzzleUI = FamilyManager.getFamily(new AnyOfTags("PuzzleUI"), new AllOfComponents(typeof(RectTransform)));


    private FSystem instance;

    private GameContent gameContent;

    private Texture2D tmpTex;
    private GameObject forGO;

    public LoadGameContent()
    {
        if (Application.isPlaying)
        {
            if (Directory.Exists("Data") && File.Exists("Data/Data_LearningScape.txt"))
            {
                //Load game content from the file
                gameContent = JsonUtility.FromJson<GameContent>(File.ReadAllText("Data/Data_LearningScape.txt"));

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
                            foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>()) {
                                if (tmp.gameObject.name == "Description")
                                {
                                    int solutionID;
                                    int.TryParse(gameContent.ballBoxAnswer[0], out solutionID);
                                    tmp.text = gameContent.ballTexts[solutionID-1];
                                    for (int j = 1; j < gameContent.ballBoxAnswer.Count; j++)
                                    {
                                        int.TryParse(gameContent.ballBoxAnswer[j], out solutionID);
                                        tmp.text = string.Concat(tmp.text, " - ", gameContent.ballTexts[solutionID-1]);
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

                            foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>()) {
                                if (tmp.gameObject.name == "Description")
                                {
                                    tmp.text = string.Concat(gameContent.plankAndWireAnswers[0], " - ", gameContent.plankAndWireAnswers[1], " - ", gameContent.plankAndWireAnswers[2]);
                                }
                            }
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

                            foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>())
                            {
                                if (tmp.gameObject.name == "Answer")
                                {
                                    tmp.text = gameContent.greenFragmentsWords[0];
                                    int l = gameContent.greenFragmentsWords.Length;
                                    for(int j = 1; j < l; j++)
                                    {
                                        tmp.text = string.Concat(tmp.text, " - ", gameContent.greenFragmentsWords[j]);
                                    }
                                }
                            }
                            forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                            forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(gameContent.greenFragmentAnswer));
                            break;

                        default:
                            break;
                    }
                }

                //Ball Box
                int nbBalls = f_balls.Count;
                Ball b;
                for (int i = 0; i < nbBalls; i++)
                {
                    b = f_balls.getAt(i).GetComponent<Ball>();
                    b.text = gameContent.ballTexts[b.id].ToUpper();
                }
                foreach (TextMeshPro tmp in f_ballBoxTop.First().GetComponentsInChildren<TextMeshPro>())
                {
                    tmp.text = gameContent.ballBoxQuestion;
                }

                //Plank and Wire
                int nbWrongWords = f_wrongWords.Count;
                int nbCorrectWords = f_correctWords.Count;
                for (int i = 0; i < nbWrongWords; i++)
                    f_wrongWords.getAt(i).GetComponent<TextMeshPro>().text = gameContent.plankOtherWords[i];
                for (int i = 0; i < nbCorrectWords; i++)
                    f_correctWords.getAt(i).GetComponent<TextMeshPro>().text = gameContent.plankAndWireAnswers[i];
                f_plankAndWireRule.First().GetComponent<TextMeshPro>().text = gameContent.plankAndWireQuestion;

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
                if(!answerGearFound)
                {
                    int nbGears = f_gearComponent.Count;
                    Gear gear = f_gearComponent.getAt((int)Random.Range(0, nbGears -1)).GetComponent<Gear>();
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
                            foreach (TextMeshProUGUI tmp in forGO.transform.GetChild(3).gameObject.GetComponentsInChildren<TextMeshProUGUI>())
                                if (tmp.gameObject.name == "Answer")
                                {
                                    tmp.text = gameContent.glassesAnswer;
                                }
                            forGO.GetComponent<QuerySolution>().andSolutions = new List<string>();
                            forGO.GetComponent<QuerySolution>().andSolutions.Add(StringToAnswer(gameContent.glassesAnswer));
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

                LoginManager.passwordSolution = gameContent.mdpLogin;

                //Glasses
                BagImage bi = f_bagImage.First().GetComponent<BagImage>();
                for(int i = 0; i < 4; i++)
                {
                    if (File.Exists(gameContent.glassesPicturesPath[i]))
                    {
                        tmpTex = new Texture2D(1, 1);
                        byte[] fileData = File.ReadAllBytes(gameContent.glassesPicturesPath[i]);
                        if (tmpTex.LoadImage(fileData))
                        {
                            Sprite mySprite = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
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
                    }
                }
                bi.gameObject.GetComponent<Image>().sprite = bi.image1;

                //Scrolls
                int nbScroll = gameContent.scrollsWords.Length < f_scrollUI.Count ? gameContent.scrollsWords.Length : f_scrollUI.Count;
                for(int i = 0; i < nbScroll; i++)
                {
                    if (gameContent.scrollsWords[i] == string.Empty)
                        GameObjectManager.setGameObjectState(f_scrollUI.getAt(i), false);
                    else
                        f_scrollUI.getAt(i).GetComponentInChildren<TextMeshProUGUI>().text = gameContent.scrollsWords[i];
                }

                //Mirror
                if (File.Exists(gameContent.mirrorPicturePath))
                {
                    tmpTex = new Texture2D(1, 1);
                    byte[] fileData = File.ReadAllBytes(gameContent.mirrorPicturePath);
                    if(tmpTex.LoadImage(fileData))
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
                int nbAnswerRoom3 = gameContent.room3Answers.Length;
                QuerySolution qs;
                for (int i = 0; i < nbQuestionRoom3; i++)
                {
                    qs = f_queriesR3.getAt(i).GetComponent<QuerySolution>();
                    qs.orSolutions = new List<string>();
                    for (int j = 0; j < nbAnswerRoom3; j++)
                        qs.orSolutions.Add(StringToAnswer(gameContent.room3Answers[j]));
                }

                //Puzzles
                if(gameContent.puzzle && File.Exists(gameContent.puzzlePicturePath))
                {
                    tmpTex = new Texture2D(1, 1);
                    byte[] fileData = File.ReadAllBytes(gameContent.puzzlePicturePath);
                    if (tmpTex.LoadImage(fileData))
                    {
                        Sprite puzzlePicture;
                        puzzlePicture = Sprite.Create(tmpTex, new Rect(0, 0, tmpTex.width, tmpTex.height), Vector2.zero);
                        Rect rect;
                        if (tmpTex.width > tmpTex.height)
                            rect = new Rect(0, 0, 935, tmpTex.height * 935 / tmpTex.width);
                        else
                            rect = new Rect(0, 0, tmpTex.width * 935 / tmpTex.height, 935);

                        int nbPuzzleUI = f_puzzleUI.Count;
                        RectTransform rt = f_puzzleUI.getAt(0).GetComponent<RectTransform>();
                        //int puzzleID = -1;
                        Vector3 newPuzzleScale = new Vector3(rect.width * rt.localScale.x / 935, rect.width * rt.localScale.y / 935, rt.localScale.z);
                        for (int i = 0; i < nbPuzzleUI; i++)
                        {
                            rt = f_puzzleUI.getAt(i).GetComponent<RectTransform>();
                            rt.localScale = newPuzzleScale;
                            rt.GetChild(0).gameObject.GetComponent<Image>().sprite = puzzlePicture;
                            /*int.TryParse(rt.gameObject.name.Substring(rt.gameObject.name.Length - 2), out puzzleID);
                            rt.GetChild(0).gameObject.GetComponent<RectTransform>().localPosition = */
                        }
                    }
                }

                //Lamp

                //White Board

                #endregion
                Debug.Log("Data loaded");
            }
            else
            {
                //create default data files
                gameContent = new GameContent();
                Directory.CreateDirectory("Data");
                File.WriteAllText("Data/Data_LearningScape.txt", JsonUtility.ToJson(gameContent, true));
                Debug.Log("Data created");
            }

            this.Pause = true;
        }
        instance = this;
    }

    private string StringToAnswer(string answer)
    {
        // format answer
        answer = answer.Replace('é', 'e');
        answer = answer.Replace('è', 'e');
        answer = answer.Replace('à', 'a');
        answer = answer.ToUpper();
        return answer;
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}
}