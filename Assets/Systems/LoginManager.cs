using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using FYFY_plugins.Monitoring;

public class LoginManager : FSystem {

    // this system manage the login panel (mastermind)

    private Family f_focusedLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeLogin = FamilyManager.getFamily(new AnyOfTags("Login", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_mainWindow = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(PointerSensitive)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));
    private Family f_forceMoveToLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ForceMove)));

    private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)), new AnyOfTags("Player"));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    // Selectable Component is dynamically added by IARGearsEnigma when this enigma is solved => this is a sure condition to know that login is unlocked
    private Family f_loginUnlocked = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(Selectable)));

    private Family f_storyDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private GameObject selectedLoginPanel;
    private GameObject loginCover;
    private Vector3 coverTargetPosition;
    private Vector3 playerGoBackPosition;
    private Vector3 doorOpennedPosition;

    private float speed;
    private InputField ifConnectionR2;
    public static int passwordSolution = 814;
    private GameObject door;

    private AudioSource audioSource;

    private TextMeshProUGUI connectionAnswerCheck1;
    private TextMeshProUGUI connectionAnswerCheck2;
    private TextMeshProUGUI connectionAnswerCheck3;
    private Color cacGreen;
    private Color cacOrange;
    private Color cacRed;

    private bool processEndAnimation = false;
    private bool goBack = false;
    private bool openDoor = false;

    private bool coverAnimate = false;

    private string exitBy = "";
    private string openedBy = "";

    public static LoginManager instance;

    public LoginManager()
    {
        if (Application.isPlaying)
        {
            InputField inputField = f_mainWindow.First().transform.GetChild(1).GetComponent<InputField>();

            ifConnectionR2 = inputField;

            // get fourth child of the password and backup answer UI notifications
            GameObject answerCheck = inputField.gameObject.transform.GetChild(3).gameObject;
            connectionAnswerCheck1 = answerCheck.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            cacGreen = connectionAnswerCheck1.color;
            connectionAnswerCheck2 = answerCheck.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            cacOrange = connectionAnswerCheck2.color;
            connectionAnswerCheck3 = answerCheck.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            cacRed = connectionAnswerCheck3.color;

            f_loginUnlocked.addEntryCallback(onLoginUnlocked);
            f_focusedLogin.addEntryCallback(onReadyToWorkOnLogin);
            f_forceMoveToLogin.addEntryCallback(onForceMoveTo);

            audioSource = f_player.First().GetComponent<AudioSource>();
        }
        instance = this;
    }

    private void onLoginUnlocked(GameObject go)
    {
        // launch animation of login protection
        loginCover = go.transform.GetChild(0).gameObject; // the first child is the cover
        coverTargetPosition = loginCover.transform.position - (Vector3.up); 
        playerGoBackPosition = go.transform.position + (Vector3.left*3f) - (Vector3.up);
        door = f_door.First();
        doorOpennedPosition = door.transform.position + (Vector3.up*4f);
        coverAnimate = true;
    }

    private void onReadyToWorkOnLogin(GameObject go)
    {
        selectedLoginPanel = go;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "turnOn", performedBy = openedBy });
        openedBy = "player";
    }

    private void onForceMoveTo(GameObject go)
    {
        openedBy = "system";
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
	protected override void onProcess(int familiesUpdateCount)
    {
        if (selectedLoginPanel)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closeLogin.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && !coverAnimate && !processEndAnimation)
            {
                exitBy = "player";
                ExitLogin();
            }
        }

        speed = Time.deltaTime;

        if (coverAnimate)
        {
            // open the cover of the box
            loginCover.transform.position = Vector3.MoveTowards(loginCover.transform.position, coverTargetPosition, speed);
            if (loginCover.transform.position == coverTargetPosition)
                coverAnimate = false;
        }

        if (processEndAnimation)
        {
            if (goBack)
            {
                f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, playerGoBackPosition, speed);
                if (f_player.First().transform.position == playerGoBackPosition)
                {
                    goBack = false;
                    audioSource.clip = door.GetComponent<Door>().openAudio;
                    audioSource.PlayDelayed(0);
                    audioSource.loop = true;
                    openDoor = true;
                    // enable rooms two and three
                    GameObjectManager.setGameObjectState(f_gameRooms.First().transform.GetChild(2).gameObject, true);
                    GameObjectManager.setGameObjectState(f_gameRooms.First().transform.GetChild(3).gameObject, true);
                }
            }

            if (openDoor)
            {
                door.transform.position = Vector3.MoveTowards(door.transform.position, doorOpennedPosition, speed);
                if (door.transform.position == doorOpennedPosition)
                {
                    openDoor = false;
                    audioSource.loop = false;
                    processEndAnimation = false;
                    // show story
                    f_storyDisplayer.First().GetComponent<StoryText>().storyProgression++;
                    StoryDisplaying.instance.Pause = false;
                    // Enable IAR second screen
                    GameObject IARsecondScreen = f_mainWindow.First().GetComponentInChildren<LinkedWith>().link;
                    GameObjectManager.setGameObjectState(IARsecondScreen.transform.GetChild(0).gameObject, false); // first child is locked tab
                    GameObjectManager.setGameObjectState(IARsecondScreen.transform.GetChild(1).gameObject, true); // second child is unlocked tab
                    GameObjectManager.addComponent<ActionPerformedForLRS>(IARsecondScreen, new { verb = "unlocked", objectType = "menu", objectName = IARsecondScreen.name });
                    f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 2;
                    // exit login
                    exitBy = "system";
                    ExitLogin();
                }
            }
        }
	}

    private void ExitLogin()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLoginPanel);

        GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "turnOff", performedBy = exitBy });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLoginPanel, new
        {
            verb = "exited",
            objectType = "interactable",
            objectName = selectedLoginPanel.name
        });

        selectedLoginPanel = null;

        // Pause this system
        instance.Pause = true;
    }

    public void CheckMastermindAnswer() //mastermind
    {
        int answer;
        int.TryParse(ifConnectionR2.text, out answer);
        if (ifConnectionR2.text == "")
            answer = -1;

        if (answer == passwordSolution) //if the answer is correct
        {
            GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "Correct", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "perform", performedBy = "system" });
            GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLoginPanel, new
            {
                verb = "answered",
                objectType = "question",
                objectName = selectedLoginPanel.name,
                result = true,
                success = 1,
                response = answer.ToString()
            });

            //show correct answer feedback for the 3 numbers
            connectionAnswerCheck1.text = "O";
            connectionAnswerCheck1.color = cacGreen;
            connectionAnswerCheck2.text = "O";
            connectionAnswerCheck2.color = cacGreen;
            connectionAnswerCheck3.text = "O";
            connectionAnswerCheck3.color = cacGreen;
            // solution found play animation
            processEndAnimation = true;
            goBack = true;
        }
        else
        {
            //else, feedback following the rules of mastermind ('O' correct, '?' right number but wrong place, 'X' wrong number)

            GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "Wrong", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLoginPanel, new
            {
                verb = "answered",
                objectType = "question",
                objectName = selectedLoginPanel.name,
                result = true,
                success = -1,
                response = answer.ToString()
            });

            ifConnectionR2.ActivateInputField();
            int answerHundreds = answer / 100;
            int answerTens = answer / 10 % 10;
            int answerUnits = answer % 10;
            int solutionHundreds = passwordSolution / 100;
            int solutionTens = passwordSolution / 10 % 10;
            int solutionUnits = passwordSolution % 10;

            if (answerHundreds == solutionHundreds)
            {
                connectionAnswerCheck1.text = "O";
                connectionAnswerCheck1.color = cacGreen;
            }
            else if ((answerTens != solutionTens && answerHundreds == solutionTens) || (answerUnits != solutionUnits && answerHundreds == solutionUnits))
            {
                connectionAnswerCheck1.text = "?";
                connectionAnswerCheck1.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck1.text = "X";
                connectionAnswerCheck1.color = cacRed;
            }

            if (answerTens == solutionTens)
            {
                connectionAnswerCheck2.text = "O";
                connectionAnswerCheck2.color = cacGreen;
            }
            else if ((answerHundreds != solutionHundreds && answerTens == solutionHundreds) || (answerUnits != solutionUnits && answerTens == solutionUnits))
            {
                connectionAnswerCheck2.text = "?";
                connectionAnswerCheck2.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck2.text = "X";
                connectionAnswerCheck2.color = cacRed;
            }

            if (answerUnits == solutionUnits)
            {
                connectionAnswerCheck3.text = "O";
                connectionAnswerCheck3.color = cacGreen;
            }
            else if ((answerHundreds != solutionHundreds && answerUnits == solutionHundreds) || (answerTens != solutionTens && answerUnits == solutionTens))
            {
                connectionAnswerCheck3.text = "?";
                connectionAnswerCheck3.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck3.text = "X";
                connectionAnswerCheck3.color = cacRed;
            }

            GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLoginPanel, new
            {
                verb = "received",
                objectType = "feedback",
                objectName = string.Concat(selectedLoginPanel.name, "_feedback"),
                activityExtensions = new Dictionary<string, List<string>>() {
                    { "content", new List<string>() {
                        string.Concat(connectionAnswerCheck1, connectionAnswerCheck2, connectionAnswerCheck3), answer.ToString() } },
                    { "type", new List<string>() { "answer validation" } }
                }
            });
        }
    }
}