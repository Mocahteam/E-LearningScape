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
        if (Application.isPlaying)
        {
            f_newFragment.addEntryCallback(onNewFragmentAvailable);
            f_tabContent.addExitCallback(onExitDreamFragmentsPanel);
        }
        instance = this;
    }

    private void onNewFragmentAvailable(GameObject go)
    {
        if (!firstFragmentOccurs)
        {
            GameObjectManager.setGameObjectState(f_fragmentNotif.First().transform.parent.gameObject, LoadGameContent.gameContent.virtualDreamFragment);
            firstFragmentOccurs = true;
        }
        GameObjectManager.setGameObjectState(f_fragmentNotif.First(), true);
    }

    private void onExitDreamFragmentsPanel(int InstanceId)
    {
        if (f_newFragment.Count > 0)
            GameObjectManager.setGameObjectState(f_fragmentNotif.First(), true);
        else
            GameObjectManager.setGameObjectState(f_fragmentNotif.First(), false);
    }
}