using UnityEngine;
using FYFY;

public class TutorialManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject movingModeSelector;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "movingModeSelector", movingModeSelector);
	}

	public void nextStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "nextStep", null);
	}

	public void QuitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "QuitGame", null);
	}

	public void RestartGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "RestartGame", null);
	}

}
