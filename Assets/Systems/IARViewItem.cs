using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
using FYFY_plugins.Monitoring;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IARViewItem : FSystem {

    // Display item description and linked game object in the right panel when it is focused/clicked in the left panel

    // Contains all IAR tabs
    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));
    // Contains all game object inside inventory under mouse cursor (only one)
    private Family f_itemUnderMouse = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfTags("InventoryElements"));
    // Contains all selectable game objects inside inventory
    private Family f_selectable = FamilyManager.getFamily(new AllOfComponents(typeof(UnityEngine.UI.Selectable)), new AnyOfTags("InventoryElements"));
    // Contains all game objects selected inside inventory (SelectedInInventory is dynamically added by this system)
    private Family f_selectedItems = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedInInventory), typeof(Collected), typeof(AnimatedSprites)), new AnyOfTags("InventoryElements"));
    // Contains all game objects unlocked inside inventory
    private Family f_unlockedItems = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));

    private Family f_descriptionUI = FamilyManager.getFamily(new AnyOfTags("DescriptionUI"));


    private GameObject descriptionUI;
    private GameObject descriptionTitle;
    private GameObject descriptionContent;
    private GameObject descriptionInfo;

    private Dictionary<int, GameObject> id2go;

    private GameObject lastItemShown = null;
    private GameObject lastfocusedItem = null;

    public static IARViewItem instance;

    public IARViewItem()
    {
        if (Application.isPlaying)
        {
            // cache UI elements
            descriptionUI = f_descriptionUI.First();
            descriptionTitle = descriptionUI.transform.GetChild(0).gameObject; // the first child is the title
            descriptionContent = descriptionUI.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).gameObject; // the second child contain a GO and his second child is the content of the description
            descriptionInfo = descriptionUI.transform.GetChild(2).gameObject; // the third child is the usable info of the description

            // add callbacks on families
            f_unlockedItems.addEntryCallback(onNewItemEnabled);
            f_unlockedItems.addExitCallback(onItemDisabled);
            f_itemUnderMouse.addEntryCallback(onMouseEnterItem);
            f_itemUnderMouse.addExitCallback(onMouseExitItem);
            f_selectedItems.addEntryCallback(onNewItemSelected);
            f_selectedItems.addExitCallback(onItemUnselected);

            id2go = new Dictionary<int, GameObject>();
        }
        instance = this;
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
        }
    }

    private void onMouseEnterItem(GameObject go)
    {
        // force EventSystem to follow mouse focus
        EventSystem.current.SetSelectedGameObject(go);
        GameObjectManager.addComponent<ActionPerformedForLRS>(go, new
        {
            verb = "View",
            objectType = "viewable",
            objectName = string.Concat(go.name, "_Description")
        });
    }

    private void onMouseExitItem(int instanceId)
    {
        GameObject go;
        if (id2go.TryGetValue(instanceId, out go))
        {
            // if we exit from the current focused game object
            if (go == EventSystem.current.currentSelectedGameObject)
            {
                // Check if at least one item is selected
                if (f_selectedItems.Count > 0)
                    EventSystem.current.SetSelectedGameObject(f_selectedItems.getAt(f_selectedItems.Count-1));
                else
                    EventSystem.current.SetSelectedGameObject(null);

                GameObjectManager.addComponent<ActionPerformedForLRS>(go, new
                {
                    verb = "exitedView",
                    objectType = "viewable",
                    objectName = string.Concat(go.name, "_Description")
                });
            }
        }
    }

    private void onNewItemSelected(GameObject go)
    {
        showDescription(go);
    }

    private void onItemUnselected(int instanceId)
    {
        GameObject go;
        if (id2go.TryGetValue(instanceId, out go))
        {
            showDescription(go);
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // manage validation
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("X_button"))
        {
            // mouse case
            if (Input.GetMouseButtonDown(0) && f_itemUnderMouse.Count > 0)
                toggleItem(f_itemUnderMouse.First());
            else // keyboard case
            {
                foreach (GameObject go in f_selectable)
                    if (go == EventSystem.current.currentSelectedGameObject)
                    {
                        toggleItem(go);
                        break;
                    }
            }
        }
        else
        {
            if (lastfocusedItem != EventSystem.current.currentSelectedGameObject)
            {
                if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.tag == "InventoryElements")
                {
                    showDescription(EventSystem.current.currentSelectedGameObject);
                    lastfocusedItem = EventSystem.current.currentSelectedGameObject;
                }
                else if (f_selectedItems.Count > 0)
                {
                    showDescription(f_selectedItems.First());
                    lastfocusedItem = f_selectedItems.First();
                }
                else
                {
                    showDescription(null);
                    lastfocusedItem = EventSystem.current.currentSelectedGameObject;
                }
            }
            if (lastfocusedItem == null || (!lastfocusedItem.activeInHierarchy && isChildOfDescription(lastfocusedItem)))
                EventSystem.current.SetSelectedGameObject(f_tabs.First()); // If new focused item is not active in hierarchy, we give to EventSystem the first element in IAR Tab (appears on keyboard navigation when player moves from unselected item to description scroll bar, in this case description will be hidden and keyboard navigation is braken).
        }
    }

    private void toggleItem(GameObject go)
    {
        Debug.Log("selectItem " + go);
        if (go)
        {
            // we toggle animation
            AnimatedSprites animation = go.GetComponent<AnimatedSprites>();
            animation.animate = !animation.animate;
            if (go.transform.GetChild(1))
                GameObjectManager.setGameObjectState(go.transform.GetChild(1).gameObject, !go.transform.GetChild(1).gameObject.activeSelf);
            // we manage SelectedInInventory component
            if (go.GetComponent<SelectedInInventory>())
            {
                GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOff", performedBy = "player" });
                GameObjectManager.addComponent<ActionPerformedForLRS>(go, new { verb = "deactivated", objectType = "item", objectName = go.name });

                GameObjectManager.removeComponent<SelectedInInventory>(go);
            }
            else
            {
                GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
                GameObjectManager.addComponent<ActionPerformedForLRS>(go, new { verb = "activated", objectType = "item", objectName = go.name });

                GameObjectManager.addComponent<SelectedInInventory>(go);
               
            }
        }
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
            GameObjectManager.setGameObjectState(descriptionContent.transform.parent.parent.parent.gameObject, true); // switch on the description
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
                        GameObjectManager.setGameObjectState(descriptionContent.transform.parent.parent.parent.gameObject, false); // switch off the description
                        GameObjectManager.setGameObjectState(descriptionInfo, false); // switch off the info
                    }
                    GameObjectManager.setGameObjectState(item.GetComponent<LinkedWith>().link, true); // switch on the linked game object
                }
                else
                    GameObjectManager.setGameObjectState(item.GetComponent<LinkedWith>().link, false); // switch off the linked game object
            }
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