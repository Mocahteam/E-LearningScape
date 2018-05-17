using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using UnityStandardAssets.Characters.FirstPerson;

public class TakeObject : FSystem {
    // Both of the Vive Controllers
    private Family controllers = FamilyManager.getFamily(new AllOfComponents(typeof(Grabber)));
    //all takable objects
    private Family takables = FamilyManager.getFamily(new AllOfComponents(typeof(Takable)));
    //enigma03's balls
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family plankE09 = FamilyManager.getFamily(new AnyOfTags("PlankE09"));
    private Family mirror = FamilyManager.getFamily(new AllOfComponents(typeof(MirrorScript)));

    private float onTableHeight;
    private GameObject tmpGO;
    private bool moveMirrorToPlank = false;
    private Vector3 objPos;

    private bool initialiseView = false;

	private GameObject forGO;

    public TakeObject()
    {
        // For each controller
        foreach (GameObject c in controllers)
        {
            Grabber g = c.GetComponent<Grabber>();
            // Get the tracked object (device)
            g.trackedObj = g.GetComponent<SteamVR_TrackedObject>();
        }

        //at the beginning of the game, all taken object are not kinematic
        foreach (GameObject go in takables)
        {
            //go.GetComponent<Rigidbody>().isKinematic = false;
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
	protected override void onProcess(int familiesUpdateCount) {
        // Make the outline disappear
        foreach (GameObject go in takables)
        {
            foreach (Transform child in go.transform)
            {
                if (child.gameObject.tag == "MouseOver")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        foreach (GameObject c in controllers)
        {
            Grabber g = c.GetComponent<Grabber>();
            g.collidingObject = null;
            Triggered3D t3d = c.GetComponent<Triggered3D>();
            if (!t3d) continue;
            foreach (GameObject target in t3d.Targets)
            {
                Takable t = target.GetComponent<Takable>();
                if (!t) continue;
                g.collidingObject = target;
                break; // Only the first
            }

            // Make the outline appear
            if(g.collidingObject)
            {
                foreach (Transform child in g.collidingObject.transform)
                {
                    if (child.gameObject.tag == "MouseOver")
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }

        foreach (GameObject c in controllers)
        {
            Grabber g = c.GetComponent<Grabber>();

            SteamVR_Controller.Device controller = SteamVR_Controller.Input((int)g.trackedObj.index);

            // If the user is pressing the trigger
            if (g.collidingObject && controller.GetHairTriggerDown())
            {
                GrabObject(g);
            }

            // If the user has released the trigger
            if (controller.GetHairTriggerUp())
            {
                ReleaseObject(g);
                g.objectInHand = null;
            }
        }

	}

    private void GrabObject(Grabber g)
    {
        // Move the collidingObject inside the hand
        g.objectInHand = g.collidingObject;
        g.collidingObject = null;
        // Joint
        var joint = AddFixedJoint(g);
        joint.connectedBody = g.objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint(Grabber g)
    {
        FixedJoint fx = g.gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject(Grabber g)
    {
        // Check the joint
        if(g.GetComponent<FixedJoint>())
        {
            // Remove the connected body
            g.GetComponent<FixedJoint>().connectedBody = null;
            GameObject.Destroy(g.GetComponent<FixedJoint>());
            // Throw !
            SteamVR_Controller.Device controller = SteamVR_Controller.Input((int)g.trackedObj.index);
            g.objectInHand.GetComponent<Rigidbody>().velocity = controller.velocity;
            g.objectInHand.GetComponent<Rigidbody>().angularVelocity = controller.angularVelocity;
        }
    }
}
