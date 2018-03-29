using UnityEngine;
using FYFY;

public class AssembleTables : FSystem {
    
    private Family tObjects = FamilyManager.getFamily(new AnyOfTags("Object", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable), typeof(Takable)));//all takable objects
    private Family tablesE05 = FamilyManager.getFamily(new AnyOfTags("TableE05"));

    private GameObject taken = null;        //the gameobject currently taken
    private bool checkTakenObject = true;
    private bool waitForRelease = false;    
    private float tableDist = 1f;           //distance between player and the table when the table is selected
    private Vector3 point1 = Vector3.zero;
    private Vector3 point2 = Vector3.zero;
    private float speed = 0.03f;            //speed of the animation when assembling tables
    private bool movingTable = false;

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
        if(Takable.objectTaken && checkTakenObject) //if an object is taken but wasn't saved in "taken" yet
        {
            checkTakenObject = false;   //false when the currently taken object is saved in "taken"
            foreach(GameObject go in tObjects)
            {
                if (go.GetComponent<Takable>().taken)   //find the taken object
                {
                    if (go.GetComponent<Takable>().tag == "TableE05")
                    {
                        taken = go; //save it if it's a table
                    }
                    else
                    {
                        taken = null;
                    }
                    break;
                }
            }
        }
        else if(Takable.objectTaken && !checkTakenObject && taken != null && !waitForRelease)   //if the player is holding a table
        {
            waitForRelease = true;
        }

        //if the player just released the table (from the right click until the table stops moving)
        if (waitForRelease && Input.GetMouseButtonDown(1) || movingTable)
        {
            waitForRelease = false;     //initial value
            checkTakenObject = true;    //initial value
            if (Mathf.Abs(0.55f*0.7f - taken.transform.position.y) < 0.01f) //if the table is on the floor
            {
                movingTable = false;        //initial value

                /*this loop checks if the released table is close to another table
                 *the positions from which the animation can be started are different depending on the table
                 */
                foreach (GameObject table in tablesE05)
                {
                    if (table.name.Contains(3.ToString()))  //the table with the piece of paper on the back (unity gameobject axes)
                    {
                        point1 = table.transform.position - table.transform.forward * 1.55f + table.transform.right * 0.775f; //back right position
                        point2 = table.transform.position - table.transform.forward * 1.55f - table.transform.right * 0.775f; //back left position
                        //if the table is close to one of the two points (dist < tableDist), move the released table to assemble the 2 table
                        if ((point1 - taken.transform.position).magnitude < tableDist && (point1 - taken.transform.position).magnitude > 0.01f)
                        {
                            taken.transform.position = Vector3.MoveTowards(taken.transform.position, point1, speed);
                            taken.transform.rotation = table.transform.rotation;
                            movingTable = true;
                        }
                        else if ((point2 - taken.transform.position).magnitude < tableDist && (point2 - taken.transform.position).magnitude > 0.01f)
                        {
                            taken.transform.position = Vector3.MoveTowards(taken.transform.position, point2, speed);
                            taken.transform.rotation = table.transform.rotation;
                            movingTable = true;
                        }
                    }
                    else if (table.name.Contains(1.ToString())) //the table with the piece of paper on the front left (unity gameobject axes)
                    {
                        point1 = table.transform.position - table.transform.right * 1.525f;   //left position
                        point2 = table.transform.position + table.transform.forward * 1.55f - table.transform.right * 0.775f; //forward left position
                        //if the table is close to one of the two points (dist < tableDist), move the released table to assemble the 2 table
                        if ((point1 - taken.transform.position).magnitude < tableDist && (point1 - taken.transform.position).magnitude > 0.01f)
                        {
                            taken.transform.position = Vector3.MoveTowards(taken.transform.position, point1, speed);
                            taken.transform.rotation = table.transform.rotation;
                            movingTable = true;
                        }
                        else if ((point2 - taken.transform.position).magnitude < tableDist && (point2 - taken.transform.position).magnitude > 0.01f)
                        {
                            taken.transform.position = Vector3.MoveTowards(taken.transform.position, point2, speed);
                            taken.transform.rotation = table.transform.rotation;
                            movingTable = true;
                        }
                    }
                    else if (table.name.Contains(2.ToString())) //the table with the piece of paper on the front right (unity gameobject axes)
                    {
                        point1 = table.transform.position + table.transform.right * 1.525f;   //right position
                        point2 = table.transform.position + table.transform.forward * 1.55f + table.transform.right * 0.775f; //forward right position
                        //if the table is close to one of the two points (dist < tableDist), move the released table to assemble the 2 table
                        if ((point1 - taken.transform.position).magnitude < tableDist && (point1 - taken.transform.position).magnitude > 0.01f)
                        {
                            taken.transform.position = Vector3.MoveTowards(taken.transform.position, point1, speed);
                            taken.transform.rotation = table.transform.rotation;
                            movingTable = true;
                        }
                        else if ((point2 - taken.transform.position).magnitude < tableDist && (point2 - taken.transform.position).magnitude > 0.01f)
                        {
                            taken.transform.position = Vector3.MoveTowards(taken.transform.position, point2, speed);
                            taken.transform.rotation = table.transform.rotation;
                            movingTable = true;
                        }
                    }
                }
            }
        }
	}
}