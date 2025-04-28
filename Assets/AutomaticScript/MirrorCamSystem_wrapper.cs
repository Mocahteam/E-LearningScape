using UnityEngine;
using FYFY;

public class MirrorCamSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject player;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "player", player);
	}

}
