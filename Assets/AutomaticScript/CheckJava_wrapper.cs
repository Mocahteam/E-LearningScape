using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class CheckJava_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void checkJava()
	{
		MainLoop.callAppropriateSystemMethod ("CheckJava", "checkJava", null);
	}

}
