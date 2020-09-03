using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class EndManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void startEpilog()
	{
		MainLoop.callAppropriateSystemMethod ("EndManager", "startEpilog", null);
	}

}