﻿using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;

public class LoginManager : FSystem {

    // this system manage the login panel

    private Family f_focusedLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeLogin = FamilyManager.getFamily(new AnyOfTags("Login", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_mainWindow = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(PointerSensitive)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family gameRooms = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)), new AnyOfTags("GameRooms"));

    // Selectable Component is dynamically added by IARGearsEnigma when this enigma is solved => this is a sure condition to know that login is unlocked
    private Family f_loginUnlocked = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(Selectable)));

    private Family storyDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private GameObject selectedLoginPanel;
    private GameObject loginCover;
    private float bonnetSpeedRotation = 200f;
    private float tmpRotationCount = 0;
    private Vector3 coverTargetPosition;
    private Vector3 playerGoBackPosition;
    private Vector3 doorOpennedPosition;
    private float speed;
    private InputField ifConnectionR2;
    private int passwordSolution = 345;
    private GameObject door;

    private AudioSource gameAudioSource;

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
    
    public static LoginManager instance;

    public LoginManager()
    {
        if (Application.isPlaying)
        {

            f_mainWindow.First().GetComponentInChildren<Button>().onClick.AddListener(CheckConnection);
            foreach (InputField inputField in f_mainWindow.First().GetComponentsInChildren<InputField>())
            {
                if (inputField.gameObject.name == "Password")
                {
                    // Add listeners
                    inputField.onEndEdit.AddListener(delegate {
                        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            CheckConnection();
                        }
                    });

                    ifConnectionR2 = inputField;

                    // get fourth child of the password and backup answer UI notifications
                    GameObject answerCheck = inputField.gameObject.transform.GetChild(3).gameObject;
                    connectionAnswerCheck1 = answerCheck.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    cacGreen = connectionAnswerCheck1.color;
                    connectionAnswerCheck2 = answerCheck.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    cacOrange = connectionAnswerCheck2.color;
                    connectionAnswerCheck3 = answerCheck.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                    cacRed = connectionAnswerCheck3.color;
                }
            }

            f_loginUnlocked.addEntryCallback(onLoginUnlocked);
            f_focusedLogin.addEntryCallback(onReadyToWorkOnLogin);

            gameAudioSource = gameRooms.First().GetComponent<AudioSource>();
        }
        instance = this;
    }

    private void onLoginUnlocked(GameObject go)
    {
        // launch animation of login protection
        loginCover = go.transform.GetChild(0).gameObject; // the first child is the cover
        coverTargetPosition = go.transform.position - (Vector3.up); 
        coverAnimate = true;
        playerGoBackPosition = go.transform.position + (Vector3.left*3f) - (Vector3.up);
        door = f_door.First();
        doorOpennedPosition = door.transform.position + (Vector3.up*4f);
    }

    private void onReadyToWorkOnLogin(GameObject go)
    {
        selectedLoginPanel = go;
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
            if (((f_closeLogin.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)))
                ExitLogin();
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
                player.First().transform.position = Vector3.MoveTowards(player.First().transform.position, playerGoBackPosition, speed);
                if (player.First().transform.position == playerGoBackPosition)
                {
                    goBack = false;
                    gameAudioSource.clip = door.GetComponent<Door>().openAudio;
                    gameAudioSource.PlayDelayed(0);
                    gameAudioSource.loop = true;
                    openDoor = true;
                    // enable rooms two and three
                    GameObjectManager.setGameObjectState(gameRooms.First().transform.GetChild(2).gameObject, true);
                    GameObjectManager.setGameObjectState(gameRooms.First().transform.GetChild(3).gameObject, true);
                }
            }

            if (openDoor)
            {
                door.transform.position = Vector3.MoveTowards(door.transform.position, doorOpennedPosition, speed);
                if (door.transform.position == doorOpennedPosition)
                {
                    openDoor = false;
                    gameAudioSource.loop = false;
                    processEndAnimation = false;
                    // show story
                    storyDisplayer.First().GetComponent<StoryText>().storyProgression++;
                    StoryDisplaying.instance.Pause = false;
                    // exit login
                    ExitLogin();
                }
            }
        }
	}

    private void ExitLogin()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLoginPanel);

        selectedLoginPanel = null;
    }

    private void CheckConnection() //mastermind
    {
        int answer;
        int.TryParse(ifConnectionR2.text, out answer);

        if (answer == passwordSolution) //if the answer is correct
        {
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
            ifConnectionR2.ActivateInputField();
            //else, feedback following the rules of mastermind ('O' correct, '?' right number but wrong place, 'X' wrong number)
            f_mainWindow.First().GetComponentInChildren<InputField>().ActivateInputField();
            if (answer / 100 == passwordSolution / 100)
            {
                connectionAnswerCheck1.text = "O";
                connectionAnswerCheck1.color = cacGreen;
            }
            else if (passwordSolution.ToString().Contains((answer / 100).ToString()))
            {
                connectionAnswerCheck1.text = "?";
                connectionAnswerCheck1.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck1.text = "X";
                connectionAnswerCheck1.color = cacRed;
            }

            if (answer / 10 % 10 == passwordSolution / 10 % 10)
            {
                connectionAnswerCheck2.text = "O";
                connectionAnswerCheck2.color = cacGreen;
            }
            else if (passwordSolution.ToString().Contains((answer / 10 % 10).ToString()))
            {
                connectionAnswerCheck2.text = "?";
                connectionAnswerCheck2.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck2.text = "X";
                connectionAnswerCheck2.color = cacRed;
            }

            if (answer % 10 == passwordSolution % 10)
            {
                connectionAnswerCheck3.text = "O";
                connectionAnswerCheck3.color = cacGreen;
            }
            else if (passwordSolution.ToString().Contains((answer % 10).ToString()))
            {
                connectionAnswerCheck3.text = "?";
                connectionAnswerCheck3.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck3.text = "X";
                connectionAnswerCheck3.color = cacRed;
            }
        }
    }
}