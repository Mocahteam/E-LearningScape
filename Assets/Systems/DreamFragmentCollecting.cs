using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class DreamFragmentCollecting : FSystem {

    // Display Fragment UI when player select a fragment in game

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
    private Family f_dreamFragmentUI = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AnyOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD));
    private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));
    private Family f_mainloop = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));
    private Family f_inventoryElements = FamilyManager.getFamily(new AnyOfTags("InventoryElements"));
    private Family f_collectedPuzzleFragments = FamilyManager.getFamily(new AllOfComponents(typeof(PuzzleFragmentSeen)));

    private GameObject dfUI;
    private TextMeshProUGUI FragmentText;
    private RaycastHit hit;
    private GameObject selectedFragment;
    private DreamFragment tmpDFComponent;
    private bool[] fragmentsSeen;
    private bool enigmaSolved = false;
    private bool backupIARNavigationState;
    private GameObject puzzleLeftUI;
    private bool allPuzzleFragmentsCollected = false;
    private GameObject tmpGo;

    public static DreamFragmentCollecting instance;

    public DreamFragmentCollecting()
    {
        if (Application.isPlaying)
        {
            dfUI = f_dreamFragmentUI.First();
            // Add listener on child button to close UI
            dfUI.GetComponentInChildren<Button>().onClick.AddListener(CloseWindow);
            // Get child text area
            FragmentText = dfUI.GetComponentInChildren<TextMeshProUGUI>();

            fragmentsSeen = new bool[6];
            for(int i = 0; i < fragmentsSeen.Length; i++)
                fragmentsSeen[i] = false;
            int nbInvetoryElems = f_inventoryElements.Count;
            for(int i = 0; i < nbInvetoryElems; i++)
            {
                if(f_inventoryElements.getAt(i).name == "Puzzle")
                {
                    puzzleLeftUI = f_inventoryElements.getAt(i);
                    break;
                }
            }
        }
        instance = this;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // Compute Raycast only when mouse is clicked
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                // try to find a fragment touched by the raycast
                if (f_dreamFragments.contains(hit.transform.gameObject.GetInstanceID()))
                {
                    // Show fragment UI
                    selectedFragment = hit.transform.gameObject;
                    GameObjectManager.setGameObjectState(dfUI, true);
                    tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                    // Set UI text depending on type and id
                    if (tmpDFComponent.type == 0)
                        FragmentText.text = string.Concat("Ouvrez le fragment de rêve numéro ", tmpDFComponent.id);
                    else if (tmpDFComponent.type == 1 || tmpDFComponent.type == 2)
                        FragmentText.text = string.Concat("\"", tmpDFComponent.itemName, "\"");
                    // Pause this system and dependant systems
                    this.Pause = true;
                    MovingSystem.instance.Pause = true;
                    backupIARNavigationState = IARTabNavigation.instance.Pause;
                    IARTabNavigation.instance.Pause = true;

                    switch (selectedFragment.name)
                    {
                        case "Fragment_souvenir_9":
                            GameObjectManager.addComponent<SyllabusSeen>(selectedFragment);
                            break;

                        case "Fragment_souvenir_17":
                            GameObjectManager.addComponent<SyllabusSeen>(selectedFragment);
                            break;

                        case "Fragment_souvenir_1":
                            GameObjectManager.addComponent<BerthiaumeClueSeen>(selectedFragment);
                            break;

                        case "Fragment_souvenir_7":
                            GameObjectManager.addComponent<PuzzleFragmentSeen>(selectedFragment);
                            if(!allPuzzleFragmentsCollected && f_collectedPuzzleFragments.Count == 5)
                            {
                                allPuzzleFragmentsCollected = true;
                                GameObjectManager.addComponent<ActionPerformed>(puzzleLeftUI, new { name = "perform", performedBy = "system" });
                            }
                            break;

                        case "Fragment_souvenir_14":
                            GameObjectManager.addComponent<PuzzleFragmentSeen>(selectedFragment);
                            if (!allPuzzleFragmentsCollected && f_collectedPuzzleFragments.Count == 5)
                            {
                                allPuzzleFragmentsCollected = true;
                                GameObjectManager.addComponent<ActionPerformed>(puzzleLeftUI, new { name = "perform", performedBy = "system" });
                            }
                            break;

                        case "Fragment_souvenir_15":
                            GameObjectManager.addComponent<PuzzleFragmentSeen>(selectedFragment);
                            if (!allPuzzleFragmentsCollected && f_collectedPuzzleFragments.Count == 5)
                            {
                                allPuzzleFragmentsCollected = true;
                                GameObjectManager.addComponent<ActionPerformed>(puzzleLeftUI, new { name = "perform", performedBy = "system" });
                            }
                            break;

                        case "Fragment_souvenir_16":
                            GameObjectManager.addComponent<PuzzleFragmentSeen>(selectedFragment);
                            if (!allPuzzleFragmentsCollected && f_collectedPuzzleFragments.Count == 5)
                            {
                                allPuzzleFragmentsCollected = true;
                                GameObjectManager.addComponent<ActionPerformed>(puzzleLeftUI, new { name = "perform", performedBy = "system" });
                            }
                            break;

                        case "Fragment_souvenir_18":
                            GameObjectManager.addComponent<PuzzleFragmentSeen>(selectedFragment);
                            if (!allPuzzleFragmentsCollected && f_collectedPuzzleFragments.Count == 5)
                            {
                                allPuzzleFragmentsCollected = true;
                                GameObjectManager.addComponent<ActionPerformed>(puzzleLeftUI, new { name = "perform", performedBy = "system" });
                            }
                            break;

                        default:
                            break;
                    }

                    if (selectedFragment.name == "Fragment_souvenir_de")
                    {
                        if(f_player.First().transform.localScale.x < 0.9f)
                            GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player", orLabels = new string[] { "l16"} });
                        else
                            GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player", orLabels = new string[] { "l17" } });

                    }
                    else
                        GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player" });
                }
            }
        }
    }

    private void CloseWindow()
    {
        if (selectedFragment.GetComponent<DreamFragment>().type != 2)
        {
            // disable particles
            if (selectedFragment.GetComponentInChildren<ParticleSystem>())
                GameObjectManager.setGameObjectState(selectedFragment.GetComponentInChildren<ParticleSystem>().gameObject,false);
            // disable glowing
            foreach (Transform child in selectedFragment.transform)
            {
                if (child.gameObject.tag == "DreamFragmentLight")
                {
                    child.gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                    break;
                }
            }
        }
        selectedFragment = null;
        // close UI
        GameObjectManager.setGameObjectState(dfUI,false);
        // Unpause this system and dependants systems
        this.Pause = false;
        MovingSystem.instance.Pause = false;
        IARTabNavigation.instance.Pause = backupIARNavigationState;
    }
}