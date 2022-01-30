using UnityEngine;
using FYFY;

public class CheckJava_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void checkJava()
	{
		MainLoop.callAppropriateSystemMethod (system, "checkJava", null);
	}

}
