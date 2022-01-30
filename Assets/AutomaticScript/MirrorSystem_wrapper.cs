using UnityEngine;
using FYFY;

public class MirrorSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject player;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "player", player);
	}

}
