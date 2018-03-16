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
            if(go.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0)) //when the game object is left clicked
            {
                if (go.transform.parent.gameObject.name.Contains(1.ToString())) //if the gameobject if from room 1
                {
                    //teleport the player to room 2
                    player.First().transform.position = new Vector3(3f, 1, -4f);
                    player.First().transform.rotation = Quaternion.Euler(0, 31f, 0);
                }
                else if (go.transform.parent.gameObject.name.Contains(2.ToString())) //if the gameobject is from room 2
                {
                    //teleport the player to room 1
                    player.First().transform.position = new Vector3(-3f, 1, -3f);
                    player.First().transform.rotation = Quaternion.Euler(0,-26f,0);
                }
            }
        }
	}
}