using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class CheckDebugMode_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

}
