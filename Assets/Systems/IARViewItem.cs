using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using TMPro;

public class IARViewItem : FSystem {

    // Display item description and linked game object in the right panel when it is focused/clicked in the left panel

    // Contains all game object inside inventory under mouse cursor (only one)
    private Family f_viewed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfTags("InventoryElements"));
    // Contains all game objects selected inside inventory (SelectedInInventory is dynamically added by this system)
    private Family f_selected = FamilyManager.getFamily(new AllOfComponents(typeof(SelectedInInventory), typeof(Collected), typeof(AnimatedSprites)), new AnyOfTags("InventoryElements"));
    private Family f_descriptionUI = FamilyManager.getFamily(new AnyOfTags("DescriptionUI"));

    private GameObject descriptionUI;
    private GameObject descriptionTitle;
    private GameObject descriptionContent;

    private GameObject currentView = null;
    private GameObject lastSelection = null;

    public static IARViewItem instance;

    public IARViewItem()
    {
        if (Application.isPlaying)
        {
            // cache UI elements
            descriptionUI = f_descriptionUI.First();
            descriptionTitle = descriptionUI.transform.GetChild(0).gameObject; // the first child is the title
            descriptionContent = descriptionUI.transform.GetChild(1).gameObject; // the second child is the content of the description

            // add callback on families
            f_viewed.addEntryCallback(onEnterItem);
            f_viewed.addExitCallback(onExitItem);
            f_selected.addEntryCallback(onNewSelection);
            f_selected.addExitCallback(onSelectionRemoved);
        }
        instance = this;
    }

    // if new gameobject enter inside f_viewed we store it as current game object viewed
    private void onEnterItem(GameObject go)
    {
        currentView = go;
    }

    // because f_viewed contains only one gameobject (thanks to PointerOver), if it exits family no game object ar viewed
    private void onExitItem(int instanceId)
    {
        currentView = null;
    }

    // a new game object enter f_selected family => we consider this game object as the last game object selected
    private void onNewSelection(GameObject go)
    {
        lastSelection = go;
    }

    // when a selected game object leaves f_selected family, we update the last game object selected to the last game object of the family
    private void onSelectionRemoved(int instanceId)
    {
        if (f_selected.Count > 0)
            lastSelection = f_selected.getAt(f_selected.Count - 1);
        else
            lastSelection = null;
    }

    private void showDescription(GameObject item)
    {
        GameObjectManager.setGameObjectState(descriptionUI, true);
        GameObjectManager.setGameObjectState(descriptionTitle, true); // switch on the title
        GameObjectManager.setGameObjectState(descriptionContent, true); // switch off the description
        descriptionTitle.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().itemName;
        descriptionContent.GetComponent<TextMeshProUGUI>().text = item.GetComponent<Collected>().description;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // reset default UI game object states
        GameObjectManager.setGameObjectState(descriptionTitle, true);
        GameObjectManager.setGameObjectState(descriptionContent, true);
        GameObjectManager.setGameObjectState(descriptionUI, false);

        // if at least one game object is selected
        if (lastSelection)
        {
            // if it contains a linked game object => we display it in the right panel
            if (lastSelection.GetComponent<LinkedWith>())
            {
                GameObjectManager.setGameObjectState(descriptionUI, true);
                GameObjectManager.setGameObjectState(descriptionTitle, true); // switch on the title
                GameObjectManager.setGameObjectState(descriptionContent, false); // switch off the description
                descriptionTitle.GetComponent<TextMeshProUGUI>().text = lastSelection.GetComponent<Collected>().itemName;
                descriptionContent.GetComponent<TextMeshProUGUI>().text = "";
                GameObjectManager.setGameObjectState(lastSelection.GetComponent<LinkedWith>().link, true); // switch on the view linked in remplacement of the description
            }
            else // if not we simply display description in the right panel
                showDescription(lastSelection);
        }

        // in case the player view another game object than the last selected, we override right panel with its description
        if (currentView && currentView != lastSelection)
        {
            if (lastSelection && lastSelection.GetComponent<LinkedWith>())
                GameObjectManager.setGameObjectState(lastSelection.GetComponent<LinkedWith>().link, false); // switch off the view of the last selection
            showDescription(currentView);
        }

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
                    GameObjectManager.removeComponent<SelectedInInventory>(go);
                    // this game object is unselected and it is the last one selected with a linked game object => we hide the linked game object in the right panel
                    if (go == lastSelection && lastSelection.GetComponent<LinkedWith>())
                        GameObjectManager.setGameObjectState(lastSelection.GetComponent<LinkedWith>().link, false); // switch off the view of the last selection
                }
                else
                    GameObjectManager.addComponent<SelectedInInventory>(go);
            }
        }
    }
}