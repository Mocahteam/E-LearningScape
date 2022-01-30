using UnityEngine;
using FYFY;

public class TutoMovingSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetWalkSpeed(System.Single speedW)
	{
		MainLoop.callAppropriateSystemMethod (null, "SetWalkSpeed", speedW);
	}

	public void SetHUD(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (null, "SetHUD", state);
	}

}
