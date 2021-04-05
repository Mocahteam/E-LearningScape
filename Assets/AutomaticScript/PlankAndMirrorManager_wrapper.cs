using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class PlankAndMirrorManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void PutMirrorOnPlank()
	{
		MainLoop.callAppropriateSystemMethod ("PlankAndMirrorManager", "PutMirrorOnPlank", null);
	}

	public void SetPlankDiscovered(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod ("PlankAndMirrorManager", "SetPlankDiscovered", state);
	}

}
