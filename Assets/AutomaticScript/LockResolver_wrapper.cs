using UnityEngine;
using FYFY;

public class LockResolver_wrapper : BaseWrapper
{
	public System.Single wheelSpeedRotation;
	public UnityEngine.GameObject wallIntro;
	public UnityEngine.GameObject fences;
	public UnlockedRoom unlockedRoom;
	public UnityEngine.Transform player;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "wheelSpeedRotation", wheelSpeedRotation);
		MainLoop.initAppropriateSystemField (system, "wallIntro", wallIntro);
		MainLoop.initAppropriateSystemField (system, "fences", fences);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
		MainLoop.initAppropriateSystemField (system, "player", player);
	}

	public void moveWheelUp()
	{
		MainLoop.callAppropriateSystemMethod (system, "moveWheelUp", null);
	}

	public void moveWheelDown()
	{
		MainLoop.callAppropriateSystemMethod (system, "moveWheelDown", null);
	}

	public void SelectLeftWheel()
	{
		MainLoop.callAppropriateSystemMethod (system, "SelectLeftWheel", null);
	}

	public void SelectRightWheel()
	{
		MainLoop.callAppropriateSystemMethod (system, "SelectRightWheel", null);
	}

	public void SetWheelSpeed(System.Single newValue)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetWheelSpeed", newValue);
	}

	public void UnlockIntroWall()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockIntroWall", null);
	}

	public void UnlockRoom2Fences()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockRoom2Fences", null);
	}

}
