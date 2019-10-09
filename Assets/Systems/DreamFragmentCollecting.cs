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

    private GameObject dfUI;
    private TextMeshProUGUI FragmentText;
    private RaycastHit hit;
    private GameObject selectedFragment;
    private DreamFragment tmpDFComponent;
    private bool backupIARNavigationState;
    private GameObject tmpGo;
    //button in dream fragment UI to open the link
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
                if (b.gameObject.name == "ButtonOnline")
                    onlineButton = b.gameObject;
            }
            // Get child text area
            FragmentText = dfUI.GetComponentInChildren<TextMeshProUGUI>();
        }
        instance = this;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        foreach(GameObject go in f_dreamFragments)
        {
            DreamFragment df = go.GetComponent<DreamFragment>();
            if (!df.viewed && df.type != 1)
                go.transform.Rotate(new Vector3(0, 20*Time.deltaTime, 0));
        }

        // Compute Raycast only when mouse is clicked
        if (Input.GetButtonDown("Fire1"))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                // try to find a fragment touched by the raycast
                if (f_dreamFragments.contains(hit.transform.gameObject.GetInstanceID()))
                {
                    // Show fragment UI
                    selectedFragment = hit.transform.gameObject;
                    GameObjectManager.addComponent<ActionPerformedForLRS>(selectedFragment, new { verb = "activated", objectType = "viewable", objectName = selectedFragment.name });
                    GameObjectManager.setGameObjectState(dfUI, true);
                    tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                    GameObjectManager.setGameObjectState(onlineButton, tmpDFComponent.urlLink != null && tmpDFComponent.urlLink != "");
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
    }

    public void CloseFragmentUI()
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedFragment, new { verb = "deactivated", objectType = "viewable", objectName = selectedFragment.name });
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
            selectedFragment.GetComponent<DreamFragment>().viewed = true;
        }
        selectedFragment = null;
        // close UI
        GameObjectManager.setGameObjectState(dfUI,false);
        // Unpause this system and dependants systems
        this.Pause = false;
        MovingSystem.instance.Pause = false;
        IARTabNavigation.instance.Pause = backupIARNavigationState;
    }

    public void OpenFragmentLink()
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
            File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Invalid dream fragment link: ", df.urlLink));
        }
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedFragment, new
        {
            verb = "accessed",
            objectType = "viewable",
            objectName = string.Concat(selectedFragment.name, "_Link"),
            activityExtensions = new Dictionary<string, List<string>>() { { "link", new List<string>() { df.urlLink } } }
        });
    }
}