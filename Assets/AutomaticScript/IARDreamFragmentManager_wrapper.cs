using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARDreamFragmentManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void OnClickDreamToggle(UnityEngine.UI.Toggle t)
	{
		MainLoop.callAppropriateSystemMethod ("IARDreamFragmentManager", "OnClickDreamToggle", t);
	}

}