using UnityEngine;
using FYFY;

public class WhiteBoardManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject eraser;
	public UnityEngine.GameObject boardTexture;
	public UnityEngine.GameObject player;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "eraser", eraser);
		MainLoop.initAppropriateSystemField (system, "boardTexture", boardTexture);
		MainLoop.initAppropriateSystemField (system, "player", player);
	}

	public void SetRenderOrder(UnityEngine.GameObject unused)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetRenderOrder", unused);
	}

}
