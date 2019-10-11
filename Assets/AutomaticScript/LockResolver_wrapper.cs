using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class LockResolver_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void moveWheelUp()
	{
		MainLoop.callAppropriateSystemMethod ("LockResolver", "moveWheelUp", null);
	}

	public void moveWheelDown()
	{
		MainLoop.callAppropriateSystemMethod ("LockResolver", "moveWheelDown", null);
	}

	public void SelectLeftWheel()
	{
		MainLoop.callAppropriateSystemMethod ("LockResolver", "SelectLeftWheel", null);
	}

	public void SelectRightWheel()
	{
		MainLoop.callAppropriateSystemMethod ("LockResolver", "SelectRightWheel", null);
	}

	public void SetWheelSpeed(System.Single newValue)
	{
		MainLoop.callAppropriateSystemMethod ("LockResolver", "SetWheelSpeed", newValue);
	}

}