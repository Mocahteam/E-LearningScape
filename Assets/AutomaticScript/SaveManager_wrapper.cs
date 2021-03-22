using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SaveManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void TrySaving(System.Boolean checkName)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "TrySaving", checkName);
	}

	public void LoadSelectedSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "LoadSelectedSave", null);
	}

	public void CloseSavePopup()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "CloseSavePopup", null);
	}

	public void SetSaveNoticeState(System.Boolean enabled)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SetSaveNoticeState", enabled);
	}

}
