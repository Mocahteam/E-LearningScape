using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class Select : FSystem {

    //all selectable objects
	private Family objects = FamilyManager.getFamily(new AnyOfComponents(typeof(Selectable), typeof(Takable), typeof(ToggleableGO)));
    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Takable)));

    private GameObject focused;
    private bool selected = false;

	private GameObject forGO;

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

		int nbObjects = objects.Count;
		for(int i = 0; i < nbObjects; i++)
        {
			forGO = objects.getAt (i);
			foreach(Transform child in forGO.transform)
            {
                //hide all gameobject's "Mouse over overlay"
                if(child.gameObject.tag == "MouseOver" && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    break;
                }
            }
			if (forGO.GetComponent<Takable>())
            {
				forGO.GetComponent<Takable>().focused = false;
			}
			if (forGO.GetComponent<ToggleableGO>())
			{
				forGO.GetComponent<ToggleableGO>().focused = false;
			}

            //if the gameobject is selected, save it as focused object
			if (forGO.GetComponent<Selectable>())
            {
				if (forGO.GetComponent<Selectable>().isSelected)
                {
					focused = forGO;
                    selected = true;
                }
            }
        }

        if (!selected) //if there is no selected objects
        {
            //if an object is taken, save it as focused object
            if (Takable.objectTaken)
            {
				int nbTakable = tObjects.Count;
				for(int i = 0; i < nbTakable; i++)
                {
					forGO = tObjects.getAt(i);
					if (forGO.GetComponent<Takable>().taken)
                    {
						focused = forGO;
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
                    if (hit.transform.gameObject.GetComponent<Takable>())
                    {
                        hit.transform.gameObject.GetComponent<Takable>().focused = true;
                        focused = hit.transform.gameObject;
                    }
                    else if (hit.transform.parent.gameObject.GetComponent<Takable>())
                    {
                        hit.transform.parent.gameObject.GetComponent<Takable>().focused = true;
                        focused = hit.transform.parent.gameObject;
					}
					if (hit.transform.gameObject.GetComponent<ToggleableGO>())
					{
						hit.transform.gameObject.GetComponent<ToggleableGO>().focused = true;
						focused = hit.transform.gameObject;
					}
					else if (hit.transform.parent.gameObject.GetComponent<ToggleableGO>())
					{
						hit.transform.parent.gameObject.GetComponent<ToggleableGO>().focused = true;
						focused = hit.transform.parent.gameObject;
					}
                    if (hit.transform.gameObject.GetComponent<Selectable>())
                    {
                        focused = hit.transform.gameObject;
                    }
                    else if (hit.transform.parent.gameObject.GetComponent<Selectable>())
                    {
                        focused = hit.transform.parent.gameObject;
                    }
                }
            }
        }

        if (focused)    //if there is a focused object
        {
            //if the player clicks on the object while it is not taken and inventory isn't opened, select it
            if (Input.GetMouseButtonDown(0) && !Takable.objectTaken && !CollectableGO.onInventory && focused.GetComponent<Selectable>())
            {
                focused.GetComponent<Selectable>().isSelected = true;
                Selectable.selected = true;
            }
            //if the object isn't the plank, the box, the bag or the lock room 2 and is selected, show the mouse over overlay
            if (!((focused.tag == "Plank" || focused.tag == "Box" || focused.tag == "Bag" || focused.tag == "LockRoom2" || focused.tag == "Board") && focused.GetComponent<Selectable>().isSelected) && !(Takable.mirrorOnPlank && focused.GetComponent<MirrorScript>()))
            {
                foreach (Transform child in focused.transform)
                {
                    if (child.gameObject.tag == "MouseOver" && !child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
}