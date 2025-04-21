using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class IARViewItem : FSystem {

    // Display item description and linked game object in the right panel when it is focused/clicked in the left panel

    // Contains all game object inside inventory under mouse cursor (only one)
    private Family f_viewed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfTags("InventoryElements"));
    // Contains all game objects selected inside inventory (SelectedInInventory is dynamically added by this system)
    private Family f_selected = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedInInventory), typeof(Collected), typeof(AnimatedSprites)), new AnyOfTags("InventoryElements"));
    private Family f_viewable = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_selectedItem = FamilyManager.getFamily(new AnyOfTags("IARItemSelected"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    public GameObject descriptionUI; 
    private GameObject descriptionTitle;
    private GameObject descriptionContent;
    private GameObject descriptionInfo;

    public GameObject defaultIarTab;

    private Dictionary<int, GameObject> id2go;

    private GameObject lastSelection = null;
    private GameObject lastfocusedItem = null;
    private GameObject lastItemShown = null;

    public static IARViewItem instance;

    public IARViewItem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        // cache UI elements
        descriptionTitle = descriptionUI.transform.GetChild(0).gameObject; // the first child is the title
        descriptionContent = descriptionUI.transform.GetChild(1).gameObject; // the second child is the content of the description
        descriptionInfo = descriptionUI.transform.GetChild(2).gameObject; // the third child is the usable info of the description

        // add callback on families
        f_viewed.addEntryCallback(onMouseEnterItem);
        f_viewed.addExitCallback(onMouseExitItem);
        f_selected.addEntryCallback(onNewItemSelected);
        f_selected.addExitCallback(onItemUnselected);
        f_viewable.addEntryCallback(onNewItemEnabled);
        f_viewable.addExitCallback(onItemDisabled);

        id2go = new Dictionary<int, GameObject>();
    }

    private void onNewItemEnabled(GameObject go)
    {
        if (!id2go.ContainsKey(go.GetInstanceID()))
            id2go.Add(go.GetInstanceID(), go);
    }

    private void onItemDisabled(int instanceId)
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
    private void onMouseEnterItem(GameObject go)
    {
        // force EventSystem to follow mouse focus
        EventSystem.current.SetSelectedGameObject(go);
    }

    // because f_viewed contains only one gameobject (thanks to PointerOver), if it exits family no game object are focused
    private void onMouseExitItem(int instanceId)
    {
        GameObject go;
        if (id2go.TryGetValue(instanceId, out go))
        {
            // if we exit from the current focused game object
            if (go == EventSystem.current.currentSelectedGameObject)
            {
                // Check if at least one item is selected
                if (f_selected.Count > 0)
                    EventSystem.current.SetSelectedGameObject(f_selected.getAt(f_selected.Count - 1));
                else
                    EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    // a new game object enter f_selected family => we consider this game object as the last game object selected
    private void onNewItemSelected(GameObject go)
    {
        // display description of this new selection
        showDescription(go);
    }

    // when a selected game object leaves f_selected family, we update the last game object selected to the last game object of the family
    private void onItemUnselected(int instanceId)
    {
        GameObject go;
        if (id2go.TryGetValue(instanceId, out go))
            showDescription(go);
    }

    // Show title and description of a game object
    // if this gameobject is linked with another one and is selected, the description area is replaced by its linked game object (except for glasses)
    private void showDescription(GameObject item)
    {
        // In case of last item shown has a linked gameobject, we hide it (except for glasses)
        if (lastItemShown && lastItemShown.GetComponent<LinkedWith>() && !lastItemShown.name.Contains("Glasses"))
            GameObjectManager.setGameObjectState(lastItemShown.GetComponent<LinkedWith>().link, false); // switch off the linked game object
        lastItemShown = item;
        if (item == null)
            GameObjectManager.setGameObjectState(descriptionUI, false);
        else
        {
            // Shows title and description
            GameObjectManager.setGameObjectState(descriptionUI, true);
            descriptionTitle.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().itemName;
            GameObjectManager.setGameObjectState(descriptionContent.transform.parent.gameObject, true); // switch on the description
            descriptionContent.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().description;
            GameObjectManager.setGameObjectState(descriptionInfo, true); // switch on the info
            descriptionInfo.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().info;

            // Check if the item is linked and selected
            if (item.GetComponent<LinkedWith>())
            {
                if (item.GetComponent<SelectedInInventory>())
                {
                    // replace description UI by linked game Object (exception for glasses)
                    if (!item.name.Contains("Glasses"))
                    {
                        GameObjectManager.setGameObjectState(descriptionContent.transform.parent.gameObject, false); // switch off the description
                        GameObjectManager.setGameObjectState(descriptionInfo, false); // switch off the info
                    }
                    GameObjectManager.setGameObjectState(item.GetComponent<LinkedWith>().link, true); // switch on the linked game object
                }
                else
                    GameObjectManager.setGameObjectState(item.GetComponent<LinkedWith>().link, false); // switch off the linked game object
            }
        }
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        // display last selection if it exists
        if (lastSelection)
            showDescription(lastSelection);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (lastfocusedItem != EventSystem.current.currentSelectedGameObject)
        {
            if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.tag == "InventoryElements")
            {
                showDescription(EventSystem.current.currentSelectedGameObject);
                lastfocusedItem = EventSystem.current.currentSelectedGameObject;
            }
            else if (f_selected.Count > 0)
            {
                showDescription(f_selected.getAt(f_selected.Count - 1));
                lastfocusedItem = f_selected.getAt(f_selected.Count - 1);
            }
            else
            {
                showDescription(null);
                lastfocusedItem = EventSystem.current.currentSelectedGameObject;
            }
        }
        if (lastfocusedItem == null || (!lastfocusedItem.activeInHierarchy && isChildOfDescription(lastfocusedItem)))
        {
            EventSystem.current.SetSelectedGameObject(defaultIarTab); // If new focused item is not active in hierarchy, we give to EventSystem the default element in IAR Tab (appears on keyboard navigation when player moves from unselected item to description scroll bar, in this case description will be hidden and keyboard navigation is braken).
        }


        foreach (GameObject go in f_selectedItem)
        {
            go.transform.Rotate(Vector3.back, Time.deltaTime * 10f);
        }
    }

    public void ToggleItem(GameObject item) // called from EventWrapper
    {
        AnimatedSprites animation = item.GetComponent<AnimatedSprites>();
        // get second child
        Transform secondChild = item.transform.GetChild(1);
        if (secondChild && secondChild.gameObject.tag == "IARItemSelected")
            GameObjectManager.setGameObjectState(secondChild.gameObject, !secondChild.gameObject.activeSelf);
        // we manage SelectedInInventory component
        if (item.GetComponent<SelectedInInventory>())
        {
            // Ne pas générer de trace pour Laalys dans les cas des fragments de l'énigme où il faut se baisser
            if (item.name != "CrouchFragments")
                GameObjectManager.addComponent<ActionPerformed>(item, new { name = "turnOff", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformedForLRS>(item, new 
            {
                verb = "deactivated",
                objectType = "item",
                activityExtensions = new Dictionary<string, string>() {
                    { "value", item.name }
                }
            });

            GameObjectManager.removeComponent<SelectedInInventory>(item);
            GameObjectManager.addComponent<PlaySound>(item, new { id = 14 }); // id refer to FPSController AudioBank

            // this game object is unselected and it contains a linked game object => we hide the linked game object
            if (item.GetComponent<LinkedWith>())
                GameObjectManager.setGameObjectState(item.GetComponent<LinkedWith>().link, false); // switch off the view of the last selection

            // enable HUD under cursor
            if (item.GetComponent<HUDItemSelected>())
                GameObjectManager.setGameObjectState(item.GetComponent<HUDItemSelected>().hudGO, false);

            animation.animate = false; // stop animation
        }
        else
        {
            // Ne pas générer de trace pour Laalys dans les cas des fragments de l'énigme où il faut se baisser
            if (item.name != "CrouchFragments")
                GameObjectManager.addComponent<ActionPerformed>(item, new { name = "turnOn", performedBy = "player" });
            GameObjectManager.addComponent<ActionPerformedForLRS>(item, new 
            {
                verb = "activated",
                objectType = "item",
                activityExtensions = new Dictionary<string, string>() {
                    { "value", item.name }
                }
            });

            GameObjectManager.addComponent<SelectedInInventory>(item);
            GameObjectManager.addComponent<PlaySound>(item, new { id = 13 }); // id refer to FPSController AudioBank

            // disable HUD under cursor
            if (item.GetComponent<HUDItemSelected>())
                GameObjectManager.setGameObjectState(item.GetComponent<HUDItemSelected>().hudGO, true);

            animation.animate = true; // start animation
        }
    }

    private bool isChildOfDescription(GameObject go)
    {
        if (go.tag == "DescriptionUI")
            return true;
        else if (go.transform.parent)
            return isChildOfDescription(go.transform.parent.gameObject);
        else return false;
    }
}