using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.Collections.Generic;

public class DreamFragmentCollecting : FSystem {

    // Display Fragment UI when player select a fragment in game
    // This system has to be after "CollectObject" system in main loop

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment), typeof(PointerOver)));
    private Family f_dreamFragmentUI = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AnyOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD));
    private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));
    private Family f_HUD = FamilyManager.getFamily(new AnyOfTags("HUD_Main"));

    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));

    private Family f_movingModeSelector = FamilyManager.getFamily(new AllOfComponents(typeof(MovingModeSelector)));

    private GameObject dfUI;
    private TextMeshProUGUI FragmentText;
    private DreamFragment tmpDFComponent;
    private bool backupIARNavigationState;
    private bool backupHighlighterState;
    //button in dream fragment UI to open the link
    private GameObject onlineButton;
    public bool firstFragmentFound = false;

    private GameObject itemCollectedNotif;

    public static DreamFragmentCollecting instance;

    public DreamFragmentCollecting()
    {
        if (Application.isPlaying)
        {
            dfUI = f_dreamFragmentUI.First();
            // Add listener on child button to close UI
            foreach(Button b in dfUI.GetComponentsInChildren<Button>())
            {
                if (b.gameObject.name == "ButtonOnline")
                    onlineButton = b.gameObject;
            }
            // Get child text area
            FragmentText = dfUI.GetComponentInChildren<TextMeshProUGUI>();

            itemCollectedNotif = f_HUD.First().transform.GetChild(f_HUD.First().transform.childCount - 1).gameObject;

            f_dreamFragments.addEntryCallback(onDreamFragmentFocused);
        }
        instance = this;
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
                if (!IARDreamFragmentManager.virtualDreamFragment)
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
                        FragmentText.text = string.Concat(LoadGameContent.gameContent.dreamFragmentText, tmpDFComponent.id);
                    else if (tmpDFComponent.type == 1 || tmpDFComponent.type == 2)
                        FragmentText.text = string.Concat("\"", tmpDFComponent.itemName, "\"");
                    // Pause this system and dependant systems
                    this.Pause = true;
                    f_movingModeSelector.First().GetComponent<MovingModeSelector>().pauseMovingSystems();
                    backupIARNavigationState = IARTabNavigation.instance.Pause;
                    IARTabNavigation.instance.Pause = true;
                    backupHighlighterState = Highlighter.instance.Pause;
                    Highlighter.instance.Pause = true;
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
                    objectType = "item",
                    objectName = selectedFragment.name,
                    activityExtensions = new Dictionary<string, string>() { { "type", "dreamFragment" } }
                });

                GameObjectManager.addComponent<PlaySound>(selectedFragment, new { id = 3 }); // id refer to FPSController AudioBank

                if (selectedFragment.GetComponentInParent<IsSolution>())
                {
                    if(f_player.First().transform.localScale.x < 0.9f)
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
        f_movingModeSelector.First().GetComponent<MovingModeSelector>().resumeMovingSystems();
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
            objectType = "viewable",
            objectName = string.Concat(selectedFragment.name, "_Link"),
            activityExtensions = new Dictionary<string, string>() { { "link", df.urlLink } }
        });
    }
}