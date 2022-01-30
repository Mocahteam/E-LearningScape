using UnityEngine;
using FYFY;

public class PlankAndWireManager_wrapper : BaseWrapper
{
	public UnityEngine.LineRenderer lineRenderer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "lineRenderer", lineRenderer);
	}

	public void DisplayWireOnSolution()
	{
		MainLoop.callAppropriateSystemMethod (system, "DisplayWireOnSolution", null);
	}

}
