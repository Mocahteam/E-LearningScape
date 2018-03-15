using UnityEngine;
using FYFY;

public class TakeObject : FSystem {

    private Family tObjects = FamilyManager.getFamily(new AnyOfTags("Object", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable), typeof(Takable)));
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));

    public TakeObject()
    {
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
        foreach(GameObject go in tObjects)
        {
            if(go.transform.position.y < go.transform.parent.transform.position.y-1)
            {
                go.transform.position = go.transform.parent.transform.position + Vector3.up*3;
            }
        }
        if (!Selectable.selected)
        {
            if (Takable.objectTaken)
            {
                foreach (GameObject go in tObjects)
                {
                    if (go.GetComponent<Takable>().taken)
                    {
                        Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                        v.Normalize();
                        go.transform.position = Camera.main.transform.position + v * (go.transform.localScale.y + 1.5f);
                        go.transform.rotation = Quaternion.Euler(10, Camera.main.transform.rotation.eulerAngles.y, 0);
                        if (Input.GetMouseButtonDown(1))
                        {
                            go.GetComponent<Takable>().taken = false;
                            go.GetComponent<Rigidbody>().isKinematic = false;
                            Takable.objectTaken = false;
                            if (go.tag == "Box")
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
            else
            {
                foreach (GameObject go in tObjects)
                {
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