using UnityEngine;
using FYFY;

public class PlankAndMirrorManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mirrorOnPlank;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mirrorOnPlank", mirrorOnPlank);
	}

	public void PutMirrorOnPlank()
	{
		MainLoop.callAppropriateSystemMethod (system, "PutMirrorOnPlank", null);
	}

	public void SetPlankDiscovered(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetPlankDiscovered", state);
	}

}
