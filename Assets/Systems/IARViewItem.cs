using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using FYFY_plugins.Monitoring;

public class IARViewItem : FSystem {

    // Display item description and linked game object in the right panel when it is focused/clicked in the left panel

    // Contains all game object inside inventory under mouse cursor (only one)
    private Family f_viewed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfTags("InventoryElements"));
    // Contains all game objects selected inside inventory (SelectedInInventory is dynamically added by this system)
    private Family f_selected = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedInInventory), typeof(Collected), typeof(AnimatedSprites)), new AnyOfTags("InventoryElements"));
    private Family f_descriptionUI = FamilyManager.getFamily(new AnyOfTags("DescriptionUI"));
    private Family f_viewable = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_collectedPuzzles = FamilyManager.getFamily(new AnyOfTags("Puzzle"), new NoneOfComponents(typeof(DreamFragment)), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_selectedBag = FamilyManager.getFamily(new AnyOfTags("Bag"), new AllOfComponents(typeof(ReadyToWork)));

    private Family f_soundObj = FamilyManager.getFamily(new AllOfComponents(typeof(AudioBank), typeof(AudioSource)));

    private GameObject descriptionUI;
    private GameObject descriptionTitle;
    private GameObject descriptionContent;
    private GameObject descriptionInfo;

    private Dictionary<int, GameObject> id2go;

    private GameObject currentView = null;
    private GameObject lastSelection = null;

    //Variables used to blur the background when IAR opened
    public static float focusDistance;
    public static float initialFocusDistance = 1.5f;

    public static IARViewItem instance;

    public IARViewItem()
    {
        if (Application.isPlaying)
        {
            // cache UI elements
            descriptionUI = f_descriptionUI.First();
            descriptionTitle = descriptionUI.transform.GetChild(0).gameObject; // the first child is the title
            descriptionContent = descriptionUI.transform.GetChild(1).gameObject; // the second child is the content of the description
            descriptionInfo = descriptionUI.transform.GetChild(2).gameObject; // the third child is the usable info of the description

            // add callback on families
            f_viewed.addEntryCallback(onEnterItem);
            f_viewed.addExitCallback(onExitItem);
            f_selected.addEntryCallback(onNewSelection);
            f_selected.addExitCallback(onSelectionRemoved);
            f_viewable.addEntryCallback(onEnable);
            f_viewable.addExitCallback(onDisable);

            id2go = new Dictionary<int, GameObject>();
        }
        instance = this;
    }

    private void onEnable(GameObject go)
    {
        if (!id2go.ContainsKey(go.GetInstanceID()))
            id2go.Add(go.GetInstanceID(), go);
    }

    private void onDisable(int instanceId)
    {
        GameObject go;
        if (id2go.TryGetValue(instanceId, out go)) {
            if (go.GetComponent<SelectedInInventory>())
                GameObjectManager.removeComponent<SelectedInInventory>(go);
            if (go.GetComponent<LinkedWith>())
                GameObjectManager.setGameObjectState(go.GetComponent<LinkedWith>().link, false); // switch off the linked game object
        }
    }

    // if new gameobject enter inside f_viewed we store it as current game object viewed
    private void onEnterItem(GameObject go)
    {
        currentView = go;
        // show description of the new focused Game Object
        showDescription(go);
    }

    // because f_viewed contains only one gameobject (thanks to PointerOver), if it exits family no game object are focused
    private void onExitItem(int instanceId)
    {
        // if we exit a selected and linked game object which is not the last selected we hide its linked game object (exception for glasses)
        if (currentView && currentView != lastSelection && currentView.GetComponent<LinkedWith>() && currentView.GetComponent<SelectedInInventory>() && !currentView.name.Contains("Glasses"))
            GameObjectManager.setGameObjectState(currentView.GetComponent<LinkedWith>().link, false); // switch off the linked game object

        if(currentView)
            GameObjectManager.addComponent<ActionPerformedForLRS>(currentView, new
            {
                verb = "exitedView",
                objectType = "viewable",
                objectName = string.Concat(currentView.name, "_Description")
            });

        currentView = null;

        // if at least one game object was selected => we display its description
        if (lastSelection)
            showDescription(lastSelection);
        else
            GameObjectManager.setGameObjectState(descriptionUI, false);
    }

    // a new game object enter f_selected family => we consider this game object as the last game object selected
    private void onNewSelection(GameObject go)
    {
        lastSelection = go;
        // display description of this new selection
        showDescription(go);
        //Play sound effect when object is activate 
        f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[11]);
        GameObjectManager.addComponent<ActionPerformedForLRS>(go, new
        {
            verb = "read",
            objectType = "viewable",
            objectName = string.Concat(go.name, "_Description"),
            activityExtensions = new Dictionary<string, List<string>>() { { "content", new List<string>() { descriptionContent.GetComponent<TextMeshProUGUI>().text } } }
        });
    }

    // when a selected game object leaves f_selected family, we update the last game object selected to the last game object of the family
    private void onSelectionRemoved(int instanceId)
    {
        //Play sound effect when object is desactivate
        f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[12]);
        if (f_selected.Count > 0)
            lastSelection = f_selected.getAt(f_selected.Count - 1);            
        else
            lastSelection = null;

        // Update the current view in order to take in consideration the new last selection
        if (currentView)
            showDescription(currentView);
        else
            onExitItem(-1);
    }

    // return true if UI with name "name" is selected into inventory
    private GameObject isSelected(string name)
    {
        Debug.Log("obj selected");
        foreach (GameObject go in f_selected)
            if (go.name == name)
                return go;
        return null;
    }

    // Show title and description of a game object
    // if this gameobject is linked with another one and is selected, the description area is replaced by its linked game object (except for glasses)
    private void showDescription(GameObject item)
    {
        // Shows title and description
        GameObjectManager.setGameObjectState(descriptionUI, true);
        descriptionTitle.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().itemName;
        GameObjectManager.setGameObjectState(descriptionContent, true); // switch on the description
        descriptionContent.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().description;
        GameObjectManager.setGameObjectState(descriptionInfo, true); // switch on the info
        descriptionInfo.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().info;

        // Check if the item is linked and selected
        if (item.GetComponent<LinkedWith>() && item.GetComponent<SelectedInInventory>())
        {
            // replace description UI by linked game Object (exception for glasses)
            if (!item.name.Contains("Glasses"))
            {
                GameObjectManager.setGameObjectState(descriptionContent, false); // switch off the description
                GameObjectManager.setGameObjectState(descriptionInfo, false); // switch off the info
            }
            GameObjectManager.setGameObjectState(item.GetComponent<LinkedWith>().link, true); // switch on the linked game object
            
        }

        // if item to display is not the last selection but this last selection is linked with antoher game object => we hide linked game object
        // of the last selection to avoid superpositions (except for glasses)
        if (item != lastSelection && lastSelection && lastSelection.GetComponent<LinkedWith>() && !lastSelection.name.Contains("Glasses"))
            GameObjectManager.setGameObjectState(lastSelection.GetComponent<LinkedWith>().link, false); // switch off the linked game object
        
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        focusDistance = initialFocusDistance;
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        focusDistance = 0.1f;
        // display last selection if it exists
        if (lastSelection)
            showDescription(lastSelection);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // mouse click management
        if (Input.GetMouseButtonDown(0))
        {
            // We parse all viewed game object (only once)
            foreach (GameObject go in f_viewed)
            {
                // we toggle animation
                AnimatedSprites animation = go.GetComponent<AnimatedSprites>();
                animation.animate = !animation.animate;
                // we manage SelectedInInventory component
                if (go.GetComponent<SelectedInInventory>())
                {
                    GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOff", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(go, new { verb = "deactivated", objectType = "item", objectName = go.name });
                    GameObjectManager.removeComponent<SelectedInInventory>(go);

                    if (go.name.Contains("Glasses"))
                    {
                        if (f_selectedBag.Count > 0)
                        {
                            if (isSelected("Glasses1"))
                            {
                                GameObjectManager.addComponent<ActionPerformed>(f_selectedBag.First().transform.GetChild(1).gameObject, new { overrideName = "activate2", performedBy = "player" });
                                if(f_selectedBag.Count > 0)
                                    GameObjectManager.addComponent<ActionPerformedForLRS>(f_selectedBag.First().transform.GetChild(1).gameObject, new { verb = "accessed", objectType = "interactable", objectName = "paper2" });
                            }
                            else if (isSelected("Glasses2"))
                            {
                                GameObjectManager.addComponent<ActionPerformed>(f_selectedBag.First().transform.GetChild(1).gameObject, new { overrideName = "activate3", performedBy = "player" });
                                if (f_selectedBag.Count > 0)
                                    GameObjectManager.addComponent<ActionPerformedForLRS>(f_selectedBag.First().transform.GetChild(1).gameObject, new { verb = "accessed", objectType = "interactable", objectName = "paper3" });
                            }
                            else
                            {
                                GameObjectManager.addComponent<ActionPerformed>(f_selectedBag.First().transform.GetChild(1).gameObject, new { overrideName = "activate", performedBy = "player" });
                                if (f_selectedBag.Count > 0)
                                    GameObjectManager.addComponent<ActionPerformedForLRS>(f_selectedBag.First().transform.GetChild(1).gameObject, new { verb = "accessed", objectType = "interactable", objectName = "paper1" });
                            }
                        }
                    }
                    // this game object is unselected and it contains a linked game object => we hide the linked game object
                    if (go.GetComponent<LinkedWith>())
                        GameObjectManager.setGameObjectState(go.GetComponent<LinkedWith>().link, false); // switch off the view of the last selection
                }
                else
                {
                    GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(go, new { verb = "activated", objectType = "item", objectName = go.name });

                    GameObjectManager.addComponent<SelectedInInventory>(go);
                    
                    
                    if(go.name.Contains("Glasses"))
                    {
                        if(f_selectedBag.Count > 0)
                        {
                            if (isSelected("Glasses1") && isSelected("Glasses2"))
                            {
                                GameObjectManager.addComponent<ActionPerformed>(f_selectedBag.First().transform.GetChild(1).gameObject, new { overrideName = "activate4", performedBy = "player" });
                                if (f_selectedBag.Count > 0)
                                    GameObjectManager.addComponent<ActionPerformedForLRS>(f_selectedBag.First().transform.GetChild(1).gameObject, new { verb = "accessed", objectType = "interactable", objectName = "paper4" });
                            }
                            else if (isSelected("Glasses1"))
                            {
                                GameObjectManager.addComponent<ActionPerformed>(f_selectedBag.First().transform.GetChild(1).gameObject, new { overrideName = "activate2", performedBy = "player" });
                                if (f_selectedBag.Count > 0)
                                    GameObjectManager.addComponent<ActionPerformedForLRS>(f_selectedBag.First().transform.GetChild(1).gameObject, new { verb = "accessed", objectType = "interactable", objectName = "paper2" });
                            }
                            else if (isSelected("Glasses2"))
                            {
                                GameObjectManager.addComponent<ActionPerformed>(f_selectedBag.First().transform.GetChild(1).gameObject, new { overrideName = "activate3", performedBy = "player" });
                                if (f_selectedBag.Count > 0)
                                    GameObjectManager.addComponent<ActionPerformedForLRS>(f_selectedBag.First().transform.GetChild(1).gameObject, new { verb = "accessed", objectType = "interactable", objectName = "paper3" });
                            }
                        }
                    }
                }
            }
        }
    }
}