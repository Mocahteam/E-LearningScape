using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class WhiteBoardManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetRenderOrder(UnityEngine.GameObject unused)
	{
		MainLoop.callAppropriateSystemMethod ("WhiteBoardManager", "SetRenderOrder", unused);
	}

}
