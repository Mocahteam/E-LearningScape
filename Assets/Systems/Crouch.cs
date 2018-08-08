using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;

public class Crouch : FSystem
{

    private Family player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));
    private Family linkedHud = FamilyManager.getFamily(new AnyOfTags("EnableOnFirstCrouch"), new AllOfComponents(typeof(Image)));
    private Family endRoom = FamilyManager.getFamily(new AnyOfTags("EndRoom"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private bool crouching = false; // true when the player is crouching
    private bool changingPose = false;
    private float crouchingSpeed;
    private Vector3 targetScale;
    private Vector3 crouchingScale;
    private Vector3 standingScale = Vector3.one;
    private bool firstCrouchOccurs = false;
    private FirstPersonController playerController;
    private Image tmpImage;

    private bool playerIsWalking = false;
    private bool playerWasWalking = false;
    private bool showHUD;
    private bool hideHUD;
    private float hudShowingSpeed;
    private float hudHidingSpeed;
    private bool previousHUDState;

    public Crouch()
    {
        //when crouching, the scale of the player is changed (rather than its position)
        crouchingScale = Vector3.one * 0.2f;
        if (Application.isPlaying)
        {
            playerController = player.First().GetComponent<FirstPersonController>();
        }
    }

    private void SetHUD(bool state)
    {
        if (firstCrouchOccurs && previousHUDState != state)
        {
            foreach (GameObject hud in linkedHud)
                GameObjectManager.setGameObjectState(hud, state);
            previousHUDState = state;
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        SetHUD(playerController.enabled && endRoom.Count == 0);

        if (playerController.enabled)
        {
            crouchingSpeed = 70f * Time.deltaTime;
            // when control button or right click is pressed and nothing is selected and inventory isn't opened and story is not displing and no fragment is swhown then the player can crouch and standing
            if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(1)) && playerController.enabled && !Selectable.selected && !CollectableGO.onInventory && !StoryDisplaying.reading && !DreamFragmentCollect.onFragment)
            {
                changingPose = true; //true when the player is crouching or standing
                //change moving speed according to the stance
                if (crouching)
                {
                    playerController.m_WalkSpeed = 5;
                    playerController.m_RunSpeed = 5;
                    if (playerController.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(playerController.GetComponent<ComponentMonitoring>(), "turnOff");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                        HelpSystem.traces.Enqueue(trace);
                    }
                }
                else
                { // standing and want to crouch
                    if (!firstCrouchOccurs)
                        firstCrouchOccurs = true;
                    playerController.m_WalkSpeed = 1;
                    playerController.m_RunSpeed = 1;
                    if (playerController.GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(playerController.GetComponent<ComponentMonitoring>(), "turnOn");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                        HelpSystem.traces.Enqueue(trace);
                    }
                }
            }

            //if the player is changing stance
            if (changingPose)
            {
                if (crouching)
                    targetScale = standingScale;
                else
                    targetScale = crouchingScale;

                playerController.transform.localScale = Vector3.MoveTowards(playerController.transform.localScale, targetScale, crouchingSpeed / 10); //change stance gradually

                if (playerController.transform.localScale == targetScale) //when standing scale is reached
                {
                    changingPose = false;
                    crouching = !crouching; //true when the player is crouching
                }

            }

            // make HUD transparent on moving
            hudHidingSpeed = -3f * Time.deltaTime;
            hudShowingSpeed = 3f * Time.deltaTime;
            if (hideHUD)
            {
                float aCount = 1;
                foreach (GameObject hud in linkedHud)
                {
                    tmpImage = hud.GetComponent<Image>();
                    tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, tmpImage.color.a + hudHidingSpeed);
                    aCount = tmpImage.color.a;
                }
                if (aCount < 0.3f)
                {
                    foreach (GameObject hud in linkedHud)
                    {
                        tmpImage = hud.GetComponent<Image>();
                        tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, 0.3f);
                    }
                    hideHUD = false;
                }
            }
            else if (showHUD)
            {
                float aCount = 0;
                foreach (GameObject hud in linkedHud)
                {
                    tmpImage = hud.GetComponent<Image>();
                    tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, tmpImage.color.a + hudShowingSpeed);
                    aCount = tmpImage.color.a;
                }
                if (aCount >= 1f)
                {
                    foreach (GameObject hud in linkedHud)
                    {
                        tmpImage = hud.GetComponent<Image>();
                        tmpImage.color = new Color(tmpImage.color.r, tmpImage.color.g, tmpImage.color.b, 1f);
                    }
                    showHUD = false;
                }
            }
            playerIsWalking = playerController.m_Input.x != 0 || playerController.m_Input.y != 0;
            if (playerIsWalking && !playerWasWalking)
            {
                hideHUD = true;
                showHUD = false;
            }
            else if (!playerIsWalking && playerWasWalking)
            {
                showHUD = true;
                hideHUD = false;
            }
            playerWasWalking = playerIsWalking;
        }
    }
}