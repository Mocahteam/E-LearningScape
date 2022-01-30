using UnityEngine;
using FYFY;

public class SatchelManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject bag;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "bag", bag);
	}

	public void UnlockSatchel()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockSatchel", null);
	}

}
