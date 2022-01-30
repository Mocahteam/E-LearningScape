using UnityEngine;
using FYFY;

public class DebugModeSystem_wrapper : BaseWrapper
{
	public System.Collections.Generic.List<System.String> code;
	public UnityStandardAssets.Characters.FirstPerson.FirstPersonController player;
	public UnityEngine.Light sun;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "code", code);
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "sun", sun);
	}

}
