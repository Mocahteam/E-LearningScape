using UnityEngine;
using FYFY;

public class AssembleTables : FSystem {
    
    private Family tObjects = FamilyManager.getFamily(new AnyOfTags("Object", "Box", "Tablet", "TableE05"), new AllOfComponents(typeof(Selectable), typeof(Takable)));
    private Family tablesE05 = FamilyManager.getFamily(new AnyOfTags("TableE05"));

    private GameObject taken = null;
    private bool checkTakenObject = true;
    private bool waitForRelease = false;
    private float tableDist = 1f;
    private Vector3 point1 = Vector3.zero;
    private Vector3 point2 = Vector3.zero;
    private float speed = 0.03f;
    private bool checkingTablePos = false;
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
        if(Takable.objectTaken && checkTakenObject)
        {
            checkTakenObject = false;
            foreach(GameObject go in tObjects)
            {
                if (go.GetComponent<Takable>().taken)
                {
                    taken = go;
                }
            }
            if (taken.tag != "TableE05")
            {
                taken = null;
            }
        }
        else if(Takable.objectTaken && !checkTakenObject && taken != null && !waitForRelease)
        {
            waitForRelease = true;
        }

        if (waitForRelease && Input.GetMouseButtonDown(1) || checkingTablePos || movingTable)
        {
            waitForRelease = false;
            checkTakenObject = true;
            if(Mathf.Abs(0.55f*0.7f - taken.transform.position.y) < 0.01f)
            {
                checkingTablePos = false;
                movingTable = false;
                foreach(GameObject table in tablesE05)
                {
                    if (table.name.Contains(3.ToString()))
                    {
                        point1 = table.transform.position - table.transform.forward * 1.5f + table.transform.right * 0.75f;
                        point2 = table.transform.position - table.transform.forward * 1.5f - table.transform.right * 0.75f;
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
                    else if (table.name.Contains(1.ToString()))
                    {
                        point1 = table.transform.position - table.transform.right * 1.5f;
                        point2 = table.transform.position + table.transform.forward * 1.5f - table.transform.right * 0.75f;
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
                    else if (table.name.Contains(2.ToString()))
                    {
                        point1 = table.transform.position + table.transform.right * 1.5f;
                        point2 = table.transform.position + table.transform.forward * 1.5f + table.transform.right * 0.75f;
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
            else
            {
                checkingTablePos = true;
            }
        }
	}
}