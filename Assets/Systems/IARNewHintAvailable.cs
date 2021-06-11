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
            f_tabContent.addEntryCallback(onEnterHintPanel);
            f_tabContent.addExitCallback(onExitHintPanel);
        }
        instance = this;
    }

    private void onNewHintAvailable(GameObject go)
    {
        if (!firstHelpOccurs)
        {
            foreach (GameObject notif in f_helpNotif)
                GameObjectManager.setGameObjectState(notif.transform.parent.gameObject, true);
            firstHelpOccurs = true;
        }
        if (f_tabContent.Count == 0)
            foreach (GameObject notif in f_helpNotif)
                GameObjectManager.setGameObjectState(notif, true);
    }

    private void onEnterHintPanel(GameObject unused)
    {
        foreach (GameObject notif in f_helpNotif)
            GameObjectManager.setGameObjectState(notif, false);
    }

    private void onExitHintPanel(int InstanceId)
    {
        if (f_newHint.Count > 0)
            foreach (GameObject notif in f_helpNotif)
                GameObjectManager.setGameObjectState(notif, true);
        else
            foreach (GameObject notif in f_helpNotif)
                GameObjectManager.setGameObjectState(notif, false);
    }
}