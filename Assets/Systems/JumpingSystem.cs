﻿using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class JumpingSystem : FSystem {
    private GameObject pinTarget;
    private GameObject fpsController;
    private Vector3 higherPosition;
    private Vector3 CameraPlanarPosition;

    private Family f_dreamFragmentUI = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AllOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD, PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_highlighted = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)));
    private Family f_readyToWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"), new AllOfComponents(typeof(SwitchPerso)));
    private Family f_OutOfFirstRoom = FamilyManager.getFamily(new AllOfComponents(typeof(TriggerSensitive3D), typeof(LinkedWith)));

    public bool lockSystem;

    public static JumpingSystem instance;

    public JumpingSystem()
    {
        if (Application.isPlaying)
        {
            pinTarget = GameObject.Find("PinTarget");
            pinTarget.SetActive(false);
            lockSystem = true; // system is unlocked if "ToggleTarget" input is pressed (key "k")
            fpsController = GameObject.Find("FPSController");

            f_highlighted.addEntryCallback(onHighlight);
            f_highlighted.addExitCallback(onUnhighlight);
            f_readyToWork.addEntryCallback(onHighlight);
            f_readyToWork.addExitCallback(onUnhighlight);

            instance = this;
        }
    }

    private void onHighlight (GameObject go)
    {
        this.Pause = true;
    }

    private void onUnhighlight(int instanceId)
    {
        if (f_highlighted.Count == 0)
            this.Pause = false;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
        GameObjectManager.setGameObjectState(pinTarget, false);
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame)
    {
        if (!lockSystem)
            GameObjectManager.setGameObjectState(pinTarget, true);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        RaycastHit hit;
        int layerMask = (1 << 14);
        // Launch a ray to hit clother colider
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, ~(1 << 2), QueryTriggerInteraction.Ignore))
        {
            //Launch another ray to hit the ground
            higherPosition = hit.point + Vector3.up * 10;
            if (Physics.Raycast(higherPosition, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                pinTarget.transform.position = hit.point;
                // get planar camera position
                CameraPlanarPosition = Camera.main.transform.position;
                CameraPlanarPosition.y = pinTarget.transform.GetChild(1).transform.position.y;
                // rotate pin to the camera position
                pinTarget.transform.GetChild(1).transform.LookAt(CameraPlanarPosition);

                if (Input.GetButtonDown("Fire1") && f_highlighted.Count == 0 && f_dreamFragmentUI.Count == 0 && pinTarget.activeInHierarchy)
                {
                    fpsController.transform.position = hit.point + Vector3.up * 2 - Camera.main.transform.forward;
                    GameObjectManager.addComponent<PlaySound>(fpsController, new { id = 18 }); // id refer to FPSController AudioBank
                }
            }
        }

        if (Input.GetButtonDown("ToggleTarget"))
        {
            pinTarget.SetActive(!pinTarget.activeSelf);
            lockSystem = !pinTarget.activeSelf;

            SwitchPerso sp = f_player.First().GetComponent<SwitchPerso>();
            sp.fpsCam = true;
            sp.forceUpdate();

            // If player switch to assisted mouse navigation disable HUD warnings for moving
            foreach (LinkedWith link in f_OutOfFirstRoom.First().GetComponents<LinkedWith>())
                GameObjectManager.setGameObjectState(link.link, false);
        }
    }
}