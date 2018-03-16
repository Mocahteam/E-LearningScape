using UnityEngine;
using FYFY;

public class TakeObject : FSystem {

    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AnyOfTags("Object", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable), typeof(Takable)));
    //enigma03's balls
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));

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
        //respawn objects that fall under the room
        foreach(GameObject go in tObjects)
        {
            if(go.transform.position.y < go.transform.parent.transform.position.y-1)
            {
                go.transform.position = go.transform.parent.transform.position + Vector3.up*3;
            }
        }

        if (!Selectable.selected)   //if there is not selected object
        {
            if (Takable.objectTaken)    //if an object is taken
            {
                foreach (GameObject go in tObjects)
                {
                    if (go.GetComponent<Takable>().taken)   //find the taken object
                    {
                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                        v.Normalize();
                        go.transform.position = Camera.main.transform.position + v * (go.transform.localScale.y + 1.5f);    //move the object in front of the camera
                        go.transform.rotation = Quaternion.Euler(10, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
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
                            break;
                        }
                    }
                }
            }
            else    //is there is not taken object
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
                        break;
                    }
                }
            }
        }
	}
}