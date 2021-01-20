using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class MenuSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void StartGame()
	{
		MainLoop.callAppropriateSystemMethod ("MenuSystem", "StartGame", null);
	}

	public void QuitGame()
	{
		MainLoop.callAppropriateSystemMethod ("MenuSystem", "QuitGame", null);
	}

	public void RestartGame()
	{
		MainLoop.callAppropriateSystemMethod ("MenuSystem", "RestartGame", null);
	}

}
