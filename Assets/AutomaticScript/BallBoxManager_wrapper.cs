using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class BallBoxManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void UnlockBallBox()
	{
		MainLoop.callAppropriateSystemMethod ("BallBoxManager", "UnlockBallBox", null);
	}

}
