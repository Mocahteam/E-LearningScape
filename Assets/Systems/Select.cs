using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class Select : FSystem { // TODO
    // Both Vive controllers (they also have TriggerSensitive3D)
    private Family controllers = FamilyManager.getFamily(new AllOfComponents(typeof(Grabber)));

    // All the selectable objects
    private Family selectables = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)));

    private GameObject focused;
    private bool selected = false;

    public Select()
    {
        // For each controller
        foreach (GameObject c in controllers)
        {
            Grabber g = c.GetComponent<Grabber>();
            // Get the tracked object (device)
            g.trackedObj = g.GetComponent<SteamVR_TrackedObject>();
        }
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
        // Reinit every selectable
        foreach(GameObject go in selectables)
        {
            Selectable s = go.GetComponent<Selectable>();
            s.focused = false;
        }

        // For each controller
        foreach(GameObject go in controllers)
        {
            Grabber g = go.GetComponent<Grabber>();
            g.collidingObject = null;
            Triggered3D t3d = go.GetComponent<Triggered3D>();
            if (!t3d) continue;
            foreach(GameObject target in t3d.Targets)
            {
                Selectable s = target.GetComponent<Selectable>();
                if (!s) continue;
                s.focused = true;
                g.collidingObject = target;
                break; // Only the first
            }

        }

        // For each selectable element
        foreach(GameObject go in selectables)
        {
            Selectable s = go.GetComponent<Selectable>();
            if (Input.GetMouseButtonDown(0) && !Takable.objectTaken)
            {
                 s.GetComponent<Selectable>().isSelected = s.focused;
                 //Selectable.selected = true;
            }
            if (!((go.tag == "Plank" || go.tag == "Box")))
            {
                foreach (Transform child in go.transform)
                {
                    if (child.gameObject.tag == "MouseOver")
                    {
                        child.gameObject.SetActive(s.focused);
                    }
                }
            }
        }
    }
}
