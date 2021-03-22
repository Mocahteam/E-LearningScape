using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARHintManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetNormalColor(UnityEngine.UI.Button b)
	{
		MainLoop.callAppropriateSystemMethod ("IARHintManager", "SetNormalColor", b);
	}

	public void OnClickHintLinkButton()
	{
		MainLoop.callAppropriateSystemMethod ("IARHintManager", "OnClickHintLinkButton", null);
	}

}
