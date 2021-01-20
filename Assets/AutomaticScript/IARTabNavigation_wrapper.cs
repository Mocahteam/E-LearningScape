using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARTabNavigation_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void openLastQuestions()
	{
		MainLoop.callAppropriateSystemMethod ("IARTabNavigation", "openLastQuestions", null);
	}

	public void openIar(System.Int32 tabId)
	{
		MainLoop.callAppropriateSystemMethod ("IARTabNavigation", "openIar", tabId);
	}

	public void closeIar()
	{
		MainLoop.callAppropriateSystemMethod ("IARTabNavigation", "closeIar", null);
	}

	public void SwitchTab(UnityEngine.GameObject newSelectedTab)
	{
		MainLoop.callAppropriateSystemMethod ("IARTabNavigation", "SwitchTab", newSelectedTab);
	}

}
