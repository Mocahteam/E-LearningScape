using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class TutorialManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void nextStep()
	{
		MainLoop.callAppropriateSystemMethod ("TutorialManager", "nextStep", null);
	}

	public void QuitGame()
	{
		MainLoop.callAppropriateSystemMethod ("TutorialManager", "QuitGame", null);
	}

	public void RestartGame()
	{
		MainLoop.callAppropriateSystemMethod ("TutorialManager", "RestartGame", null);
	}

}
