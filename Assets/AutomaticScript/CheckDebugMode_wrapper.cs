using UnityEngine;
using FYFY;

public class CheckDebugMode_wrapper : BaseWrapper
{
	public System.Collections.Generic.List<System.String> code;
	public UnityEngine.Light sun;
	public UnityEngine.Transform rooms;
	public UnlockedRoom unlockedRoom;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "code", code);
		MainLoop.initAppropriateSystemField (system, "sun", sun);
		MainLoop.initAppropriateSystemField (system, "rooms", rooms);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
	}

}
