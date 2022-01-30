using UnityEngine;
using FYFY;

public class IARNewQuestionsAvailable : FSystem {

    private Family f_newQuestions = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_tabContent = FamilyManager.getFamily(new AnyOfTags("QuestionTagContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_terminalScreens = FamilyManager.getFamily(new AnyOfTags("TerminalScreen"));

    public GameObject questionNotif;
    public UnlockedRoom unlockedRoom;

    private bool firstQuestionOccurs = false;

    public static IARNewQuestionsAvailable instance;

    public IARNewQuestionsAvailable()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_newQuestions.addEntryCallback(onNewQuestionAvailable);
        f_tabContent.addEntryCallback(onQuestionsViewed);
    }

    private void onNewQuestionAvailable(GameObject go)
    {
        if(go.name.Contains("ScreenR"))
        {
            if (!firstQuestionOccurs)
            {
                GameObjectManager.setGameObjectState(questionNotif.transform.parent.gameObject, true);
                firstQuestionOccurs = true;
            }
            GameObjectManager.setGameObjectState(questionNotif, true);
        }
    }

    private void onQuestionsViewed(GameObject go)
    {
        if (go.name.EndsWith(unlockedRoom.roomNumber.ToString()))
        {
            GameObjectManager.setGameObjectState(questionNotif, false);
            // put a screenshot of the IAR on the terminal when the screen is viewed for the first time
            if (f_terminalScreens.Count >= unlockedRoom.roomNumber)
                MainLoop.instance.StartCoroutine(IARQueryEvaluator.instance.SetTerminalScreen());
        }
    }
}