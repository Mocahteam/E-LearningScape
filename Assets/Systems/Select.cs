using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class Select : FSystem {

    //all selectable objects
    private Family objects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)));
    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Takable)));

    private GameObject focused;
    private bool selected = false;

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
        focused = null; //selected or mouse over object
        selected = false;   //initial value

        foreach(GameObject go in objects)
        {
            foreach(Transform child in go.transform)
            {
                //hide all gameobject's "Mouse over overlay"
                if(child.gameObject.tag == "MouseOver" && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    go.GetComponent<Selectable>().focused = false;
                }
            }

            //if the gameobject is selected, save it as focused object
            if (go.GetComponent<Selectable>().isSelected)
            {
                focused = go;
                selected = true;
            }
        }

        if (!selected) //if there is no selected objects
        {
            //if an object is taken, save it as focused object
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
                /*look for the first selectable object in the direction of the player (on the cursor)
                 * and save it as focused object
                 */
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

        if (focused)    //if there is a focused object
        {
            //if the player clicks on the object while it is not taken and inventory isn't opened, select it
            if (Input.GetMouseButtonDown(0) && !Takable.objectTaken && !CollectableGO.onInventory)
            {
                focused.GetComponent<Selectable>().isSelected = true;
                Selectable.selected = true;
            }
            //if the object isn't the plank or the box and is selected, show the mouse over overlay
            if(!((focused.tag == "Plank" || focused.tag == "Box" || focused.tag == "Bag") && focused.GetComponent<Selectable>().isSelected) && !(Takable.mirrorOnPlank && focused.GetComponent<MirrorScript>()))
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