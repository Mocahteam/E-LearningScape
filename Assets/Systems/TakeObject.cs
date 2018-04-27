using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;

public class TakeObject : FSystem {

    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Takable)));
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
        //at the beginning of the game, all takable object are not kinematic
		int nbTakable = tObjects.Count;
		for(int i = 0; i < nbTakable; i++)
        {
            if(tObjects.getAt(i).tag != "TableE05")
            {
                tObjects.getAt(i).GetComponent<Rigidbody>().isKinematic = false;
            }
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
        if (initialiseView)
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
		int nbTakable = tObjects.Count;
		for(int i = 0; i < nbTakable; i++)
        {
			forGO = tObjects.getAt (i);
			if(forGO.transform.position.y < forGO.transform.parent.transform.position.y-1)
            {
				forGO.transform.position = forGO.transform.parent.transform.position + Vector3.up*3;
            }
        }

        if (!Selectable.selected && !CollectableGO.onInventory)   //if there is no selected object and inventory isn't opened
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
        }
	}
}