using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class Select : FSystem {

    private Family objects = FamilyManager.getFamily(new AnyOfTags("Object", "Plank", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable)));
    private Family tObjects = FamilyManager.getFamily(new AnyOfTags("Object", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable), typeof(Takable)));

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
        GameObject focused = null; //slected or mouse over object
        bool selected = false;

        foreach(GameObject go in objects)
        {
            foreach(Transform child in go.transform)
            {
                if(child.gameObject.tag == "MouseOver" && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    go.GetComponent<Selectable>().focused = false;
                }
            }
            if (go.GetComponent<Selectable>().isSelected)
            {
                focused = go;
                selected = true;
            }
        }

        if (!selected)
        {
            if (Takable.objectTaken)
            {
                foreach(GameObject go in tObjects)
                {
                    if (go.GetComponent<Takable>().taken)
                    {
                        focused = go;
                        break;
                    }
                }
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
                {
                    if (hit.transform.gameObject.GetComponent<Selectable>())
                    {
                        focused = hit.transform.gameObject;
                        focused.GetComponent<Selectable>().focused = true;
                    }
                    else if (hit.transform.parent.gameObject.GetComponent<Selectable>())
                    {
                        focused = hit.transform.parent.gameObject;
                        focused.GetComponent<Selectable>().focused = true;
                    }
                }
            }
        }

        if (focused)
        {
            if (Input.GetMouseButtonDown(0) && !Takable.objectTaken)
            {
                focused.GetComponent<Selectable>().isSelected = true;
                Selectable.selected = true;
            }
            if(!((focused.tag == "Plank" || focused.tag == "Box") && focused.GetComponent<Selectable>().isSelected))
            {
                foreach (Transform child in focused.transform)
                {
                    if (child.gameObject.tag == "MouseOver" && !child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}