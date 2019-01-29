using UnityEngine;
using FYFY;

public class IARNewHintAvailable : FSystem {

    private Family f_newHint = FamilyManager.getFamily(new AllOfComponents(typeof(NewHint)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_helpWarning = FamilyManager.getFamily(new AnyOfTags("HelpWarning"));

    private GameObject helpWarning;

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