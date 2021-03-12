using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class HelpSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void updatePnCompletion()
	{
		MainLoop.callAppropriateSystemMethod ("HelpSystem", "updatePnCompletion", null);
	}

	public void OnPlayerAskHelp()
	{
		MainLoop.callAppropriateSystemMethod ("HelpSystem", "OnPlayerAskHelp", null);
	}

	public void SetPlayerHintTimer(System.Single hintCooldown)
	{
		MainLoop.callAppropriateSystemMethod ("HelpSystem", "SetPlayerHintTimer", hintCooldown);
	}

	public void LoadHelpSystemValues()
	{
		MainLoop.callAppropriateSystemMethod ("HelpSystem", "LoadHelpSystemValues", null);
	}

}
