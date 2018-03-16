using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;

public class Crouch : FSystem
{

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private bool crouching = false;
    private bool changingPose = false;
    private float crouchingSpeed = 1f;
    private Vector3 crouchingScale;
    private Vector3 standingScale = Vector3.one;

    public Crouch()
    {
        //when crouching, the scale of the player is changed (rather than its position)
        crouchingScale = new Vector3(0.2f, 0.2f, 0.2f);
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
        if (!Selectable.selected)   //if nothing is selected (the player can't move when an object is selected)
        {
            if (Input.GetKeyDown(KeyCode.E))    //when "e" is pressed
            {
                changingPose = true; //true when the player is crouching or standing
                //change moving speed according to the stance
                if (crouching)
                {
                    player.First().GetComponent<FirstPersonController>().m_WalkSpeed = 5;
                    player.First().GetComponent<FirstPersonController>().m_RunSpeed = 5;
                }
                else
                {
                    player.First().GetComponent<FirstPersonController>().m_WalkSpeed = 1;
                    player.First().GetComponent<FirstPersonController>().m_RunSpeed = 1;
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
	}
}