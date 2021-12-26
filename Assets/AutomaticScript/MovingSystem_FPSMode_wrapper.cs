using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class MovingSystem_FPSMode_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetWalkSpeed(System.Single speedW)
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem_FPSMode", "SetWalkSpeed", speedW);
	}

	public void UnlockAllHUD()
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem_FPSMode", "UnlockAllHUD", null);
	}

	public void ChangePose(System.Boolean animation)
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem_FPSMode", "ChangePose", animation);
	}

}
