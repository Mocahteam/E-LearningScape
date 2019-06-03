using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	//Used for singleton
	public static GameManager GM;

	//Create Keycodes that will be associated with each of our commands.
	//These can be accessed by any other script in our game
	public KeyCode jump {get; set;}
	public KeyCode forward {get; set;}
	public KeyCode backward {get; set;}
	public KeyCode left {get; set;}
	public KeyCode right {get; set;}



	void Awake()
	{
		//Singleton pattern
		if(GM == null)
		{
			DontDestroyOnLoad(gameObject);
			GM = this;
		}	
		else if(GM != this)
		{
			Destroy(gameObject);
		}

		/*Assign each keycode when the game starts.
		 * Loads data from PlayerPrefs so if a user quits the game, 
		 * their bindings are loaded next time. Default values
		 * are assigned to each Keycode via the second parameter
		 * of the GetString() function
		 */
		jump = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("jumpKey", "Space"));
		forward = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("forwardKey", "Z"));
		backward = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("backwardKey", "S"));
		left = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("leftKey", "Q"));
		right = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("rightKey", "D"));

	}

	void Start () 
	{
	
	}

	void Update () 
	{
	
	}
}
