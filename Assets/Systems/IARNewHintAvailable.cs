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
        if (f_helpWarning.Count <= 0)
            this.Pause = true;
        else
        {
            if (f_newHint.Count > 0 && HUD_neverDisplayed && f_iarBackground.Count <= 0)
            {
                // enable parent
                GameObjectManager.setGameObjectState(f_helpWarning.First().transform.parent.gameObject, true);
                HUD_neverDisplayed = false;
            }

            if (f_newHint.Count > 0 && helpWarning.transform.parent.gameObject.activeSelf)
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
            else if (helpWarning.activeSelf)
                GameObjectManager.setGameObjectState(helpWarning, false);
        }
	}
}