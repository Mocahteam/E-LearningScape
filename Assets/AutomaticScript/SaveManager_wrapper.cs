using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SaveManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SaveOnFile(System.String fileName)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SaveOnFile", fileName);
	}

	public void AutoSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "AutoSave", null);
	}

	public void LoadSave()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "LoadSave", null);
	}

	public void SetHintAsSeen(HintContent hint)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SetHintAsSeen", hint);
	}

	public void RemoveHintFromSave(HintContent hint)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "RemoveHintFromSave", hint);
	}

}
