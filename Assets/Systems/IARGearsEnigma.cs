using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class IARGearsEnigma : FSystem
{

    // Evaluate queries inside IAR

    // Contains all query
    private Family f_gearsSet = FamilyManager.getFamily(new AnyOfTags("Gears"), new AllOfComponents(typeof(LinkedWith)));
    private Family f_uiEffects = FamilyManager.getFamily(new AnyOfTags("UIEffect"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_answer = FamilyManager.getFamily(new AnyOfTags("A-R1"), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // answers not displayed of the first room
    private Family f_gears = FamilyManager.getFamily(new AllOfComponents(typeof(Gear)));
    private Family f_rotatingGears = FamilyManager.getFamily(new AnyOfTags("RotateGear")); //gears that can rotate (middle top, middle bot, and the solution gear)

    // Will contain a game object when IAR is openned
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_login = FamilyManager.getFamily(new AnyOfTags("Login"), new NoneOfComponents(typeof(PointerSensitive))); // to unlock login

    private bool switchToGears = false;
    private bool unlockLogin = false;
    private bool rotateGear;

    private GameObject gearsEnigma;
    private GameObject forGO;
    private GameObject gearDragged;
    private GameObject transparentGear;
    private GameObject question;

    public static IARGearsEnigma instance;

    public IARGearsEnigma()
    {
        if (Application.isPlaying)
        {
            //set the initial position of each gear to their local position at the beginning of the game
            int nbGears = f_gears.Count;
            for (int i = 0; i < nbGears; i++)
            {
                forGO = f_gears.getAt(i);
                forGO.GetComponent<Gear>().initialPosition = forGO.transform.localPosition;
            }
            f_answer.addExitCallback(onNewAnswerDisplayed);
            f_uiEffects.addEntryCallback(onUiEffectFinished);
            f_iarBackground.addExitCallback(onIARClosed);

            gearsEnigma = f_gearsSet.First();
            question = gearsEnigma.transform.GetChild(0).gameObject; // first child is the question text
            transparentGear = gearsEnigma.transform.GetChild(7).gameObject; // eight child is the transparent gear
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
            GameObjectManager.setGameObjectState(gearsEnigma, true);
            // Hide queries
            GameObjectManager.setGameObjectState(gearsEnigma.GetComponent<LinkedWith>().link, false);

            switchToGears = false;
        }
    }

    private void onIARClosed(int instanceId)
    {
        if (unlockLogin)
        {
            // Make login selectable
            GameObjectManager.addComponent<Selectable>(f_login.First(), new { standingPosDelta = new Vector3(-0.8f, -0.76f, 0f), standingOrientation = new Vector3(1f, 0f, 0f) });
            // And force to move on
            GameObjectManager.addComponent<ForceMove>(f_login.First());
            unlockLogin = false;
            LoginManager.instance.Pause = false;
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        //if the player is playing enigma04 and didn't answer
        if (gearsEnigma.activeSelf)
        {
            int nbGears = f_gears.Count;
            for (int i = 0; i < nbGears; i++)
            {
                forGO = f_gears.getAt(i);
                //if a gear is dragged
                if (forGO.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
                {
                    gearDragged = forGO; //save the dragged gear
                    GameObjectManager.setGameObjectState(question, false);
                    GameObjectManager.setGameObjectState(transparentGear, true);
                }
            }
            if (gearDragged != null) //if a gear is dragged
            {
                rotateGear = false; //initial value
                if (Input.GetMouseButtonUp(0))  //when the gear is released
                {
                    GameObjectManager.setGameObjectState(transparentGear, false);
                    //if the gear is released in the center of the tablet (player answering)
                    if (gearDragged.transform.localPosition.x < 125 && gearDragged.transform.localPosition.x > -125 && gearDragged.transform.localPosition.y < 125f / 2 && gearDragged.transform.localPosition.x > -125f / 2)
                    {
                        gearDragged.transform.localPosition = Vector3.zero; //place the gear at the center
                        if (gearDragged.GetComponent<Gear>().isSolution) //if answer is correct
                        {
                            //start audio and animation for "Right answer"
                            if (HelpSystem.monitoring)
                            {
                                MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(69), "Correct");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }

                            rotateGear = true;  //rotate gears in the middle
                            GameObjectManager.addComponent<PlayUIEffect>(gearDragged, new { effectCode = 3 });
                            unlockLogin = true; // unlock login when UIEffect will be end
                        }
                        else //if answer is wrong
                        {
                            GameObjectManager.setGameObjectState(question, true);
                            //start audio and animation for "Wrong answer"
                            GameObjectManager.addComponent<PlayUIEffect>(gearDragged, new { effectCode = 1 });
                            if (HelpSystem.monitoring)
                            {
                                MonitoringTrace trace = new MonitoringTrace(MonitoringManager.getMonitorById(69), "Wrong");
                                trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                                HelpSystem.traces.Enqueue(trace);
                            }

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
                else //when dragging a gear
                {
                    //move the gear to mouse position
                    gearDragged.transform.localPosition = (Input.mousePosition - Vector3.right * (float)Camera.main.pixelWidth / 2 - Vector3.up * (float)Camera.main.pixelHeight / 2) / 0.45f;
                }
            }
        }
        if (rotateGear) //true when the correct answer is given
        {
            int nbRotGears = f_rotatingGears.Count;
            for (int i = 0; i < nbRotGears; i++)
            {
                forGO = f_rotatingGears.getAt(i);
                //rotate gears in the middle
                if (forGO.GetComponent<Gear>())
                    forGO.transform.rotation = Quaternion.Euler(forGO.transform.rotation.eulerAngles.x, forGO.transform.rotation.eulerAngles.y, forGO.transform.rotation.eulerAngles.z - 1);
                else
                    forGO.transform.rotation = Quaternion.Euler(forGO.transform.rotation.eulerAngles.x, forGO.transform.rotation.eulerAngles.y, forGO.transform.rotation.eulerAngles.z + 1);
            }
        }
    }
}