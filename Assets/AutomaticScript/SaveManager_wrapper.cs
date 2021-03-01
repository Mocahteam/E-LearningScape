using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SaveManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SetNewSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SetNewSave", null);
	}

	public void SaveOnFile(System.String fileName)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SaveOnFile", fileName);
	}

	public void TrySaving(System.Boolean checkName)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "TrySaving", checkName);
	}

	public void AutoSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "AutoSave", null);
	}

	public void LoadSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "LoadSave", null);
	}

	public void LoadSelectedSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "LoadSelectedSave", null);
	}

	public void SetHintAsSeen(HintContent hint)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SetHintAsSeen", hint);
	}

	public void RemoveHintFromSave(HintContent hint)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "RemoveHintFromSave", hint);
	}

	public void SetSelectedSaveButton(UnityEngine.GameObject loadButton)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SetSelectedSaveButton", loadButton);
	}

	public void SetSaveInputfieldText(UnityEngine.GameObject saveButton)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SetSaveInputfieldText", saveButton);
	}

	public void CloseSavePopup()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "CloseSavePopup", null);
	}

}
