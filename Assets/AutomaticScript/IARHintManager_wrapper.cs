using UnityEngine;
using FYFY;

public class IARHintManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject scrollView;
	public UnityEngine.GameObject helpDescUI;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "scrollView", scrollView);
		MainLoop.initAppropriateSystemField (system, "helpDescUI", helpDescUI);
	}

	public void SetNormalColor(UnityEngine.UI.Button b)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetNormalColor", b);
	}

	public void OnClickHintLinkButton()
	{
		MainLoop.callAppropriateSystemMethod (system, "OnClickHintLinkButton", null);
	}

}
