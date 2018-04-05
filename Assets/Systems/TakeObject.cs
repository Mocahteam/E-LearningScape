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
        /*if (initialiseView)
        {
            player.First().GetComponent<FirstPersonController>().m_MouseLook.MinimumX = -90;
            player.First().GetComponent<FirstPersonController>().m_MouseLook.MaximumX = 90;
            initialiseView = false;
        }

        if (moveMirrorToPlank)
        {
            mirror.First().transform.position = Vector3.MoveTowards(mirror.First().transform.position, objPos, 0.05f);
            if(mirror.First().transform.position == objPos)
            {
                mirror.First().GetComponent<Rigidbody>().isKinematic = false;
                moveMirrorToPlank = false;
                Takable.mirrorOnPlank = true;
            }
        }
        if(Takable.mirrorOnPlank && (mirror.First().transform.hasChanged || plankE09.First().transform.hasChanged))
        {
            objPos = plankE09.First().transform.position + Vector3.up * (0.1f + mirror.First().GetComponentInChildren<MirrorReflectionScript>().gameObject.transform.localScale.y / 2 + tmpGO.transform.localScale.y / 2);
            if ((mirror.First().transform.position - objPos).magnitude > 0.15f)
            {
                Takable.mirrorOnPlank = false;
            }
        }

        //respawn objects that fall under the room
		int nbTakable = takables.Count;
		for(int i = 0; i < nbTakable; i++)
        {
			forGO = takables.getAt (i);
			if(forGO.transform.position.y < forGO.transform.parent.transform.position.y-1)
            {
				forGO.transform.position = forGO.transform.parent.transform.position + Vector3.up*3;
            }
        }

        if (!Selectable.selected && !CollectableGO.onInventory)   //if there is not selected object and inventory isn't opened
        {
            if (Takable.objectTaken)    //if an object is taken
            {
				for(int i = 0; i < nbTakable; i++)
                {
					forGO = tObjects.getAt (i);
					if (forGO.GetComponent<Takable>().taken)   //find the taken object
                    {
						if (forGO.tag == "TableE05")
                        {
                            //Camera.main.transform.localRotation = Quaternion.Euler(90,0,0);
                            player.First().transform.position += Vector3.up * (onTableHeight - player.First().transform.position.y);
							forGO.transform.position = player.First().transform.position + Vector3.down*2;    //move the object under the player
							forGO.transform.rotation = Quaternion.Euler(0, player.First().transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                        }
                        else
                        {
                            Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                            v.Normalize();
							forGO.transform.position = Camera.main.transform.position + v * (forGO.transform.localScale.y + 1.5f);    //move the object in front of the camera
							if (forGO.GetComponent<MirrorScript>())
                            {
								forGO.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                            }
                            else
                            {
								forGO.transform.rotation = Quaternion.Euler(10, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                            }
                        }
                        if (Input.GetMouseButtonDown(1)) //if right click, release the object
                        {
							forGO.GetComponent<Takable>().taken = false;
							forGO.GetComponent<Rigidbody>().isKinematic = false;
                            Takable.objectTaken = false;
							if (forGO.tag == "Box")    //when box is released, balls are no more kinematic 
                            {
                                foreach (GameObject ball in balls)
                                {
                                    ball.GetComponent<Rigidbody>().isKinematic = false;
                                }
                            }
							else if(forGO.tag == "TableE05")
                            {
								player.First().transform.position = forGO.transform.position - forGO.transform.forward * 1.5f;
                                initialiseView = true;
                            }
							else if (forGO.GetComponent<MirrorScript>())
                            {
                                tmpGO = plankE09.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.gameObject;
								if(forGO.transform.position.x < tmpGO.transform.position.x + tmpGO.transform.localScale.x/2 && forGO.transform.position.x > tmpGO.transform.position.x - tmpGO.transform.localScale.x / 2 && forGO.transform.position.z < tmpGO.transform.position.z + tmpGO.transform.localScale.z / 2 && forGO.transform.position.z > tmpGO.transform.position.z - tmpGO.transform.localScale.z / 2 && forGO.transform.position.y > tmpGO.transform.position.y)
                                {
                                    objPos = plankE09.First().transform.position + Vector3.up * (0.1f + mirror.First().GetComponentInChildren<MirrorReflectionScript>().gameObject.transform.localScale.y/2 + tmpGO.transform.localScale.y / 2);
                                    mirror.First().GetComponent<Rigidbody>().isKinematic = true;
                                    moveMirrorToPlank = true;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else    //if there is no taken object
            {
				for(int i = 0; i < nbTakable; i++)
                {
					forGO = tObjects.getAt (i);
                    //if right click on a focused (but not selected) object, take it
					if (forGO.GetComponent<Takable>().focused && Input.GetMouseButtonDown(1))
                    {
						forGO.GetComponent<Takable>().taken = true;
						forGO.GetComponent<Rigidbody>().isKinematic = true;
                        Takable.objectTaken = true;
						if (forGO.tag == "Box")
                        {
							int nbBalls = balls.Count;
							for(int j = 0; j < nbBalls; j++)
                            {
								balls.getAt(j).GetComponent<Rigidbody>().isKinematic = true;
                            }
                        }
						else if(forGO.tag == "TableE05")
                        {
							player.First().transform.forward = forGO.transform.forward;
							player.First().transform.position = forGO.transform.position + Vector3.up * 2;
                            onTableHeight = player.First().transform.position.y;
                            player.First().GetComponent<FirstPersonController>().m_MouseLook.MinimumX = 90;
                        }
                        break;
                    }
                }
            }
        }*/

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