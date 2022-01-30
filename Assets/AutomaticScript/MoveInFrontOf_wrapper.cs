using UnityEngine;
using FYFY;

public class MoveInFrontOf_wrapper : BaseWrapper
{
	public UnityEngine.GameObject player;
	public MovingModeSelector movingModeSelector;
	public UnityEngine.GameObject quitEnigma;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "movingModeSelector", movingModeSelector);
		MainLoop.initAppropriateSystemField (system, "quitEnigma", quitEnigma);
	}

}
