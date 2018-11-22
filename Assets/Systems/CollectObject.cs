using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class CollectObject : FSystem {

    // This system shows objects in inventory when they are clicked in game

    //all collectable objects
    // 5 <=> UI layer. We want only in game linked game object so we exlude ones in UI
    // We process only Highlighted game objects (this component is dynamically added by Highlight system)
    private Family f_collectableObjects = FamilyManager.getFamily(new AllOfComponents(typeof(LinkedWith), typeof(Highlighted)), new NoneOfLayers(5), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_pressA = FamilyManager.getFamily(new AnyOfTags("PressA"));
    private Family f_inventoryElements = FamilyManager.getFamily(new AnyOfTags("InventoryElements"));

    public static CollectObject instance;

    private GameObject seenScroll;
    private GameObject seenPuzzle;

    public CollectObject()
    {
        if (Application.isPlaying)
        {
            int nbElem = f_inventoryElements.Count;
            for(int i = 0; i < nbElem; i++)
            {
                if (f_inventoryElements.getAt(i).name == "Puzzle")
                {
                    seenPuzzle = f_inventoryElements.getAt(i);
                    GameObjectManager.addComponent<LinkLabel>(f_inventoryElements.getAt(i));
                    break;
                }
            }
        }
        instance = this;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (GameObject collect in f_collectableObjects)
            {
                GameObjectManager.addComponent<ActionPerformedForLRS>(collect, new { verb = "collected", objectType = "item", objectName = collect.name });
                // enable UI target
                GameObjectManager.setGameObjectState(collect.GetComponent<LinkedWith>().link, true);
                GameObjectManager.addComponent<ActionPerformed>(collect, new { name = "perform", performedBy = "player"});
                // particular case of collecting room2 scrolls
                if (collect.name.Contains("_Scroll") && collect.name.Length == 8)
                {
                    // find link into IAR left screen
                    GameObject UI_metaScroll = collect.GetComponent<LinkedWith>().link;
                    GameObjectManager.setGameObjectState(UI_metaScroll.transform.GetChild(0).gameObject, true); // force to enable new item notification
                    // find link into IAR right screen
                    GameObject UIScroll = UI_metaScroll.GetComponent<LinkedWith>().link.transform.Find(collect.name).gameObject;
                    // enable it
                    GameObjectManager.setGameObjectState(UIScroll, true);
                }
                // particular case of puzzle pieces
                if (collect.name.Contains("PuzzleSet_") && collect.name.Length == 12)
                {
                    // find link into IAR left screen
                    GameObject UI_metaScroll = collect.GetComponent<LinkedWith>().link;
                    GameObjectManager.setGameObjectState(UI_metaScroll.transform.GetChild(0).gameObject, true); // force to enable new item notification
                    // find link into IAR right screen
                    GameObject UIScroll = UI_metaScroll.GetComponent<LinkedWith>().link.transform.Find(collect.name).gameObject;
                    // enable it
                    GameObjectManager.setGameObjectState(UIScroll, true);

                    switch (collect.name[collect.name.Length -1])
                    {
                        case '1':
                            seenPuzzle.GetComponent<LinkLabel>().text = "l5";
                            break;

                        case '2':
                            seenPuzzle.GetComponent<LinkLabel>().text = "l6";
                            break;

                        case '3':
                            seenPuzzle.GetComponent<LinkLabel>().text = "l7";
                            break;

                        case '4':
                            seenPuzzle.GetComponent<LinkLabel>().text = "l8";
                            break;

                        case '5':
                            seenPuzzle.GetComponent<LinkLabel>().text = "l9";
                            break;

                        default:
                            break;
                    }
                }
                // disable in-game source
                GameObjectManager.setGameObjectState(collect, false);
                // particular case of collecting Intro_scroll game object => show HUD "A"
                if (collect.name == "Intro_Scroll")
                {
                    GameObjectManager.setGameObjectState(f_pressA.First(), true);
                }
            }
        }
    }
}