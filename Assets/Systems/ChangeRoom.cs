using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class ChangeRoom : FSystem {

    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family cr = FamilyManager.getFamily(new AnyOfTags("ChangeRoom"));

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
        foreach(GameObject go in cr)
        {
            if(go.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
            {
                if (go.name.Contains(1.ToString()))
                {
                    player.First().transform.position = new Vector3(3f, 1, -4f);
                    player.First().transform.rotation = Quaternion.Euler(0, 31f, 0);
                }
                else if (go.name.Contains(2.ToString()))
                {
                    player.First().transform.position = new Vector3(-3f, 1, -3f);
                    player.First().transform.rotation = Quaternion.Euler(0,-26f,0);
                }
            }
        }
	}
}