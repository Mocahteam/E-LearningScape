using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class CollectObject : FSystem {

    // This system enables objects in inventory when they are clicked in game

    //all collectable objects
    // 5 <=> UI layer. We want only in game linked game object so we exlude ones in UI
    // We process only Highlighted game objects (this component is dynamically added by Highlight system)
    private Family collectableObjects = FamilyManager.getFamily(new AllOfComponents(typeof(LinkedWith), typeof(Highlighted)), new NoneOfLayers(5), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family pressA = FamilyManager.getFamily(new AnyOfTags("PressA"));

    public static CollectObject instance;

    public CollectObject()
    {
        instance = this;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (GameObject collect in collectableObjects)
            {
                // enable UI target
                GameObjectManager.setGameObjectState(collect.GetComponent<LinkedWith>().link, true);
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
                // disable in-game source
                GameObjectManager.setGameObjectState(collect, false);
                // particular case of collecting Intro_scroll game object => show HUD "A"
                if (collect.name == "Intro_Scroll")
                    GameObjectManager.setGameObjectState(pressA.First(), true);
            }
        }
    }
}