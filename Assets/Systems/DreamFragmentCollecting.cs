using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class DreamFragmentCollecting : FSystem {

    // Display Fragment UI when player select a fragment in game

    private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
    private Family f_dreamFragmentUI = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AnyOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD));
    private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));
    private Family f_inventoryElements = FamilyManager.getFamily(new AnyOfTags("InventoryElements"));
    private Family f_collectedPuzzleFragments = FamilyManager.getFamily(new AllOfComponents(typeof(PuzzleFragmentSeen)));

    private GameObject dfUI;
    private TextMeshProUGUI FragmentText;
    private RaycastHit hit;
    private GameObject selectedFragment;
    private DreamFragment tmpDFComponent;
    private bool[] fragmentsSeen;
    private bool backupIARNavigationState;
    private GameObject puzzleLeftUI;
    private bool allPuzzleFragmentsCollected = false;
    private GameObject tmpGo;

    private Dictionary<string, string> dreamFragmentsLinks;
    private GameObject onlineButton;

    public static DreamFragmentCollecting instance;

    public DreamFragmentCollecting()
    {
        if (Application.isPlaying)
        {
            dfUI = f_dreamFragmentUI.First();
            // Add listener on child button to close UI
            foreach(Button b in dfUI.GetComponentsInChildren<Button>())
            {
                if(b.gameObject.name == "OKButton")
                    dfUI.GetComponentInChildren<Button>().onClick.AddListener(CloseWindow);
                else if (b.gameObject.name == "ButtonOnline")
                {
                    b.onClick.AddListener(OpenFragmentLink);
                    onlineButton = b.gameObject;
                }
            }
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

            if (File.Exists(LoadGameContent.gameContent.dreamFragmentLinksPath))
                dreamFragmentsLinks = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(LoadGameContent.gameContent.dreamFragmentLinksPath));
            else
            {
                Debug.LogWarning("Unable to load dream fragment links because no file found.");
                File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Warning - Unable to load dream fragment links because no file found"));
            }
            if (dreamFragmentsLinks == null)
                dreamFragmentsLinks = new Dictionary<string, string>();
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
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedFragment, new { verb = "activated", objectType = "fragment", objectName = selectedFragment.name });
                    GameObjectManager.setGameObjectState(dfUI, true);
                    tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                    GameObjectManager.setGameObjectState(onlineButton, dreamFragmentsLinks.ContainsKey(selectedFragment.name) && dreamFragmentsLinks[selectedFragment.name] != "");
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

                    if (selectedFragment.transform.parent.gameObject.tag == "chair" && selectedFragment.transform.parent.gameObject.GetComponent<IsSolution>())
                    {
                        if(f_player.First().transform.localScale.x < 0.9f)
                            //if the player is crouching
                            GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player", orLabels = new string[] { "l21"} });
                        else
                            GameObjectManager.addComponent<ActionPerformed>(selectedFragment, new { name = "activate", performedBy = "player", orLabels = new string[] { "l20" } });

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

    private void OpenFragmentLink()
    {
        Application.OpenURL(dreamFragmentsLinks[selectedFragment.name]);
    }
}