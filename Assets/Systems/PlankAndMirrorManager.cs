using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class PlankAndMirrorManager : FSystem {

    // this system manage the plank and the wire

    //all selectable objects
    private Family f_selectedPlank = FamilyManager.getFamily(new AnyOfTags("PlankE09"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closePlank_1 = FamilyManager.getFamily(new AnyOfTags("PlankE09", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_closePlank_2 = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(HUD_TransparentOnMove)));
    private Family f_arrows = FamilyManager.getFamily(new AnyOfTags("PlankE09"), new AllOfComponents(typeof(AnimatedSprites), typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));
    private Family f_mirror = FamilyManager.getFamily(new AllOfComponents(typeof(MirrorScript)), new NoneOfComponents(typeof(LinkedWith)));

    private float speed;
    private float dist = 1.5f;

    //plank
    private bool movePlank = false;               //true during the animation to move the player above the table
    private Vector3 plankTargetPos;
    private GameObject selectedPlank;
    private GameObject plankRotation;
    private GameObject mirrorOnPlank;
    private GameObject UI;

    private bool discovered = false; // true if the plank on the floor was discovered (selected at least once)
    private bool prepareClosing = false;

    private int rotationCount = 0;

    public static PlankAndMirrorManager instance;

    public PlankAndMirrorManager()
    {
        if (Application.isPlaying)
        {
            f_selectedPlank.addEntryCallback(onReadyToWorkOnPlank);
            mirrorOnPlank = f_mirror.First();
        }
        instance = this;
    }

    // Rotate Plank
    private void rotatePlank(int way)
    {
        plankRotation.transform.Rotate(Vector3.up, way * 50 * Time.deltaTime);
        rotationCount += way;
    }

    private void onReadyToWorkOnPlank(GameObject go)
    {
        selectedPlank = go;
        plankTargetPos = go.transform.position + Vector3.up*1.5f;
        movePlank = true;
        prepareClosing = false;
        plankRotation = go.transform.GetChild(0).gameObject;
        UI = go.transform.GetChild(2).gameObject;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(selectedPlank, new { name = "turnOn", performedBy = "player" });

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
        speed = 4f * Time.deltaTime;

        if (selectedPlank)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and paper is out of the bag and IAR is closed (because Escape close IAR)
            if (((f_closePlank_1.Count == 0 && f_closePlank_2.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && !movePlank)
            {
                // ask to exit plank
                prepareClosing = true;
                plankTargetPos = selectedPlank.transform.position - Vector3.up * dist;
                movePlank = true;
                // disable UI
                GameObjectManager.setGameObjectState(UI, false);
            }

            if (movePlank)
            {
                //animation to move the plank in front of the player
                selectedPlank.transform.position = Vector3.MoveTowards(selectedPlank.transform.position, plankTargetPos, speed);
                float rotation = 20f;
                if (!discovered)
                    rotation += 180; // flip the first time the plank due to face on ground
                if (prepareClosing)
                    rotation *= -1;
                selectedPlank.transform.Rotate(-Vector3.right, rotation / (dist/speed));
                mirrorOnPlank.transform.Rotate(-Vector3.right, rotation / (dist / speed));
                //when the plank arrives
                if (selectedPlank.transform.position == plankTargetPos)
                {
                    // correct rotation
                    if (prepareClosing)
                    {
                        selectedPlank.transform.rotation = Quaternion.Euler(0, 0, 0);
                        mirrorOnPlank.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        plankRotation.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                    else
                    {
                        selectedPlank.transform.rotation = Quaternion.Euler(-20, 0, 0);
                        mirrorOnPlank.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        // enable UI
                        GameObjectManager.setGameObjectState(UI, true);
                    }
                    movePlank = false;
                    discovered = true;
                }
            }
            else
            {
                if (prepareClosing)
                {
                    ExitPlank();
                }
            }

            if (isSelected("Mirror"))
            {
                // remove mirror from inventory
                GameObjectManager.setGameObjectState(isSelected("Mirror"), false);
                GameObjectManager.setGameObjectState(isSelected("Mirror").GetComponent<HUDItemSelected>().hudGO, false);

                PutMirrorOnPlank();

                GameObjectManager.addComponent<ActionPerformed>(selectedPlank, new { name = "perform", performedBy = "system" });
                GameObjectManager.addComponent<ActionPerformedForLRS>(selectedPlank, new 
                {
                    verb = "placed",
                    objectType = "mirror"
                });
            }

            if (!movePlank)
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
            }
        }
	}

    private void ExitPlank()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedPlank);

        GameObjectManager.addComponent<ActionPerformed>(selectedPlank, new { name = "turnOff", performedBy = "player" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedPlank, new 
        {
            verb = "exited",
            objectType = "mirroredPlank"
        });

        selectedPlank = null;

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
            mirrorOnPlank.transform.parent.rotation = Quaternion.Euler(0, 0, 0);

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