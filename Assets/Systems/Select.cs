using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class Select : FSystem {
    // Both Vive controllers (they also have TriggerSensitive3D)
    private Family controllers = FamilyManager.getFamily(new AllOfComponents(typeof(ViveController), typeof(Triggered3D)));
    //all selectable objects
    private Family objects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)));
    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Takable)));

    private GameObject focused;
    private bool selected = false;

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

        foreach(GameObject go in controllers)
        {
            Triggered3D t3d = go.GetComponent<Triggered3D>();
            foreach(GameObject target in t3d.Targets)
            {
                // Select only the first Gameobject
                // TODO
                return;
            }

        }
    }
}
