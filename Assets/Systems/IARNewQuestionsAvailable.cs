using UnityEngine;
using FYFY;

public class IARNewQuestionsAvailable : FSystem {

    private Family f_newQuestions = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_tabContent = FamilyManager.getFamily(new AnyOfTags("QuestionTagContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_questionNotif = FamilyManager.getFamily(new AllOfComponents(typeof(QuestionFlag)));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    private bool firstQuestionOccurs = false;

    public static IARNewQuestionsAvailable instance;

    public IARNewQuestionsAvailable()
    {
        if (Application.isPlaying)
        {
            f_newQuestions.addEntryCallback(onNewQuestionAvailable);
            f_tabContent.addEntryCallback(onQuestionsViewed);
        }
        instance = this;
    }

    private void onNewQuestionAvailable(GameObject go)
    {
        if (!firstQuestionOccurs)
        {
            GameObjectManager.setGameObjectState(f_questionNotif.First().transform.parent.gameObject, true);
            firstQuestionOccurs = true;
        }
        GameObjectManager.setGameObjectState(f_questionNotif.First(), true);
    }

    private void onQuestionsViewed(GameObject go)
    {
        Debug.Log(go.name + " " + f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber.ToString());
        if (go.name.EndsWith(f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber.ToString()))
            GameObjectManager.setGameObjectState(f_questionNotif.First(), false);
    }
}