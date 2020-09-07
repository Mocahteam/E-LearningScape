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

}