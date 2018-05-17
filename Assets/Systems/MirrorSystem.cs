using UnityEngine;
using FYFY;

public class MirrorSystem : FSystem {

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family mirror = FamilyManager.getFamily(new AllOfComponents(typeof(MirrorScript)));

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
        //always rotate the mirror gameobject in the direction of the player
        mirror.First().transform.rotation = Quaternion.Euler(0, player.First().transform.rotation.eulerAngles.y - Vector3.SignedAngle(mirror.First().transform.position - player.First().transform.position - Vector3.up * (mirror.First().transform.position.y - player.First().transform.position.y), player.First().transform.forward, Vector3.up),0);
	}
}