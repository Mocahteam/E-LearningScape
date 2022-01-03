using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using FYFY_plugins.PointerManager;

public class MovingSystem_TeleportMode : FSystem {
    private GameObject pinTarget;
    private GameObject fpsController;
    private Vector3 CameraPlanarPosition;

    private Family f_dreamFragmentUI = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AllOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD, PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_CrouchHint = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites), typeof(PointerOver), typeof(LinkedWith), typeof(BoxCollider)));
    private Family f_OutOfFirstRoom = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(LinkedWith)));

    public bool lockSystem;

    public static MovingSystem_TeleportMode instance;

    public MovingSystem_TeleportMode()
    {
        if (Application.isPlaying)
        {
            pinTarget = GameObject.Find("PinTarget");
            pinTarget.SetActive(false);

            fpsController = GameObject.Find("FPSController");

            if (!SceneManager.GetActiveScene().name.Contains("Tuto"))
            {
                f_CrouchHint.addEntryCallback(disableHUDWarning);
                f_OutOfFirstRoom.addEntryCallback(disableHUDWarning);
            }

            instance = this;
        }
    }

    private void disableHUDWarning(GameObject go)
    {
        foreach (LinkedWith link in go.GetComponents<LinkedWith>())
            GameObjectManager.setGameObjectState(link.link, false);
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
        
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        RaycastHit hit;
        // Launch a ray to hit clother colider (exclude layer with id 2)
        if (!lockSystem && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, ~(1 << 2), QueryTriggerInteraction.Ignore))
        {
            // check if this collider is the ground or the water
            if (hit.collider.gameObject.layer == 14 || hit.collider.gameObject.layer == 4)
            {
                GameObjectManager.setGameObjectState(pinTarget, true); // be sure pin is displayed
                pinTarget.transform.position = hit.point;
                // get planar camera position
                CameraPlanarPosition = Camera.main.transform.position;
                CameraPlanarPosition.y = pinTarget.transform.GetChild(1).transform.position.y;
                // rotate pin to the camera position
                pinTarget.transform.GetChild(1).transform.LookAt(CameraPlanarPosition);

                if (Input.GetButtonDown("Fire1") && f_dreamFragmentUI.Count == 0 && pinTarget.activeInHierarchy)
                {
                    fpsController.transform.position = hit.point + Vector3.up * 2;
                    GameObjectManager.addComponent<PlaySound>(fpsController, new { id = 18 }); // id refer to FPSController AudioBank
                    GameObjectManager.addComponent<ActionPerformedForLRS>(fpsController.gameObject, new
                    {
                        verb = "moved",
                        objectType = "avatar",
                        objectName = "player",
                        activityExtensions = new Dictionary<string, string>() { { "position", fpsController.transform.position.ToString("G4") } }
                    });
                }
            }
            else
                GameObjectManager.setGameObjectState(pinTarget, false); // hide pin
        }
    }
}