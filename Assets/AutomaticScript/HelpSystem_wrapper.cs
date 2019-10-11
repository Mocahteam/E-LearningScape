using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class HelpSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void OnPlayerAskHelp()
	{
		MainLoop.callAppropriateSystemMethod ("HelpSystem", "OnPlayerAskHelp", null);
	}

}