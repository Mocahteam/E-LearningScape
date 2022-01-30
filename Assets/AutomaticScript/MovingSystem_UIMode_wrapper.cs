using UnityEngine;
using FYFY;

public class MovingSystem_UIMode_wrapper : BaseWrapper
{
	public UnityEngine.GameObject fpsController;
	public UnityEngine.GameObject fpsCamera;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "fpsController", fpsController);
		MainLoop.initAppropriateSystemField (system, "fpsCamera", fpsCamera);
	}

	public void turn(System.Single angle)
	{
		MainLoop.callAppropriateSystemMethod (system, "turn", angle);
	}

	public void moveForward(System.Single distance)
	{
		MainLoop.callAppropriateSystemMethod (system, "moveForward", distance);
	}

	public void moveOnTheFloor()
	{
		MainLoop.callAppropriateSystemMethod (system, "moveOnTheFloor", null);
	}

}
