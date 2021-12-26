using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class MovingSystem_UIMode_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void turn(System.Single angle)
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem_UIMode", "turn", angle);
	}

	public void moveForward(System.Single distance)
	{
		MainLoop.callAppropriateSystemMethod ("MovingSystem_UIMode", "moveForward", distance);
	}

}
