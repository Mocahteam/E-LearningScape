using UnityEngine;
using FYFY;

public class BallBoxManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject ballBox;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "ballBox", ballBox);
	}

	public void UnlockBallBox()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockBallBox", null);
	}

}
