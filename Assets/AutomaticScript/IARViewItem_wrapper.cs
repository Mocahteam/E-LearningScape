using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARViewItem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void ToggleItem(UnityEngine.GameObject item)
	{
		MainLoop.callAppropriateSystemMethod ("IARViewItem", "ToggleItem", item);
	}

}
