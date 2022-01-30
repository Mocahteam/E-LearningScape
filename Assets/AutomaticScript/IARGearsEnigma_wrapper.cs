using UnityEngine;
using FYFY;

public class IARGearsEnigma_wrapper : BaseWrapper
{
	public UnityEngine.GameObject gears;
	public UnityEngine.GameObject login;
	public UnityEngine.GameObject player;
	public UnlockedRoom unlockedRoom;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "gears", gears);
		MainLoop.initAppropriateSystemField (system, "login", login);
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
	}

	public void SolveGearsEnigma()
	{
		MainLoop.callAppropriateSystemMethod (system, "SolveGearsEnigma", null);
	}

}
