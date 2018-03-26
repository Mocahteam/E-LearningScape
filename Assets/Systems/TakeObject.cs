using UnityEngine;
using FYFY;
using UnityStandardAssets.Characters.FirstPerson;

public class TakeObject : FSystem {

    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Takable)));
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

    public TakeObject()
    {
        //at the beginning of the game, all taken object are not kinematic
        foreach(GameObject go in tObjects)
        {
            go.GetComponent<Rigidbody>().isKinematic = false;
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
        foreach(GameObject go in tObjects)
        {
            if(go.transform.position.y < go.transform.parent.transform.position.y-1)
            {
                go.transform.position = go.transform.parent.transform.position + Vector3.up*3;
            }
        }

        if (!Selectable.selected && !CollectableGO.onInventory)   //if there is no selected object and inventory isn't opened
        {
            if (Takable.objectTaken)    //if an object is taken
            {
                foreach (GameObject go in tObjects)
                {
                    if (go.GetComponent<Takable>().taken)   //find the taken object
                    {
                        if(go.tag == "TableE05")
                        {
                            //Camera.main.transform.localRotation = Quaternion.Euler(90,0,0);
                            player.First().transform.position += Vector3.up * (onTableHeight - player.First().transform.position.y);
                            go.transform.position = player.First().transform.position + Vector3.down*2;    //move the object under the player
                            go.transform.rotation = Quaternion.Euler(0, player.First().transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                        }
                        else
                        {
                            Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                            v.Normalize();
                            go.transform.position = Camera.main.transform.position + v * (go.transform.localScale.y + 1.5f);    //move the object in front of the camera
                            if (go.GetComponent<MirrorScript>())
                            {
                                go.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                            }
                            else
                            {
                                go.transform.rotation = Quaternion.Euler(10, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                            }
                        }
                        if (Input.GetMouseButtonDown(1)) //if right click, release the object
                        {
                            go.GetComponent<Takable>().taken = false;
                            go.GetComponent<Rigidbody>().isKinematic = false;
                            Takable.objectTaken = false;
                            if (go.tag == "Box")    //when box is released, balls are no more kinematic 
                            {
                                foreach (GameObject ball in balls)
                                {
                                    ball.GetComponent<Rigidbody>().isKinematic = false;
                                }
                            }
                            else if(go.tag == "TableE05")
                            {
                                player.First().transform.position = go.transform.position - go.transform.forward * 1.5f;
                                player.First().GetComponent<FirstPersonController>().m_MouseLook.MinimumX = 0;
                                player.First().GetComponent<FirstPersonController>().m_MouseLook.MaximumX = 0;
                                initialiseView = true;
                            }
                            else if (go.GetComponent<MirrorScript>())
                            {
                                tmpGO = plankE09.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.gameObject;
                                if(go.transform.position.x < tmpGO.transform.position.x + tmpGO.transform.localScale.x/2 && go.transform.position.x > tmpGO.transform.position.x - tmpGO.transform.localScale.x / 2 && go.transform.position.z < tmpGO.transform.position.z + tmpGO.transform.localScale.z / 2 && go.transform.position.z > tmpGO.transform.position.z - tmpGO.transform.localScale.z / 2 && go.transform.position.y > tmpGO.transform.position.y)
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
                foreach (GameObject go in tObjects)
                {
                    //if right click on a focused (but not selected) object, take it
                    if (go.GetComponent<Selectable>().focused && Input.GetMouseButtonDown(1))
                    {
                        go.GetComponent<Takable>().taken = true;
                        go.GetComponent<Rigidbody>().isKinematic = true;
                        Takable.objectTaken = true;
                        if (go.tag == "Box")
                        {
                            foreach (GameObject ball in balls)
                            {
                                ball.GetComponent<Rigidbody>().isKinematic = true;
                            }
                        }
                        else if(go.tag == "TableE05")
                        {
                            player.First().transform.forward = go.transform.forward;
                            player.First().transform.position = go.transform.position + Vector3.up * 2;
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