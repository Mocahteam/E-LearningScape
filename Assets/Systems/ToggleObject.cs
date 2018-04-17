using UnityEngine;
using FYFY;
using System.Collections.Generic;

public class ToggleObject : FSystem {

	private Family toggleable = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleableGO)));

	private GameObject[] togglingChairsDown;
	private int togglingChairsDownCount = 0;
	private GameObject[] togglingChairsUp;
	private int togglingChairsUpCount = 0;

	private bool toggleTableDown = false;
	private bool toggleTableUp = false;
	private GameObject table;
	private float heightBeforeTableToggle;
	private bool tableGoinUp;
	private Vector3 tableTarget;
    private bool canToggle = false;

	private GameObject forGO;
	private ToggleableGO tmpTGO;

	public ToggleObject(){
		togglingChairsDown = new GameObject[6];
		togglingChairsUp = new GameObject[6];
		int nb = toggleable.Count;
		for(int i = 0; i < nb; i++){
			if (toggleable.getAt (i).name == "Table") {
				table = toggleable.getAt (i);
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
        if (canToggle) {
            int nbToggleable = toggleable.Count;
            for (int i = 0; i < nbToggleable; i++)
            {
                forGO = toggleable.getAt(i);
                tmpTGO = forGO.GetComponent<ToggleableGO>();
                if (tmpTGO.focused && Input.GetMouseButtonDown(0) && !tmpTGO.toggled)
                {
                    tmpTGO.toggled = true;
                    if (forGO.name.Contains("Chair"))
                    {
                        if (forGO.transform.rotation.eulerAngles.x == 0)
                        {
                            togglingChairsDown[togglingChairsDownCount] = forGO;
                            togglingChairsDownCount++;
                        }
                        else if (forGO.transform.rotation.eulerAngles.x == 90)
                        {
                            togglingChairsUp[togglingChairsUpCount] = forGO;
                            togglingChairsUpCount++;
                        }
                        else
                        {
                            forGO.transform.rotation = Quaternion.Euler(0, forGO.transform.rotation.eulerAngles.y, forGO.transform.rotation.eulerAngles.z);
                        }
                    }
                    else if (forGO.name == "Table")
                    {
                        heightBeforeTableToggle = table.transform.position.y;
                        tableGoinUp = true;
                        tableTarget = new Vector3(table.transform.position.x, heightBeforeTableToggle + 1, table.transform.position.z);
                        if (forGO.transform.rotation.eulerAngles.z > 90)
                        {
                            toggleTableUp = true;
                        }
                        else
                        {
                            toggleTableDown = true;
                        }
                    }
                    break;
                }
            }
        }
        canToggle = !CollectableGO.onInventory;

		for (int i = 0; i < togglingChairsDownCount; i++) {
			togglingChairsDown[i].transform.rotation = Quaternion.RotateTowards(togglingChairsDown[i].transform.rotation, Quaternion.Euler(90,togglingChairsDown[i].transform.rotation.eulerAngles.y,togglingChairsDown[i].transform.rotation.eulerAngles.z), 10);
			if (togglingChairsDown[i].transform.rotation.eulerAngles.x == 90) {
				togglingChairsDown[i].GetComponent<ToggleableGO>().toggled = false;
				togglingChairsDown[i] = togglingChairsDown[togglingChairsDownCount-1];
				togglingChairsDownCount--;
			}
		}
		for (int i = 0; i < togglingChairsUpCount; i++) {
			togglingChairsUp[i].transform.rotation = Quaternion.RotateTowards(togglingChairsUp[i].transform.rotation, Quaternion.Euler(0,togglingChairsUp[i].transform.rotation.eulerAngles.y,togglingChairsUp[i].transform.rotation.eulerAngles.z), 10);
			if (togglingChairsUp[i].transform.rotation.eulerAngles.x == 0) {
				togglingChairsUp[i].GetComponent<ToggleableGO>().toggled = false;
				togglingChairsUp[i] = togglingChairsUp[togglingChairsUpCount-1];
				togglingChairsUpCount--;
			}
		}
		if (toggleTableDown) {
			if (tableGoinUp) {
				float dist = (tableTarget - table.transform.position).magnitude;
				table.transform.position = Vector3.MoveTowards (table.transform.position, tableTarget, 0.01f+dist/10);
				if (table.transform.position == tableTarget) {
					tableTarget = new Vector3 (table.transform.position.x, heightBeforeTableToggle, table.transform.position.z);
					tableGoinUp = false;
				}
			} else {
				float dist = (tableTarget - table.transform.position).magnitude;
				table.transform.position = Vector3.MoveTowards (table.transform.position, tableTarget, 0.01f+(1-dist)/10);
				if (table.transform.position == tableTarget) {
					toggleTableDown = false;
					table.GetComponent<ToggleableGO> ().toggled = false;
				}
			}
			table.transform.rotation = Quaternion.RotateTowards (table.transform.rotation, Quaternion.Euler (table.transform.rotation.eulerAngles.x, table.transform.rotation.eulerAngles.y, 180), 5);
		} else if (toggleTableUp) {
			if (tableGoinUp) {
				float dist = (tableTarget - table.transform.position).magnitude;
				table.transform.position = Vector3.MoveTowards (table.transform.position, tableTarget, 0.01f+dist/10);
				if (table.transform.position == tableTarget) {
					tableTarget = new Vector3 (table.transform.position.x, heightBeforeTableToggle, table.transform.position.z);
					tableGoinUp = false;
				}
			} else {
				float dist = (tableTarget - table.transform.position).magnitude;
				table.transform.position = Vector3.MoveTowards (table.transform.position, tableTarget, 0.01f+(1-dist)/10);
				if (table.transform.position == tableTarget) {
					toggleTableUp = false;
					table.GetComponent<ToggleableGO> ().toggled = false;
				}
			}
			table.transform.rotation = Quaternion.RotateTowards (table.transform.rotation, Quaternion.Euler (table.transform.rotation.eulerAngles.x, table.transform.rotation.eulerAngles.y, 0), 5);
		}
	}
}