using UnityEngine;
using FYFY;

public class IARNewHintAvailable : FSystem {

    private Family f_newHint = FamilyManager.getFamily(new AllOfComponents(typeof(NewHint)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_helpWarning = FamilyManager.getFamily(new AnyOfTags("HelpWarning"));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private GameObject helpWarning;
    private bool HUD_neverDisplayed = true;

    public static IARNewHintAvailable instance;

    public IARNewHintAvailable()
    {
        if (Application.isPlaying)
        {
            helpWarning = f_helpWarning.First();
        }
        instance = this;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // f_helpWarning will be empty if monitoring module is not enabled in config file => then pause this system
        if (f_helpWarning.Count <= 0)
            this.Pause = true;
        else
        {
            if (f_newHint.Count > 0)
            {
                if (HUD_neverDisplayed && f_iarBackground.Count <= 0)
                {
                    // enable parent
                    GameObjectManager.setGameObjectState(f_helpWarning.First().transform.parent.gameObject, true);
                    HUD_neverDisplayed = false;
                }

                if (helpWarning.transform.parent.gameObject.activeSelf)
                {
                    if (Time.time - (int)Time.time > 0.5f && !helpWarning.activeSelf)
                    {
                        // display warning
                        GameObjectManager.setGameObjectState(helpWarning, true);
                    }
                    else if (Time.time - (int)Time.time < 0.5f && helpWarning.activeSelf)
                    {
                        // disable warning
                        GameObjectManager.setGameObjectState(helpWarning, false);
                    }
                }
                f_helpWarning.First().transform.parent.gameObject.GetComponent<Animator>().enabled = true;
            }

            else
            {
                GameObject warn = f_helpWarning.First();
                GameObject HUD_H = warn.transform.parent.gameObject;
                if (helpWarning.activeSelf)
                {
                    GameObjectManager.setGameObjectState(helpWarning, false);
                    HUD_H.transform.position = new Vector3(HUD_H.transform.position.x, 50.0f, HUD_H.transform.position.z);
                }
                    
                HUD_H.GetComponent<Animator>().enabled = false;

            }


            //f_helpWarning.First().transform.parent.gameObject.GetComponent<Animator>().enabled = true;
        }
	}
}