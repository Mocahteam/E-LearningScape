﻿using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class IARGearsEnigma : FSystem
{

    // Evaluate queries inside IAR

    // Contains all query
    private Family f_gearsSet = FamilyManager.getFamily(new AnyOfTags("Gears"), new AllOfComponents(typeof(LinkedWith)));
    private Family f_uiEffects = FamilyManager.getFamily(new AnyOfTags("UIEffect"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_answer = FamilyManager.getFamily(new AnyOfTags("A-R1"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not displayed of the first room
    private Family f_gears = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));
    private Family f_rotatingGears = FamilyManager.getFamily(new AnyOfTags("RotateGear")); //gears that can rotate (middle top, middle bot, and the solution gear)
    private Family f_canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));

    // Will contain a game object when IAR is openned
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_login = FamilyManager.getFamily(new AnyOfTags("Login"), new NoneOfComponents(typeof(PointerSensitive))); // to unlock login
    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private Family f_terminalScreens = FamilyManager.getFamily(new AnyOfTags("TerminalScreen"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    private bool switchToGears = false;
    private bool takeGearsScreenshot = false;
    private bool correctAnswerScreenshotTaken = false; //used to do it only once
    private bool unlockLogin = false;
    private bool rotateGear;

    private GameObject gears;
    private GameObject tmpGO;
    private GameObject gearDragged;
    private GameObject transparentGear;
    private GameObject question;

    private bool avoidCoroutine = false;

    private RectTransform iarRectTransform;

    public static IARGearsEnigma instance;

    public IARGearsEnigma()
    {
        if (Application.isPlaying)
        {
            //set the initial position of each gear to their local position at the beginning of the game
            int nbGears = f_gears.Count;
            for (int i = 0; i < nbGears; i++)
            {
                tmpGO = f_gears.getAt(i);
                tmpGO.GetComponent<Gear>().initialPosition = tmpGO.transform.localPosition;
            }
            f_answer.addExitCallback(onNewAnswerDisplayed);
            f_uiEffects.addEntryCallback(onUiEffectFinished);
            f_iarBackground.addExitCallback(onIARClosed);

            gears = f_gearsSet.First();
            question = gears.transform.GetChild(0).gameObject; // first child is the question text
            transparentGear = gears.transform.GetChild(7).gameObject; // eight child is the transparent gear

            int nbCanvas = f_canvas.Count;
            for(int i = 0; i < nbCanvas; i++)
            {
                tmpGO = f_canvas.getAt(i);
                if(tmpGO.name == "IAR")
                {
                    iarRectTransform = tmpGO.GetComponent<RectTransform>();
                    break;
                }
            }
        }
        instance = this;
    }

    private void onNewAnswerDisplayed(int instanceId)
    {
        // When all answer was displayed => ask to switch to gears when animation will end (launched by IARQueryEvaluator)
        if (f_answer.Count == 0)
            switchToGears = true;
    }

    private void onUiEffectFinished(GameObject go)
    {
        if (switchToGears)
        {
            // Show gears
            GameObjectManager.setGameObjectState(gears, true);
            // Hide queries
            GameObjectManager.setGameObjectState(gears.GetComponent<LinkedWith>().link, false);

            switchToGears = false;
            takeGearsScreenshot = true;
        }
        if(!correctAnswerScreenshotTaken && rotateGear)
        {
            takeGearsScreenshot = true; //take a screenshot of the correct answer for the terminal
            correctAnswerScreenshotTaken = true;
        }
    }

    private void onIARClosed(int instanceId)
    {
        if (unlockLogin)
        {
            // Make login selectable
            GameObjectManager.addComponent<Selectable>(f_login.First(), new { standingPosDelta = new Vector3(-0.9f, -0.8f, 0f), standingOrientation = new Vector3(1f, 0f, 0f) });
            GameObjectManager.addComponent<PointerSensitive>(f_login.First());
            // And force to move on
            GameObjectManager.addComponent<ForceMove>(f_login.First());
            unlockLogin = false;
            LoginManager.instance.Pause = false;
            avoidCoroutine = true;
        }
    }

    private IEnumerator autoCloseIAR()
    {
        yield return new WaitForSeconds(3);
        if (!avoidCoroutine)
            IARTabNavigation.instance.closeIar();
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        //take a screenshot of IAR for the terminal
        //has to be done here rather than in onUiEffectFinished to wait one frame for GameObjectManager.setGameObjectState to display gears
        if (takeGearsScreenshot)
        {
            // put a screenshot of the IAR on the terminal when gears are displayed
            int lastUnlockedRoom = f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber;
            if (f_terminalScreens.Count >= lastUnlockedRoom)
                MainLoop.instance.StartCoroutine(IARQueryEvaluator.instance.SetTerminalScreen());
            takeGearsScreenshot = false;
        }

        //if the player is playing enigma04 and didn't answer
        if (gears.activeSelf && !rotateGear)
        {
            int nbGears = f_gears.Count;
            for (int i = 0; i < nbGears; i++)
            {
                tmpGO = f_gears.getAt(i);
                //if a gear is dragged
                if (tmpGO.GetComponent<PointerOver>() && Input.GetButtonDown("Fire1"))
                {
                    gearDragged = tmpGO; //save the dragged gear
                    GameObjectManager.setGameObjectState(question, false);
                    GameObjectManager.setGameObjectState(transparentGear, true);
                    GameObjectManager.addComponent<ActionPerformedForLRS>(gearDragged, new
                    {
                        verb = "dragged",
                        objectType = "gear",
                        activityExtensions = new Dictionary<string, string>() {
                            { "value", gearDragged.name }
                        }
                    });
                }
            }
            if (gearDragged != null) //if a gear is dragged
            {
                rotateGear = false; //initial value
                if (Input.GetButtonUp("Fire1"))  //when the gear is released
                {
                    GameObjectManager.addComponent<ActionPerformedForLRS>(gearDragged, new
                    {
                        verb = "dropped",
                        objectType = "gear",
                        activityExtensions = new Dictionary<string, string>() {
                            { "value", gearDragged.name }
                        }
                    });
                    GameObjectManager.setGameObjectState(transparentGear, false);
                    //if the gear is released in the center of the tablet (player answering)
                    if (gearDragged.transform.localPosition.x < 125 && gearDragged.transform.localPosition.x > -125 && gearDragged.transform.localPosition.y < 125f / 2 && gearDragged.transform.localPosition.x > -125f / 2)
                    {
                        gearDragged.transform.localPosition = Vector3.zero; //place the gear at the center
                        if (gearDragged.GetComponent<Gear>().isSolution) //if answer is correct
                        {
                            GameObjectManager.addComponent<ActionPerformed>(gears, new { name = "Correct", performedBy = "player" });
                            GameObjectManager.addComponent<ActionPerformed>(gears, new { name = "perform", performedBy = "system" });
                            GameObjectManager.addComponent<ActionPerformedForLRS>(gears, new 
                            { 
                                verb = "answered",
                                objectType = "question",
                                activityExtensions = new Dictionary<string, string>() {
                                    { "value", gearDragged.name }
                                },
                                result = true,
                                success = 1,
                                response = gearDragged.GetComponentInChildren<TextMeshProUGUI>().text
                            });

                            //start audio and animation for "Right answer"

                            rotateGear = true;  //rotate gears in the middle
                            GameObjectManager.addComponent<PlayUIEffect>(gearDragged, new { effectCode = 2 });
                            unlockLogin = true; // unlock login when UIEffect will be end
                            // Look the panel
                            Vector3 newDir = Vector3.forward;
                            f_player.First().transform.rotation = Quaternion.LookRotation(f_login.First().transform.position - f_player.First().transform.position);
                            Camera.main.transform.rotation = Quaternion.LookRotation(f_login.First().transform.position - f_player.First().transform.position);
                            
                            MainLoop.instance.StartCoroutine(autoCloseIAR());
                        }
                        else //if answer is wrong
                        {
                            GameObjectManager.addComponent<ActionPerformed>(gears, new { name = "Wrong", performedBy = "player" });
                            GameObjectManager.addComponent<ActionPerformedForLRS>(gears, new
                            {
                                verb = "answered",
                                objectType = "question",
                                activityExtensions = new Dictionary<string, string>() {
                                    { "value", gearDragged.name }
                                },
                                result = true,
                                success = -1,
                                response = gearDragged.GetComponentInChildren<TextMeshProUGUI>().text
                            });

                            GameObjectManager.setGameObjectState(question, true);
                            //start audio and animation for "Wrong answer"
                            GameObjectManager.addComponent<PlayUIEffect>(gearDragged, new { effectCode = 1 });
                            GameObjectManager.addComponent<WrongAnswerInfo>(gears, new { givenAnswer = gearDragged.GetComponentInChildren<TextMeshProUGUI>().text });
                            gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition; //set gear position to initial position
                        }
                        gearDragged = null; //initial value
                    }
                    else //if the gear is not released at the center
                    {
                        GameObjectManager.setGameObjectState(question, true);
                        gearDragged.transform.localPosition = gearDragged.GetComponent<Gear>().initialPosition; //set gear position to initial position
                    }
                    gearDragged = null; //initial value
                }
                else // when dragging a gear
                {
                    // move the gear to mouse position
                    // compute mouse position from screen center
                    Vector3 pos = (Input.mousePosition.x / Screen.width -0.5f) * Vector3.right + (Input.mousePosition.y / Screen.height - 0.5f) * Vector3.up;
                    // correct position depending on canvas scale (0.6) and screen size comparing to reference size (800:500 => 16:10 screen ratio)
                    gearDragged.transform.localPosition = Vector3.Scale(pos, iarRectTransform.sizeDelta/0.6f);
                }
            }
        }
        if (rotateGear) //true when the correct answer is given
        {
            int nbRotGears = f_rotatingGears.Count;
            for (int i = 0; i < nbRotGears; i++)
            {
                tmpGO = f_rotatingGears.getAt(i);
                //rotate gears in the middle
                if (tmpGO.GetComponent<Gear>())
                    tmpGO.transform.rotation = Quaternion.Euler(tmpGO.transform.rotation.eulerAngles.x, tmpGO.transform.rotation.eulerAngles.y, tmpGO.transform.rotation.eulerAngles.z - 1);
                else
                    tmpGO.transform.rotation = Quaternion.Euler(tmpGO.transform.rotation.eulerAngles.x, tmpGO.transform.rotation.eulerAngles.y, tmpGO.transform.rotation.eulerAngles.z + 1);
            }
        }
    }

    public void SolveGearsEnigma()
    {
        foreach (GameObject gear in f_gears)
            if (gear.GetComponent<Gear>().isSolution)
                gear.transform.localPosition = Vector3.zero; //place the gear at the center
        rotateGear = true;  //rotate gears in the middle
        // Make login selectable
        GameObjectManager.addComponent<Selectable>(f_login.First(), new { standingPosDelta = new Vector3(-0.9f, -0.8f, 0f), standingOrientation = new Vector3(1f, 0f, 0f) });
    }

    public bool IsResolved()
    {
        return rotateGear;
    }
}