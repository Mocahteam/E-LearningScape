using UnityEngine;
using FYFY;

public class IARNewHintAvailable : FSystem {

    private Family f_newHint = FamilyManager.getFamily(new AllOfComponents(typeof(NewHint)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_tabContent = FamilyManager.getFamily(new AnyOfTags("HelpTabContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_helpNotif = FamilyManager.getFamily(new AllOfComponents(typeof(HelpFlag)));

    private bool firstHelpOccurs = false;

    public static IARNewHintAvailable instance;

    public IARNewHintAvailable()
    {
        if (Application.isPlaying)
        {
            f_newHint.addEntryCallback(onNewHintAvailable);
            f_tabContent.addExitCallback(onExitHintPanel);
        }
        instance = this;
    }

    private void onNewHintAvailable(GameObject go)
    {
        if (!firstHelpOccurs)
        {
            GameObjectManager.setGameObjectState(f_helpNotif.First().transform.parent.gameObject, true);
            firstHelpOccurs = true;
        }
        GameObjectManager.setGameObjectState(f_helpNotif.First(), true);
    }

    private void onExitHintPanel(int InstanceId)
    {
        if (f_newHint.Count > 0)
            GameObjectManager.setGameObjectState(f_helpNotif.First(), true);
        else
            GameObjectManager.setGameObjectState(f_helpNotif.First(), false);
    }
}