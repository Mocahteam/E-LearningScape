using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class MovingSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetWalkSpeed(System.Single speedW)
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem", "SetWalkSpeed", speedW);
	}

	public void SetHUD(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem", "SetHUD", state);
	}

}
