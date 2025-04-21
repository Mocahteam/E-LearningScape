using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DreamFragmentCollecting : FSystem {

    // Display Fragment UI when player select a fragment in game
    // This system has to be after "CollectObject" system in main loop

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment), typeof(PointerOver)));

    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));

    public GameObject dfUI;
    private TextMeshProUGUI FragmentText;
    private DreamFragment tmpDFComponent;
    private bool backupIARNavigationState;
    private bool backupHighlighterState;
    //button in dream fragment UI to open the link
    public GameObject onlineButton;
    public bool firstFragmentFound; // defined in inspector

    public GameObject itemCollectedNotif;
    public MovingModeSelector movingModeSelector;
    public GameObject player;

    public static DreamFragmentCollecting instance;

    public DreamFragmentCollecting()
    {
        instance = this;
    }

    protected override void onStart()
    {
        // Get child text area
        FragmentText = dfUI.GetComponentInChildren<TextMeshProUGUI>();

        f_dreamFragments.addEntryCallback(onDreamFragmentFocused);
    }

    private void onDreamFragmentFocused(GameObject df)
    {
        GameObjectManager.addComponent<PlaySound>(df, new { id = 2 }); // id refer to FPSController AudioBank
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // Compute Raycast only when mouse is clicked
        if (Input.GetButtonDown("Fire1"))
        {
            foreach (GameObject selectedFragment in f_dreamFragments)
            {
                // display IAR dream fragments tab after the first one is found
                if (!firstFragmentFound && IARDreamFragmentManager.virtualDreamFragment)
                {
                    foreach (GameObject go in f_tabs)
                    {
                        if(go.name == "DreamFragments")
                        {
                            GameObjectManager.setGameObjectState(go, true);
                            break;
                        }
                    }
                    firstFragmentFound = true;
                }

                tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                if (!IARDreamFragmentManager.virtualDreamFragment || tmpDFComponent.tag == "Puzzle" || tmpDFComponent.type != 0)
                {
                    // Show fragment UI
                    GameObjectManager.setGameObjectState(dfUI, true);
                    if(tmpDFComponent.urlLink != null && tmpDFComponent.urlLink != "")
                    {
                        onlineButton.GetComponentInChildren<TextMeshProUGUI>().text = tmpDFComponent.linkButtonText;
                        GameObjectManager.setGameObjectState(onlineButton, true);
                        // set callback
                        onlineButton.GetComponent<Button>().onClick.RemoveAllListeners();
                        onlineButton.GetComponent<Button>().onClick.AddListener(delegate { OpenFragmentLink(selectedFragment); });
                    }
                    else
                        GameObjectManager.setGameObjectState(onlineButton, false);
                    // Set UI text depending on type and id
                    if (tmpDFComponent.type == 0)
                        FragmentText.text = string.Concat(LoadGameContent.internalGameContent.dreamFragmentText, tmpDFComponent.id);
                    else if (tmpDFComponent.type == 1 || tmpDFComponent.type == 2)
                        FragmentText.text = tmpDFComponent.itemName;
                    // Pause this system and dependant systems
                    this.Pause = true;
                    movingModeSelector.pauseMovingSystems();
                    backupIARNavigationState = IARTabNavigation.instance.Pause;
                    IARTabNavigation.instance.Pause = true;
                    backupHighlighterState = Highlighter.instance.Pause;
                    Highlighter.instance.Pause = true;
                    if (tmpDFComponent.type == 1)
                        GameObjectManager.addComponent<ForceCollect>(selectedFragment);
                }
                else
                {
                    // enable UI target
                    GameObjectManager.setGameObjectState(selectedFragment.GetComponent<LinkedWith>().link, true);
                    itemCollectedNotif.GetComponent<Animator>().SetTrigger("Start2");
                }

                TurnOffDreamFragment(selectedFragment);

                GameObjectManager.addComponent<ActionPerformedForLRS>(selectedFragment, new
                {
                    verb = "collected",
                    objectType = "dreamFragment",
                    activityExtensions = new Dictionary<string, string>() {
                        { "value", selectedFragment.name }
                    }
                });

                GameObjectManager.addComponent<PlaySound>(selectedFragment, new { id = 3 }); // id refer to FPSController AudioBank

                if (selectedFragment.GetComponentInParent<IsSolution>())
                {
                    if(player.transform.localScale.x < 0.9f)
                        //if the player is crouching
                        GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player", orLabels = new string[] { "crouch"} });
                    else
                        GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player", orLabels = new string[] { "chairDown" } });

                }
                else if (tmpDFComponent.type != 2)
                    GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player" });
            }
        }
    }

    public void CloseFragmentUI()
    {
        // close UI
        GameObjectManager.setGameObjectState(dfUI,false);
        // Unpause this system and dependants systems
        this.Pause = false;
        movingModeSelector.resumeMovingSystems();
        IARTabNavigation.instance.Pause = backupIARNavigationState;
        Highlighter.instance.Pause = backupHighlighterState;
    }

    public void TurnOffDreamFragment(GameObject fragment)
    {
        tmpDFComponent = fragment.GetComponent<DreamFragment>();
        if (fragment && tmpDFComponent.type != 2)
        {
            // disable particles
            if (fragment.GetComponentInChildren<ParticleSystem>())
                GameObjectManager.setGameObjectState(fragment.GetComponentInChildren<ParticleSystem>().gameObject, false);
            // disable glowing
            foreach (Transform child in fragment.transform)
            {
                if (child.gameObject.tag == "DreamFragmentLight")
                {
                    child.gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                    break;
                }
            }
            tmpDFComponent.viewed = true;
            Animator anim;
            if (fragment.TryGetComponent<Animator>(out anim))
                anim.enabled = false;
        }
    }

    public void OpenFragmentLink(GameObject selectedFragment)
    {
        DreamFragment df = selectedFragment.GetComponent<DreamFragment>();
        //when onlineButton is clicked
        try
        {
            Application.OpenURL(df.urlLink);
        }
        catch (Exception)
        {
            Debug.LogError(string.Concat("Invalid dream fragment link: ", df.urlLink));
        }
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedFragment, new
        {
            verb = "accessed",
            objectType = "link",
            activityExtensions = new Dictionary<string, string>() {
                { "type", "dreamFragment" },
                { "value", selectedFragment.name },
                { "link", df.urlLink }
            }
        });
    }
}