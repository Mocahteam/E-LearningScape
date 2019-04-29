using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class MoveInFrontOf : FSystem {

    // all focused GO (only once)
    private Family f_focused = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Highlighted)));
    // ReadyToWork component is added dynamically by this system and removed by systems interested by it (LockResolver, PlankManager...)
    private Family f_readyToWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));
    private Family f_forcedMove = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(ForceMove)));

    private Family f_quitEnigma = FamilyManager.getFamily(new AnyOfTags("QuitEnigma"));

	private Family f_player = FamilyManager.getFamily (new AnyOfTags ("Player"), new AllOfComponents(typeof(SwitchPerso))); // rajouter allofcomponent SwitchPerso f_player.First().getComponent<SwitchPerso>().bool

    //information for animations
    private float speed;
    private float speedRotation;
    private float angleRotation;
    private float dist = -1;
    private Vector3 camNewDir;
    private Vector3 newDir;
    private Vector3 playerLocalScale;

    private bool moveInFrontOf = false;
    private Vector3 targetPos;

    private GameObject emulateClickOn = null;

    private GameObject focusedGO;

    public static MoveInFrontOf instance;

    public MoveInFrontOf()
    {
        if (Application.isPlaying)
        {
            f_forcedMove.addEntryCallback(onForceMove);

            f_readyToWork.addExitCallback(onWorkFinished);
        }
        instance = this;
    }

    private void onForceMove(GameObject go)
    {
        GameObjectManager.removeComponent<ForceMove>(go);
        emulateClickOn = go;
    }

    private void onWorkFinished(int instanceId)
    {
        // if family is empty we can resume stopped systems
        if (f_readyToWork.Count == 0)
        {
            // reset intial player scale
            f_player.First().transform.localScale = playerLocalScale;
            //reset player camera
            f_player.First().GetComponent<SwitchPerso>().forceUpdate();
            // enable systems
            MovingSystem.instance.Pause = false;
            Highlighter.instance.Pause = false;
            MirrorSystem.instance.Pause = false;
            DreamFragmentCollecting.instance.Pause = false;
            ToggleObject.instance.Pause = false;
            CollectObject.instance.Pause = false;
            // pause unused system
            LockResolver.instance.Pause = true;
            PlankAndWireManager.instance.Pause = true;
            BallBoxManager.instance.Pause = true;
            LoginManager.instance.Pause = true;
            SatchelManager.instance.Pause = true;
            PlankAndMirrorManager.instance.Pause = true;
            WhiteBoardManager.instance.Pause = true;
            // hide help overlay
            GameObjectManager.setGameObjectState(f_quitEnigma.First(), false);


        }
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        speed = 8f * Time.deltaTime;

        if (!moveInFrontOf && (Input.GetMouseButtonDown(0) || emulateClickOn != null))
        {
			if (Input.GetMouseButtonDown (0)) {
				//If player is in third person view so when he click on object player will be in First person cam view
				f_player.First ().GetComponent<SwitchPerso> ().ThirdCamera.enabled = false;
				f_player.First ().GetComponent<SwitchPerso> ().FirstCamera.enabled = true;
				focusedGO = f_focused.First ();

			}
            else
            {
                focusedGO = emulateClickOn;
                emulateClickOn = null;
            }

            if (focusedGO)
            {
                GameObjectManager.addComponent<ActionPerformedForLRS>(focusedGO, new { verb = "accessed", objectType = "interactable", objectName = focusedGO.name });

                // pause unuse systems
                MovingSystem.instance.Pause = true;
                Highlighter.instance.Pause = true;
                DreamFragmentCollecting.instance.Pause = true;
                ToggleObject.instance.Pause = true;
                CollectObject.instance.Pause = true;

                // save player scale (crouch or not) in order to reset it when player exit the focused GameObject
                playerLocalScale = f_player.First().transform.localScale;

                // be sure the player standing in front of the selected game object
                f_player.First().transform.localScale = Vector3.one;
                // compute target position and orientation in front of the focused GameObject
                Selectable selectable = focusedGO.GetComponent<Selectable>();
                targetPos = new Vector3(focusedGO.transform.position.x + selectable.standingPosDelta.x, focusedGO.transform.position.y + selectable.standingPosDelta.y, focusedGO.transform.position.z + selectable.standingPosDelta.z);
                // compute distance between the player and the focused GO and compute speed rotation
                dist = (targetPos - f_player.First().transform.position).magnitude;
                camNewDir = selectable.standingOrientation;
                angleRotation = Vector3.Angle(Camera.main.transform.forward, camNewDir);
                moveInFrontOf = true; // start animation to move the player in front of the selected GameObject

            }
        }

        // Do we have to process animation to move the player in front of the selected GO ?
        if (moveInFrontOf)
        {
            // move the player in front of the selected GO
            f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, targetPos, speed);
            speedRotation = angleRotation * speed / dist;
            newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, Mathf.Deg2Rad * speedRotation, 0);
            Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
            // Check if the animation is finished
            if (Vector3.Angle(Camera.main.transform.forward, camNewDir) < 0.5f && f_player.First().transform.position == targetPos)
            {
                // Correct position
                f_player.First().transform.position = targetPos;
                // Correct the rotation
                newDir = Vector3.RotateTowards(f_player.First().transform.forward, camNewDir, 360, 0);
                f_player.First().transform.rotation = Quaternion.LookRotation(newDir);
                newDir = Vector3.RotateTowards(Camera.main.transform.forward, camNewDir, 360, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                // animation is over
                moveInFrontOf = false;
                // Add ReadyToWork component
                GameObjectManager.addComponent<ReadyToWork>(focusedGO);
                // show help overlay
                GameObjectManager.setGameObjectState(f_quitEnigma.First(), true);
            }
        }
    }
}