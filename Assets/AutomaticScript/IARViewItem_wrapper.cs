using UnityEngine;
using FYFY;

public class IARViewItem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject descriptionUI;
	public UnityEngine.GameObject defaultIarTab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "descriptionUI", descriptionUI);
		MainLoop.initAppropriateSystemField (system, "defaultIarTab", defaultIarTab);
	}

	public void ToggleItem(UnityEngine.GameObject item)
	{
		MainLoop.callAppropriateSystemMethod (system, "ToggleItem", item);
	}

}
