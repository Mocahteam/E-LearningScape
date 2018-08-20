using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;

public class IARNewItemAvailable : FSystem {

    // Disable feedback when parent item is clicked or under mouse pointer

    private Family itemsEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family notificationEnabled = FamilyManager.getFamily(new AnyOfTags("NewItemFeedback"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family newItemOver = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager), typeof(PointerOver)));
    private Family inventoryWarning = FamilyManager.getFamily(new AnyOfTags("InventoryWarning"));

    private bool warningNewItem = true;
    private bool HUD_neverDisplayed = true;

    private Dictionary<int, GameObject> id2Go;

    public static IARNewItemAvailable instance;

    public IARNewItemAvailable()
    {
        if (Application.isPlaying)
        {
            itemsEnabled.addEntryCallback(OnNewItemEnabled);
            itemsEnabled.addExitCallback(OnItemDisabled);
            newItemOver.addEntryCallback(OnMouseEnter);

            id2Go = new Dictionary<int, GameObject>();
        }
        instance = this;
    }

    private GameObject getFeedbackChild(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).CompareTag("NewItemFeedback"))
            {
                return go.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }

    private void OnNewItemEnabled(GameObject go)
    {
        GameObject child = getFeedbackChild(go);
        if (child)
            GameObjectManager.setGameObjectState(child, true);
        if (!id2Go.ContainsKey(go.GetInstanceID()))
            id2Go.Add(go.GetInstanceID(), go);
    }

    private void OnItemDisabled(int instanceId)
    {
        // if an item is disable => disable also its child tagged NewItemFeedback
        GameObject go;
        if (id2Go.TryGetValue(instanceId, out go))
        {
            GameObject child = getFeedbackChild(go);
            if (child)
                GameObjectManager.setGameObjectState(child, false);
        }
    }

    private void OnMouseEnter(GameObject go)
    {
        // find child with tag "NewItemFeedback"
        GameObject child = getFeedbackChild(go);
        if (child && child.activeInHierarchy)
        {
            NewItemManager nim = go.GetComponent<NewItemManager>();
            if (nim.disableOnMouseOver || (nim.disableOnClick && Input.GetMouseButton(0)))
                GameObjectManager.setGameObjectState(child, false);
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // manage click when mouse is over an item
        foreach (GameObject go in newItemOver)
            OnMouseEnter(go); // same process as OnMouseEnter callback

        // blink HUD "A" if at least one new item is available
        if (notificationEnabled.Count > 0)
        {
            if (Time.time - (int)Time.time > 0.5f && warningNewItem)
            {
                if (HUD_neverDisplayed)
                {
                    // enable parent
                    GameObjectManager.setGameObjectState(inventoryWarning.First().transform.parent.gameObject, true);
                    HUD_neverDisplayed = false;
                }
                // display warning
                GameObjectManager.setGameObjectState(inventoryWarning.First(), true);
                warningNewItem = false;
            }
            else if (Time.time - (int)Time.time < 0.5f && !warningNewItem)
            {
                // disable warning
                GameObjectManager.setGameObjectState(inventoryWarning.First(), false);
                warningNewItem = true;
            }
        }
        else
        {
            if (!warningNewItem)
            {
                GameObjectManager.setGameObjectState(inventoryWarning.First(), false);
                warningNewItem = true;
            }
        }
    }

}