using UnityEngine;
using FYFY;

public class MovingSystem_TeleportMode_wrapper : BaseWrapper
{
	public UnityEngine.GameObject pinTarget;
	public UnityEngine.GameObject fpsController;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "pinTarget", pinTarget);
		MainLoop.initAppropriateSystemField (system, "fpsController", fpsController);
	}

}
