using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using FYFY_plugins.TriggerManager;

public class TutorialManager : FSystem {
    private Family f_targetArea = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(Rigidbody)));
    private Family f_itemsEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_inventoryTabContent = FamilyManager.getFamily(new AnyOfTags("InventoryTabContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedInInventory), typeof(Collected), typeof(AnimatedSprites)), new AnyOfTags("InventoryElements"));
    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_newFragment = FamilyManager.getFamily(new AllOfComponents(typeof(NewDreamFragment)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_dreamTabContent = FamilyManager.getFamily(new AnyOfTags("DreamFragmentsTabContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_selectableDreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragmentToggle), typeof(Toggle)));
    private Family f_questionTabContent = FamilyManager.getFamily(new AnyOfTags("QuestionTagContent"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_answerQuestion = FamilyManager.getFamily(new AnyOfTags("A-R1"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));

    public GameObject movingModeSelector;

    private FirstPersonController playerController;
    private GameObject MovingMode;
    private Transform TutorialScreens;
    private float initWalkSpeed;
    private Vector3 initPosition;

    private string currentStepName;
    private int currentStep = 0;
    private float rotationProgress = 0;
    private float previousRotation = 0;
    private float movingProgress = 0;
    private Vector3 previousPosition;

    public static TutorialManager instance;

    public TutorialManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        TutorialScreens = GameObject.Find("TutorialScreens").transform;
        playerController = GameObject.Find("FPSController").GetComponent<FirstPersonController>();
        MovingMode = GameObject.Find("MovingMode");
        previousRotation = playerController.transform.rotation.y;
        initWalkSpeed = playerController.m_WalkSpeed;
        initPosition = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z);
        GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).gameObject, true);
        currentStepName = TutorialScreens.GetChild(currentStep).gameObject.name;
        // Toggle linked GO
        foreach (LinkedWith lw in TutorialScreens.GetChild(currentStep).GetComponents<LinkedWith>())
            GameObjectManager.setGameObjectState(lw.link, !lw.link.activeSelf);
        f_targetArea.addEntryCallback(onTargetReached);
        IARDreamFragmentManager.virtualDreamFragment = true; // force virtual dreamFragment for tutorial
    }

    public void nextStep()
    {
        GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).gameObject, false);
        currentStep++;
        currentStepName = TutorialScreens.GetChild(currentStep).gameObject.name;
        if (currentStepName == "StepObserve")
        {
            MovingSystem_FPSMode.instance.Pause = false;
            MoveInFrontOf.instance.Pause = false;
            IARTabNavigation.instance.Pause = false;
            IARNewItemAvailable.instance.Pause = false;
            UIEffectPlayer.instance.Pause = false;
            Highlighter.instance.Pause = false;
            CollectObject.instance.Pause = false;
            DreamFragmentCollecting.instance.Pause = false;
            SpritesAnimator.instance.Pause = false;
            playerController.m_WalkSpeed = 0;
        } else if (currentStepName == "StepMove" || currentStepName == "StepExplore")
            playerController.m_WalkSpeed = initWalkSpeed;
        else if (currentStepName == "StepEnd")
        {
            MovingMode.SetActive(false); // do not use FYFY to avoid delay callbacks
            MovingSystem_FPSMode.instance.Pause = true;
            MovingSystem_UIMode.instance.Pause = true;
            MoveInFrontOf.instance.Pause = true;
            IARTabNavigation.instance.Pause = true;
            IARNewItemAvailable.instance.Pause = true;
            UIEffectPlayer.instance.Pause = true;
            Highlighter.instance.Pause = true;
            CollectObject.instance.Pause = true;
            DreamFragmentCollecting.instance.Pause = true;
            MovingSystem_TeleportMode.instance.Pause = true;
        }
        GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).gameObject, true);
        // Toggle linked GO
        foreach (LinkedWith lw in TutorialScreens.GetChild(currentStep).GetComponents<LinkedWith>())
            GameObjectManager.setGameObjectState(lw.link, !lw.link.activeSelf);
    }

    private void onTargetReached(GameObject go)
    {
        GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
        nextStep();
        GameObjectManager.setGameObjectState(go, false);
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (currentStepName == "StepObserve")
        {
            rotationProgress += Mathf.Abs(playerController.transform.rotation.y - previousRotation);
            previousRotation = playerController.transform.rotation.y;
            if (rotationProgress > 1)
            {
                GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
                nextStep();
            }
        } else if (currentStepName == "StepCrouch" && playerController.transform.localScale != Vector3.one)
        {
            GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
            nextStep();
        }
        else if (currentStepName == "StepStandUp" && playerController.transform.localScale == Vector3.one)
        {
            GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
            nextStep();
            previousPosition = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z);
            playerController.m_WalkSpeed = 0; // To force using teleport on next step
            playerController.transform.position = initPosition;
        }
        else if (currentStepName == "StepWaitK" && Input.GetButtonDown("ToggleTarget"))
        {
            GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
            GameObjectManager.setGameObjectState(movingModeSelector, false);
            if (TutorialScreens.GetChild(currentStep+1).gameObject.name == "StepMoveUI")
                playerController.transform.position = initPosition; //reset position to start position
            nextStep();
        }
        else if (currentStepName == "StepExplore")
        {
            movingProgress += Vector3.Distance(previousPosition, playerController.transform.position);
            previousPosition = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z);
            if (movingProgress > 20)
            {
                GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
                nextStep();
            }
        }
        else if (currentStepName == "StepGetScroll" && f_itemsEnabled.Count > 0) // Wait one item available inside IAR
        {
            nextStep();
        }
        else if (currentStepName == "StepPressY" && f_inventoryTabContent.Count > 0) // Wait Inventory panel opened
        {
            nextStep();
        }
        else if (currentStepName == "StepSelectScrollIAR" && f_itemSelected.Count > 0) // Wait item selected
        {
            nextStep();
        }
        else if (currentStepName == "StepWaitIARClose" && f_tabs.Count == 0) // Wait IAR closed
        {
            nextStep();
        }
        else if (currentStepName == "StepGetDreamFragment" && f_newFragment.Count > 0) // Wait one fragment available inside IAR
        {
            nextStep();
        }
        else if (currentStepName == "StepPressR" && f_dreamTabContent.Count > 0) // Wait dream fragment panel opened
        {
            nextStep();
        }
        else if (currentStepName == "StepSelectDreamFragmentIAR") // Wait dream fragment selected
        {
            foreach (GameObject df in f_selectableDreamFragments)
            {
                if (df.GetComponent<Toggle>().isOn)
                    nextStep();
            }
        }
        else if (currentStepName == "StepWaitAnswerQuestion" && f_answerQuestion.Count > 0)
        {
            nextStep();
        }
        else if (currentStepName == "StepPressB" && f_questionTabContent.Count > 0) // Wait Question panel opened
        {
            nextStep();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        GameObjectManager.loadScene("E-LearningScape");
    }
}