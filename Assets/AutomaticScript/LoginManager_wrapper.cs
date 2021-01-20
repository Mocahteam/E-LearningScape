using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class LoginManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void CheckMastermindAnswer()
	{
		MainLoop.callAppropriateSystemMethod ("LoginManager", "CheckMastermindAnswer", null);
	}

	public void OnEndEditMastermindAnswer()
	{
		MainLoop.callAppropriateSystemMethod ("LoginManager", "OnEndEditMastermindAnswer", null);
	}

}
