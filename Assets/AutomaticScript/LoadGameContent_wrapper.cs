using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class LoadGameContent_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetFragments(System.Boolean virtualDreamFragment)
	{
		MainLoop.callAppropriateSystemMethod ("LoadGameContent", "SetFragments", virtualDreamFragment);
	}

	public void CopySessionID()
	{
		MainLoop.callAppropriateSystemMethod ("LoadGameContent", "CopySessionID", null);
	}

}