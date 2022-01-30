using UnityEngine;
using FYFY;

public class IARNewDreamFragmentAvailable : FSystem
{

    private Family f_newFragment = FamilyManager.getFamily(new AllOfComponents(typeof(NewDreamFragment)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_tabContent = FamilyManager.getFamily(new AnyOfTags("DreamFragmentsTabContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_fragmentNotif = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragmentFlag)));

    public bool firstFragmentOccurs = false;

    public static IARNewDreamFragmentAvailable instance;

    public IARNewDreamFragmentAvailable()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_newFragment.addEntryCallback(onNewFragmentAvailable);
        f_tabContent.addExitCallback(onExitDreamFragmentsPanel);
        f_tabContent.addEntryCallback(onEnterDreamFragmentsPanel);
    }

    private void onNewFragmentAvailable(GameObject go)
    {
        if (!firstFragmentOccurs)
        {
            foreach (GameObject notif in f_fragmentNotif)
                GameObjectManager.setGameObjectState(notif.transform.parent.gameObject, IARDreamFragmentManager.virtualDreamFragment);
            firstFragmentOccurs = true;
        }
        foreach(GameObject notif in f_fragmentNotif)
            GameObjectManager.setGameObjectState(notif, true);
    }

    private void onEnterDreamFragmentsPanel(GameObject unused)
    {
        foreach (GameObject notif in f_fragmentNotif)
            GameObjectManager.setGameObjectState(notif, false);
    }
    private void onExitDreamFragmentsPanel(int unused)
    {
        if (f_newFragment.Count > 0)
            foreach (GameObject notif in f_fragmentNotif)
                GameObjectManager.setGameObjectState(notif, true);
        else
            foreach (GameObject notif in f_fragmentNotif)
                GameObjectManager.setGameObjectState(notif, false);
    }
}