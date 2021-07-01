using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    private FirstPersonController playerController;
    private Transform TutorialScreens;
    private float initWalkSpeed;
    private Vector3 initPosition;
    private GameObject pinTarget;

    private int currentStep = 0;
    private float rotationProgress = 0;
    private float previousRotation = 0;
    private float movingProgress = 0;
    private Vector3 previousPosition;

    public TutorialManager()
    {
        if (Application.isPlaying)
        {
            TutorialScreens = GameObject.Find("TutorialScreens").transform;
            playerController = GameObject.Find("FPSController").GetComponent<FirstPersonController>();
            pinTarget = GameObject.Find("PinTarget");
            previousRotation = playerController.transform.rotation.y;
            initWalkSpeed = playerController.m_WalkSpeed;
            initPosition = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z);
            GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).gameObject, true);
            f_targetArea.addEntryCallback(onTargetReached);
        }
    }

    public void nextStep()
    {
        GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).gameObject, false);
        currentStep++;
        if (currentStep == 1)
        {
            MovingSystem.instance.Pause = false;
            MoveInFrontOf.instance.Pause = false;
            IARTabNavigation.instance.Pause = false;
            IARNewItemAvailable.instance.Pause = false;
            UIEffectPlayer.instance.Pause = false;
            Highlighter.instance.Pause = false;
            CollectObject.instance.Pause = false;
            DreamFragmentCollecting.instance.Pause = false;
            SpritesAnimator.instance.Pause = false;
            JumpingSystem.instance.Pause = false;
            playerController.m_WalkSpeed = 0;
        } else if (currentStep == 2 || currentStep == 8)
            playerController.m_WalkSpeed = initWalkSpeed;
        else if (currentStep == 18)
        {
            MovingSystem.instance.Pause = true;
            MoveInFrontOf.instance.Pause = true;
            IARTabNavigation.instance.Pause = true;
            IARNewItemAvailable.instance.Pause = true;
            UIEffectPlayer.instance.Pause = true;
            Highlighter.instance.Pause = true;
            CollectObject.instance.Pause = true;
            DreamFragmentCollecting.instance.Pause = true;
            SpritesAnimator.instance.Pause = true;
            JumpingSystem.instance.Pause = true;
        }
        GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).gameObject, true);
        if (TutorialScreens.GetChild(currentStep).GetComponent<LinkedWith>() != null)
            GameObjectManager.setGameObjectState(TutorialScreens.GetChild(currentStep).GetComponent<LinkedWith>().link, true);
    }

    private void onTargetReached(GameObject go)
    {
        GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
        nextStep();
        GameObjectManager.unbind(go);
        GameObject.Destroy(go);
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (currentStep == 1)
        {
            rotationProgress += Mathf.Abs(playerController.transform.rotation.y - previousRotation);
            previousRotation = playerController.transform.rotation.y;
            if (rotationProgress > 1)
            {
                GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
                nextStep();
            }
        } else if (currentStep == 3 && playerController.transform.localScale != Vector3.one)
        {
            GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
            nextStep();
        }
        else if (currentStep == 4 && playerController.transform.localScale == Vector3.one)
        {
            GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
            nextStep();
            previousPosition = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z);
        }
        else if (currentStep == 5)
        {
            movingProgress += Vector3.Distance(previousPosition, playerController.transform.position);
            previousPosition = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z);
            if (movingProgress > 20)
            {
                GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
                nextStep();
                playerController.m_WalkSpeed = 0; // To force using teleport on next step
                playerController.transform.position = initPosition;
            }
        }
        else if (currentStep == 6 && pinTarget.activeInHierarchy) // Wait pinTarget active in hierarchy
        {
            GameObjectManager.addComponent<PlayUIEffect>(playerController.gameObject, new { effectCode = 2 });
            nextStep();
        }
        else if (currentStep == 8 && f_itemsEnabled.Count > 0) // Wait one item available inside IAR
        {
            nextStep();
        }
        else if (currentStep == 9 && f_inventoryTabContent.Count > 0) // Wait Inventory panel opened
        {
            nextStep();
        }
        else if (currentStep == 10 && f_itemSelected.Count > 0) // Wait item selected
        {
            nextStep();
        }
        else if (currentStep == 11 && f_tabs.Count == 0) // Wait IAR closed
        {
            nextStep();
        }
        else if (currentStep == 12 && f_newFragment.Count > 0) // Wait one fragment available inside IAR
        {
            nextStep();
        }
        else if (currentStep == 13 && f_dreamTabContent.Count > 0) // Wait dream fragment panel opened
        {
            nextStep();
        }
        else if (currentStep == 14) // Wait dream fragment selected
        {
            foreach (GameObject df in f_selectableDreamFragments)
            {
                if (df.GetComponent<Toggle>().isOn)
                    nextStep();
            }
        }
        else if (currentStep == 15 && f_tabs.Count == 0) // Wait IAR closed
        {
            nextStep();
        }
        else if (currentStep == 16 && f_questionTabContent.Count > 0) // Wait Question panel opened
        {
            nextStep();
        }
        else if (currentStep == 17 && f_tabs.Count == 0) // Wait IAR closed
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