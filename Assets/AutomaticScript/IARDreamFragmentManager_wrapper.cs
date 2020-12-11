using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARDreamFragmentManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void OnClickDreamToggle(UnityEngine.UI.Toggle t)
	{
		MainLoop.callAppropriateSystemMethod ("IARDreamFragmentManager", "OnClickDreamToggle", t);
	}

	public void OpenLink()
	{
		MainLoop.callAppropriateSystemMethod ("IARDreamFragmentManager", "OpenLink", null);
	}

	public void RotateDocument(System.Single angle)
	{
		MainLoop.callAppropriateSystemMethod ("IARDreamFragmentManager", "RotateDocument", angle);
	}

	public void ZoomDocument(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod ("IARDreamFragmentManager", "ZoomDocument", value);
	}

	public void ResetFragment()
	{
		MainLoop.callAppropriateSystemMethod ("IARDreamFragmentManager", "ResetFragment", null);
	}

}