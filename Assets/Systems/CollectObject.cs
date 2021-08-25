using UnityEngine;
using FYFY;

public class CollectObject : FSystem {

    // This system shows objects in inventory when they are clicked in game

    //all collectable objects
    // 5 <=> UI layer. We want only in game linked game object so we exlude ones in UI
    // We process only Highlighted game objects (this component is dynamically added by Highlight system)
    private Family f_collectableObjects = FamilyManager.getFamily(new AllOfComponents(typeof(LinkedWith), typeof(Highlighted)), new NoneOfLayers(5), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_pressY = FamilyManager.getFamily(new AnyOfTags("PressY"));
    private Family f_HUD = FamilyManager.getFamily(new AnyOfTags("HUD_Main"));

    public static CollectObject instance;

    private GameObject itemCollectedNotif;

    public CollectObject()
    {
        if (Application.isPlaying)
        {
            itemCollectedNotif = f_HUD.First().transform.GetChild(f_HUD.First().transform.childCount-1).gameObject;
        }
        instance = this;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (Input.GetButtonDown("Fire1"))
        {
            foreach (GameObject collect in f_collectableObjects)
            {
                bool isDreamFragment = collect.GetComponent<DreamFragment>();
                if (!isDreamFragment)
                {
                    GameObjectManager.addComponent<ActionPerformed>(collect, new { name = "perform", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(collect, new { verb = "collected", objectType = "item", objectName = collect.name });
                }
                // enable UI target
                GameObjectManager.setGameObjectState(collect.GetComponent<LinkedWith>().link, true);
                // particular case of collecting room2 scrolls
                if (collect.name.Contains("Scroll") && collect.name.Length == 7)
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
                }
                // disable in-game source if it is not a dream fragment
                if (!isDreamFragment)
                    GameObjectManager.setGameObjectState(collect, false);
                // Play notification
                if (!isDreamFragment)
                    itemCollectedNotif.GetComponent<Animator>().SetTrigger("Start");
                else if (IARDreamFragmentManager.virtualDreamFragment)
                    itemCollectedNotif.GetComponent<Animator>().SetTrigger("Start2");

                // Play sound
                GameObjectManager.addComponent<PlaySound>(collect, new { id = 10 }); // id refer to FPSController AudioBank
                // particular case of collecting Intro_scroll game object => show ingame "Press Y" notification
                if (collect.name == "Intro_Scroll")
                {
                    GameObjectManager.setGameObjectState(f_pressY.First(), true);
                }
            }
        }
    }
}