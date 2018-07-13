using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;

public class Crouch : FSystem
{

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family hud = FamilyManager.getFamily(new AnyOfTags("HUDInputs"));
    private Family endRoom = FamilyManager.getFamily(new AnyOfTags("EndRoom"));

    private bool crouching = false;
    private bool changingPose = false;
    private float crouchingSpeed;
    private Vector3 crouchingScale;
    private Vector3 standingScale = Vector3.one;
    private bool firstCrouch = true;

    private bool playerIsWalking = false;
    private bool playerWasWalking = false;
    private bool showHUD;
    private bool hideHUD;
    private float hudShowingSpeed;
    private float hudHidingSpeed;

    public Crouch()
    {
        //when crouching, the scale of the player is changed (rather than its position)
        crouchingScale = Vector3.one * 0.2f;
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
        crouchingSpeed = 70f * Time.deltaTime;
        if (!Selectable.selected && !CollectableGO.onInventory)   //if nothing is selected and inventory isn't opened (the player can't move)
        {
            if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(1)) && !StoryDisplaying.reading && !DreamFragmentCollect.onFragment)    //when control button or right click is pressed
            {
                changingPose = true; //true when the player is crouching or standing
                //change moving speed according to the stance
                if (crouching)
                {
                    player.First().GetComponent<FirstPersonController>().m_WalkSpeed = 5;
                    player.First().GetComponent<FirstPersonController>().m_RunSpeed = 5;
                    if (player.First().GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(player.First().GetComponent<ComponentMonitoring>(), "turnOff");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                        HelpSystem.traces.Enqueue(trace);
                    }
                }
                else
                {
                    if (firstCrouch)
                    {
                        firstCrouch = false;
                        foreach (Transform child in hud.First().transform)
                        {
                            if(child.gameObject.name == "Crouch" || child.gameObject.name == "Move")
                            {
                                GameObjectManager.setGameObjectState(child.gameObject,true);
                            }
                        }
                    }
                    player.First().GetComponent<FirstPersonController>().m_WalkSpeed = 1;
                    player.First().GetComponent<FirstPersonController>().m_RunSpeed = 1;
                    if (player.First().GetComponent<ComponentMonitoring>() && HelpSystem.monitoring)
                    {
                        MonitoringTrace trace = new MonitoringTrace(player.First().GetComponent<ComponentMonitoring>(), "turnOn");
                        trace.result = MonitoringManager.trace(trace.component, trace.action, MonitoringManager.Source.PLAYER);
                        HelpSystem.traces.Enqueue(trace);
                    }
                }
            }
        }

        //if the player is changing stance
        if (changingPose)
        {
            if (crouching)
            {
                player.First().transform.localScale = Vector3.MoveTowards(player.First().transform.localScale, standingScale, crouchingSpeed/10); //change stance gradually
                if(player.First().transform.localScale == standingScale) //when standing scale is reached
                {
                    changingPose = false;
                    crouching = !crouching; //true when the player is crouching
                }
            }
            else
            {
                player.First().transform.localScale = Vector3.MoveTowards(player.First().transform.localScale, crouchingScale, crouchingSpeed/10); //change stance gradually
                if (player.First().transform.localScale == crouchingScale) //when crouching scale is reached
                {
                    changingPose = false;
                    crouching = !crouching; //true when the player is crouching
                }
            }
        }

        GameObjectManager.setGameObjectState(hud.First(),player.First().GetComponent<FirstPersonController>().enabled && !endRoom.First().activeSelf);
        hudHidingSpeed = -3f * Time.deltaTime;
        hudShowingSpeed = 3f * Time.deltaTime;
        if (hideHUD)
        {
            Color c;
            float aCount = 1;
            foreach(Transform child in hud.First().transform)
            {
                c = child.gameObject.GetComponent<Image>().color;
                child.gameObject.GetComponent<Image>().color = new Color(c.r, c.g, c.b, c.a + hudHidingSpeed);
                aCount = child.gameObject.GetComponent<Image>().color.a;
            }
            if(aCount < 0.3f)
            {
                foreach (Transform child in hud.First().transform)
                {
                    c = child.gameObject.GetComponent<Image>().color;
                    child.gameObject.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.3f);
                }
                hideHUD = false;
            }
        }
        else if (showHUD)
        {
            Color c;
            float aCount = 0;
            foreach (Transform child in hud.First().transform)
            {
                c = child.gameObject.GetComponent<Image>().color;
                child.gameObject.GetComponent<Image>().color = new Color(c.r, c.g, c.b, c.a + hudShowingSpeed);
                aCount = child.gameObject.GetComponent<Image>().color.a;
            }
            if (aCount >= 1f)
            {
                foreach (Transform child in hud.First().transform)
                {
                    c = child.gameObject.GetComponent<Image>().color;
                    child.gameObject.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1f);
                }
                showHUD = false;
            }
        }
        playerIsWalking = player.First().GetComponent<FirstPersonController>().m_Input.x != 0 || player.First().GetComponent<FirstPersonController>().m_Input.y != 0;
        if (playerIsWalking && !playerWasWalking)
        {
            hideHUD = true;
            showHUD = false;
        }
        else if(!playerIsWalking && playerWasWalking)
        {
            showHUD = true;
            hideHUD = false;
        }
        playerWasWalking = playerIsWalking;
    }
}