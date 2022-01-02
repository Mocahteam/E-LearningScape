using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MovingSystem_UIMode : FSystem {
    private GameObject fpsController;
    private GameObject fpsCamera;
    private GameObject tpsCamera;
    private Quaternion initTpsRotation;
    private Vector3 initTpsPos;
    private Vector3 previousPosition;

    public bool lockSystem;

    public float tempo = 0;

    public static MovingSystem_UIMode instance;

    public MovingSystem_UIMode()
    {
        if (Application.isPlaying)
        {
            fpsController = GameObject.Find("FPSController");
            fpsCamera = GameObject.Find("FirstPersonCharacter");
            tpsCamera = GameObject.Find("ThirdCamera");
            initTpsRotation = tpsCamera.transform.localRotation;
            initTpsPos = tpsCamera.transform.localPosition;
            previousPosition = fpsController.transform.localPosition;

            instance = this;
        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
    }

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame)
    {
        fpsController.transform.localEulerAngles = new Vector3(0, (float)(System.Math.Round(fpsController.transform.localEulerAngles.y / 45) * 45), 0); // set angle on the nearest multiple of 45
        fpsCamera.transform.localEulerAngles = new Vector3(0, 0, 0);

        tpsCamera.transform.localRotation = initTpsRotation;
        tpsCamera.transform.localPosition = initTpsPos;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        tempo += Time.deltaTime;
        if (tempo > 0.25f)
        {
            if (Input.GetAxis("Horizontal") < -0.2 && !SceneManager.GetActiveScene().name.Contains("Tuto"))
            {
                turn(-45);
                tempo = 0;
            }
            if (Input.GetAxis("Horizontal") > 0.2 && !SceneManager.GetActiveScene().name.Contains("Tuto"))
            {
                turn(45);
                tempo = 0;
            }
            if (Input.GetButtonDown("Fire2"))
            {
                MovingSystem_FPSMode.instance.ChangePose(false);
                moveOnTheFloor();
                tempo = 0;
            }
            if (Input.GetAxis("Vertical") > 0.2 && !SceneManager.GetActiveScene().name.Contains("Tuto"))
            {
                moveForward(2);
                tempo = 0;
            }
            if (Input.GetAxis("Vertical") < -0.2 && !SceneManager.GetActiveScene().name.Contains("Tuto"))
            {
                moveForward(-2);
                tempo = 0;
            }
        }
    }

    public void turn(float angle)
    {
        fpsController.transform.localEulerAngles = new Vector3(fpsController.transform.localEulerAngles.x, (fpsController.transform.localEulerAngles.y + angle) % 360, fpsController.transform.localEulerAngles.z);
    }

    public void moveForward(float distance)
    {
        RaycastHit hit;
        Vector3 direction;
        if (distance >= 0)
            direction = Camera.main.transform.forward;
        else
        {
            direction = -Camera.main.transform.forward;
            distance = -distance;
        }
        // move player in apropiate direction if no collider blocking
        if (!Physics.Raycast(fpsCamera.transform.position, direction, out hit, distance, ~(1 << 2), QueryTriggerInteraction.Ignore))
        {
            fpsController.transform.localPosition = new Vector3(fpsController.transform.localPosition.x + direction.x * distance, fpsController.transform.localPosition.y + direction.y * distance, fpsController.transform.localPosition.z + direction.z * distance);
            // find the ground and position player on the floor
            moveOnTheFloor();
            if (Vector3.Distance(previousPosition, fpsController.transform.localPosition) > 7)
            {
                GameObjectManager.addComponent<ActionPerformedForLRS>(fpsController.gameObject, new
                {
                    verb = "moved",
                    objectType = "avatar",
                    objectName = "player",
                    activityExtensions = new Dictionary<string, string>() { { "position", fpsController.transform.position.ToString("G4") } }
                });
                previousPosition = fpsController.transform.localPosition;
            }
        }
    }

    public void moveOnTheFloor()
    {
        RaycastHit hit;
        Physics.Raycast(fpsCamera.transform.position, -Camera.main.transform.up, out hit, Mathf.Infinity, 1 << 14 | 1 << 4, QueryTriggerInteraction.Ignore);
        fpsController.transform.localPosition = hit.point + (Vector3.up*(MovingSystem_FPSMode.instance.crouching? 0.2f : 1f));
    }

}