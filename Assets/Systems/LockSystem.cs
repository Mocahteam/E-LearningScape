using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class LockSystem : FSystem { // TODO
    // All the locks
    private Family locks = FamilyManager.getFamily(new AllOfComponents(typeof(Lock)));

    // All the keys
    private Family keys = FamilyManager.getFamily(new AllOfComponents(typeof(Key)));

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
        foreach (GameObject go in locks)
        {
            Triggered3D t3d = go.GetComponent<Triggered3D>();
            Lock l = go.GetComponent<Lock>();
            if (!t3d) continue;
            foreach (GameObject target in t3d.Targets)
            {
                Key k = target.GetComponent<Key>();
                if (k && k.id == l.id) open(l);
                
            }
        }
    }

    private void open(Lock l)
    {
        // Make correct objects inactive
        foreach (GameObject g in l.disappearOnOpen)
        {
            g.SetActive(false);
        }

        // Make correct objects active
        foreach (GameObject g in l.appearOnOpen)
        {
            g.SetActive(true);
        }
    }
}
