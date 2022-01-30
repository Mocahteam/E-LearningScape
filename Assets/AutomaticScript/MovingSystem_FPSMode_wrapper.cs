using UnityEngine;
using FYFY;

public class MovingSystem_FPSMode_wrapper : BaseWrapper
{
	public UnityEngine.GameObject player;
	public System.Single traceMovementFrequency;
	public System.Boolean crouching;
	public UnityEngine.GameObject cursor;
	public UnityEngine.GameObject night;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "traceMovementFrequency", traceMovementFrequency);
		MainLoop.initAppropriateSystemField (system, "crouching", crouching);
		MainLoop.initAppropriateSystemField (system, "cursor", cursor);
		MainLoop.initAppropriateSystemField (system, "night", night);
	}

	public void SetWalkSpeed(System.Single speedW)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetWalkSpeed", speedW);
	}

	public void UnlockAllHUD()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockAllHUD", null);
	}

	public void ChangePose(System.Boolean animation)
	{
		MainLoop.callAppropriateSystemMethod (system, "ChangePose", animation);
	}

}
