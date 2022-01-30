using UnityEngine;
using FYFY;

public class LampManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject fpsLight;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "fpsLight", fpsLight);
	}

}
