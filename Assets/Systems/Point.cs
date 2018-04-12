using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class Point : FSystem { // TODO
    // Both Vive controllers (they also have TriggerSensitive3D)
    private Family controllers = FamilyManager.getFamily(new AllOfComponents(typeof(LaserPointer)));

    // All the selectable objects
    private Family pointables = FamilyManager.getFamily(new AllOfComponents(typeof(Pointable)));

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
        // Reinit every selectable
        foreach(GameObject go in pointables)
        {
            Pointable p = go.GetComponent<Pointable>();
            p.focused = false;
        }

        // For each controller
        foreach(GameObject go in controllers)
        {
            
            LaserPointer lp = go.GetComponent<LaserPointer>();
            RaycastHit hit;
            if (Physics.Raycast(lp.trackedObj.transform.position, lp.transform.forward, out hit, 100, lp.pointMask))
            {
                GameObject pointed = hit.collider.gameObject;
                Pointable p = pointed.GetComponent<Pointable>();
                if (!p) continue;
                p.focused = true;

                SteamVR_Controller.Device controller = SteamVR_Controller.Input((int)lp.trackedObj.index);
                // If trigger is pressed, toggle selection
                if (controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) p.selected = !p.selected;
            }
        }

        // For each focused pointable element
        foreach (GameObject go in pointables)
        {
            Pointable p = go.GetComponent<Pointable>();
            if(p.focused || p.selected) p.mouseOver.SetActive(true);
            else p.mouseOver.SetActive(false);
        }
    }
}
