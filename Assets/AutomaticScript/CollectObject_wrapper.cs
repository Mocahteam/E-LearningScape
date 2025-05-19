using UnityEngine;
using FYFY;

public class CollectObject_wrapper : BaseWrapper
{
	public UnityEngine.GameObject pressY;
	public UnityEngine.GameObject itemCollectedNotif;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "pressY", pressY);
		MainLoop.initAppropriateSystemField (system, "itemCollectedNotif", itemCollectedNotif);
	}

	public void enableTargetInIAR(UnityEngine.GameObject source)
	{
		MainLoop.callAppropriateSystemMethod (system, "enableTargetInIAR", source);
	}

}
