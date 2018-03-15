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
        if (!Selectable.selected)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                changingPose = true;
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
        if (changingPose)
        {
            if (crouching)
            {
                player.First().transform.localScale = Vector3.MoveTowards(player.First().transform.localScale, standingScale, crouchingSpeed/10);
                if(player.First().transform.localScale == standingScale)
                {
                    changingPose = false;
                    crouching = !crouching;
                }
            }
            else
            {
                player.First().transform.localScale = Vector3.MoveTowards(player.First().transform.localScale, crouchingScale, crouchingSpeed/10);
                if (player.First().transform.localScale == crouchingScale)
                {
                    changingPose = false;
                    crouching = !crouching;
                }
            }
        }
	}
}