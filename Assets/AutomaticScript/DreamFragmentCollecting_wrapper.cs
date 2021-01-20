using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class DreamFragmentCollecting_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void CloseFragmentUI()
	{
		MainLoop.callAppropriateSystemMethod ("DreamFragmentCollecting", "CloseFragmentUI", null);
	}

	public void TurnOffDreamFragment(UnityEngine.GameObject fragment)
	{
		MainLoop.callAppropriateSystemMethod ("DreamFragmentCollecting", "TurnOffDreamFragment", fragment);
	}

	public void OpenFragmentLink()
	{
		MainLoop.callAppropriateSystemMethod ("DreamFragmentCollecting", "OpenFragmentLink", null);
	}

}
