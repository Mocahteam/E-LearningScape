using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;

public class LoginManager : FSystem {

    // this system manage the login panel (mastermind)

    private Family f_focusedLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeLogin_1 = FamilyManager.getFamily(new AnyOfTags("Login", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_closeLogin_2 = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(HUD_TransparentOnMove)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_forceMoveToLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ForceMove)));

    // Selectable Component is dynamically added by IARGearsEnigma when this enigma is solved => this is a sure condition to know that login is unlocked
    private Family f_loginUnlocked = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(Selectable)));

    public GameObject mainWindow;
    public GameObject door;
    public Transform player;
    public GameObject rooms;
    public UnlockedRoom unlockedRoom;
    public StoryText storyText;


    private GameObject selectedLoginPanel;
    private Vector3 playerGoBackPosition;

    private float speed;
    private TMP_InputField ifConnectionR2;
    public static int passwordSolution;

    private TextMeshProUGUI connectionAnswerCheck1;
    private TextMeshProUGUI connectionAnswerCheck2;
    private TextMeshProUGUI connectionAnswerCheck3;
    private Color cacGreen;
    private Color cacOrange;
    private Color cacRed;

    private bool goBack = false;

    private string exitBy = "";
    private string openedBy = "";

    public static LoginManager instance;

    public LoginManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        TMP_InputField inputField = mainWindow.transform.GetChild(1).GetComponent<TMP_InputField>();

        ifConnectionR2 = inputField;

        // get fourth child of the password and backup answer UI notifications
        GameObject answerCheck = inputField.gameObject.transform.GetChild(2).gameObject;
        connectionAnswerCheck1 = answerCheck.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        cacGreen = connectionAnswerCheck1.color;
        connectionAnswerCheck2 = answerCheck.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        cacOrange = connectionAnswerCheck2.color;
        connectionAnswerCheck3 = answerCheck.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        cacRed = connectionAnswerCheck3.color;

        f_loginUnlocked.addEntryCallback(onLoginUnlocked);
        f_focusedLogin.addEntryCallback(onReadyToWorkOnLogin);
        f_forceMoveToLogin.addEntryCallback(onForceMoveTo);
    }

    private void onLoginUnlocked(GameObject go)
    {
        // launch animation of login protection
        GameObject loginCover = go.transform.GetChild(0).gameObject; // the first child is the cover
        playerGoBackPosition = go.transform.position + (Vector3.left*3f) - (Vector3.up);

        GameObjectManager.addComponent<PlaySound>(loginCover, new { id = 9 }); // id refer to FPSController AudioBank
        loginCover.GetComponent<Animator>().enabled = true; // enable animation
    }

    private void onReadyToWorkOnLogin(GameObject go)
    {
        selectedLoginPanel = go;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "turnOn", performedBy = openedBy });
        openedBy = "player";

        GameObjectManager.addComponent<ActionPerformedForLRS>(go, new
        {
            verb = "accessed",
            objectType = "mastermind"
        });
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
            if (((f_closeLogin_1.Count == 0 && f_closeLogin_2.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && !goBack)
            {
                exitBy = "player";
                ExitLogin();
            }
        }

        speed = Time.deltaTime;

        if (goBack)
        {
            player.position = Vector3.MoveTowards(player.position, playerGoBackPosition, speed);
            if (player.position == playerGoBackPosition)
            {
                // show story
                storyText.storyProgression++;
                StoryDisplaying.instance.Pause = false;

                UnlockLoginDoor();

                GameObject IARsecondScreen = mainWindow.GetComponentInChildren<LinkedWith>().link;

                unlockedRoom.roomNumber = 2;
                // exit login
                exitBy = "system";
                ExitLogin();
                goBack = false;
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
            objectType = "mastermind"
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
                objectType = "mastermind",
                result = true,
                success = 1,
                response = answer.ToString()
            });
            GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLoginPanel, new
            {
                verb = "completed",
                objectType = "quest",
                activityExtensions = new Dictionary<string, string>() {
                        { "value", "room1" }
                    }
            });

            //show correct answer feedback for the 3 numbers
            connectionAnswerCheck1.text = "O";
            connectionAnswerCheck1.color = cacGreen;
            connectionAnswerCheck2.text = "O";
            connectionAnswerCheck2.color = cacGreen;
            connectionAnswerCheck3.text = "O";
            connectionAnswerCheck3.color = cacGreen;

            // enable rooms two and three
            GameObjectManager.setGameObjectState(rooms.transform.GetChild(2).gameObject, true);
            GameObjectManager.setGameObjectState(rooms.transform.GetChild(3).gameObject, true);
            // solution found play animation
            goBack = true;
            GameObjectManager.addComponent<PlaySound>(door, new { id = 9 }); // id refer to FPSController AudioBank
            // lock login
            GameObjectManager.removeComponent<Selectable>(f_loginUnlocked.First());
        }
        else
        {
            //else, feedback following the rules of mastermind ('O' correct, '?' right number but wrong place, 'X' wrong number)

            GameObjectManager.addComponent<ActionPerformed>(selectedLoginPanel, new { name = "Wrong", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformedForLRS>(selectedLoginPanel, new
            {
                verb = "answered",
                objectType = "mastermind",
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
                activityExtensions = new Dictionary<string, string>() {
                    { "value", selectedLoginPanel.name },
                    { "content", string.Concat(connectionAnswerCheck1.text, connectionAnswerCheck2.text, connectionAnswerCheck3.text) }
                }
            });
        }
    }

    public void OnEndEditMastermindAnswer()
    {
        if (Input.GetButtonDown("Submit"))
            CheckMastermindAnswer();
    }

    public void UnlockLoginDoor()
    {
        door.GetComponent<Animator>().enabled = true; // enable animation
        // Enable IAR second screen
        GameObjectManager.setGameObjectState(mainWindow.GetComponentInChildren<LinkedWith>().link, true); // enable questions tab
    }
}