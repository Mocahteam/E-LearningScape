using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;

public class ToggleObject : FSystem {

    // This system enables to manage in game toggleable objects

	private Family f_toggleable = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleableGO), typeof(Highlighted), typeof(Animator))); // Highlighted is dynamically added by Highlither system

    //temporary variables
    private GameObject tmpGO;
    private ToggleableGO tmpToggleableGO;

    public static ToggleObject instance;

    public ToggleObject(){
        if (Application.isPlaying)
        {
            instance = this;
        }
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (Input.GetButtonDown("Fire1"))
        {
            int nbToggleable = f_toggleable.Count;
            for (int i = 0; i < nbToggleable; i++)
            {
                tmpGO = f_toggleable.getAt(i);
                tmpToggleableGO = tmpGO.GetComponent<ToggleableGO>();
                Animator anim = tmpGO.GetComponent<Animator>();

                tmpToggleableGO.toggled = !tmpToggleableGO.toggled;
                if (tmpToggleableGO.toggled)
                {
                    anim.SetTrigger("turnOn");
                }
                else
                {
                    anim.SetTrigger("turnOff");
                }
            }
        }
    }
}