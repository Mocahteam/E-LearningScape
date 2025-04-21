using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class PlankAndMirrorManager : FSystem {

    // this system manage the plank and the wire

    //all selectable objects
    private Family f_selectedPlank = FamilyManager.getFamily(new AnyOfTags("PlankE09"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closePlank_1 = FamilyManager.getFamily(new AnyOfTags("PlankE09", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_closePlank_2 = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(HUD_TransparentOnMove)));
    private Family f_arrows = FamilyManager.getFamily(new AnyOfTags("PlankE09"), new AllOfComponents(typeof(AnimatedSprites), typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));

    public float height;

    //plank
    private GameObject plankRotation;
    public GameObject plank;
    public GameObject mirrorOnPlank;

    private bool discovered = false; // true if the plank on the floor was discovered (selected at least once)

    public static PlankAndMirrorManager instance;

    public PlankAndMirrorManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_selectedPlank.addEntryCallback(onReadyToWorkOnPlank);
        plankRotation = plank.transform.Find("RotationPlank").gameObject;
    }

    // Rotate Plank
    private void rotatePlank(int way)
    {
        plankRotation.transform.Rotate(Vector3.up, way * 50 * Time.deltaTime);
    }

    private void onReadyToWorkOnPlank(GameObject go)
    {
        plank.GetComponent<Animator>().SetTrigger("Reveal");

        discovered = true;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(plank, new { name = "turnOn", performedBy = "player" });

        GameObjectManager.addComponent<ActionPerformedForLRS>(go, new
        {
            verb = "accessed",
            objectType = "mirroredPlank"
        });
    }

    // return true if UI with name "name" is selected into inventory
    private GameObject isSelected(string name)
    {
        foreach (GameObject go in f_itemSelected)
            if (go.name == name)
                return go;
        return null;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed andIAR is closed (because Escape close IAR)
        if (((f_closePlank_1.Count == 0 && f_closePlank_2.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)))
            ExitPlank();

        if (f_selectedPlank.Count > 0)
        {
            if (Input.GetAxis("Horizontal") < -0.2)
                rotatePlank(1);
            if (Input.GetAxis("Horizontal") > 0.2)
                rotatePlank(-1);
            if (Input.GetButton("Fire1") && f_arrows.Count > 0)
            {
                if (f_arrows.First().name == "Left")
                    rotatePlank(1);
                else
                    rotatePlank(-1);
            }

            if (isSelected("Mirror"))
            {
                // remove mirror from inventory
                GameObjectManager.setGameObjectState(isSelected("Mirror"), false);
                GameObjectManager.setGameObjectState(isSelected("Mirror").GetComponent<HUDItemSelected>().hudGO, false);

                PutMirrorOnPlank();

                GameObjectManager.addComponent<ActionPerformed>(plank, new { name = "perform", performedBy = "system" });
                GameObjectManager.addComponent<ActionPerformedForLRS>(plank, new
                {
                    verb = "placed",
                    objectType = "mirror"
                });
            }
        }
	}

    private void ExitPlank()
    {

        plank.GetComponent<Animator>().SetTrigger("OnGround");
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(plank);

        GameObjectManager.addComponent<ActionPerformed>(plank, new { name = "turnOff", performedBy = "player" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(plank, new 
        {
            verb = "exited",
            objectType = "mirroredPlank"
        });

        // Pause this system
        instance.Pause = true;
    }

    public void PutMirrorOnPlank()
    {
        // show ingame mirror on plank
        GameObjectManager.setGameObjectState(mirrorOnPlank, true);
    }

    public void SetPlankDiscovered(bool state)
    {
        discovered = state;
        if (discovered)
            plank.GetComponent<Animator>().SetTrigger("OnGround");

    }

    public bool GetPlankDiscovered()
    {
        return discovered;
    }

    public bool IsMirrorOnPlank()
    {
        return mirrorOnPlank.activeSelf;
    }
}