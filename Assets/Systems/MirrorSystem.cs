using UnityEngine;
using FYFY;

public class MirrorSystem : FSystem {

    // This system manage cylinder mirrors

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_mirrors = FamilyManager.getFamily(new AllOfComponents(typeof(MirrorScript)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_selectedPlank = FamilyManager.getFamily(new AnyOfTags("PlankE09"), new AllOfComponents(typeof(ReadyToWork)));

    private bool plankSelected = false;

    public static MirrorSystem instance;

    public MirrorSystem()
    {
        if (Application.isPlaying)
        {
            f_selectedPlank.addEntryCallback(onPlankSelected);
            f_selectedPlank.addExitCallback(onPlankUnselected);
        }
        
        instance = this;
    }

    private void onPlankSelected (GameObject go)
    {
        plankSelected = true;
    }

    private void onPlankUnselected (int instanceID)
    {
        plankSelected = false;
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
        //always rotate the mirror gameobject in the direction of the player
        foreach (GameObject mirror in f_mirrors)
        {
            if (plankSelected)
                mirror.transform.Rotate(Vector3.up, 0f);
            else
                mirror.transform.rotation = Quaternion.Euler(0, f_player.First().transform.rotation.eulerAngles.y - Vector3.SignedAngle(mirror.transform.position - f_player.First().transform.position - Vector3.up * (mirror.transform.position.y - f_player.First().transform.position.y), f_player.First().transform.forward, Vector3.up), 0);
        }
    }
}