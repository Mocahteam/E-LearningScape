using UnityEngine;
using FYFY;

public class IARDreamFragmentManager_wrapper : BaseWrapper
{
	public UnityEngine.RectTransform iarRectTransform;
	public UnityEngine.RectTransform contentContainerRT;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "iarRectTransform", iarRectTransform);
		MainLoop.initAppropriateSystemField (system, "contentContainerRT", contentContainerRT);
	}

	public void OnClickDreamToggle(UnityEngine.UI.Toggle t)
	{
		MainLoop.callAppropriateSystemMethod (system, "OnClickDreamToggle", t);
	}

	public void OnMouseEnterToggle(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "OnMouseEnterToggle", go);
	}

	public void OnMouseExitToggle(System.Int32 instanceID)
	{
		MainLoop.callAppropriateSystemMethod (system, "OnMouseExitToggle", instanceID);
	}

	public void OpenLink()
	{
		MainLoop.callAppropriateSystemMethod (system, "OpenLink", null);
	}

	public void RotateDocument(System.Single angle)
	{
		MainLoop.callAppropriateSystemMethod (system, "RotateDocument", angle);
	}

	public void ZoomDocument(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "ZoomDocument", value);
	}

	public void ResetFragment()
	{
		MainLoop.callAppropriateSystemMethod (system, "ResetFragment", null);
	}

}
