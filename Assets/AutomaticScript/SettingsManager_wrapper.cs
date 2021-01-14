using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SettingsManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SwitchFont(System.Boolean accessibleFont)
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "SwitchFont", accessibleFont);
	}

	public void UpdateCursorSize(System.Single newSize)
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "UpdateCursorSize", newSize);
	}

	public void UpdateAlpha(System.Single newAlpha)
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "UpdateAlpha", newAlpha);
	}

	public void ToggleTextAnimation(System.Boolean newState)
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "ToggleTextAnimation", newState);
	}

	public void ResetDefaultValues()
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "ResetDefaultValues", null);
	}

	public void SaveSettings()
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "SaveSettings", null);
	}

	public void LoadSettings()
	{
		MainLoop.callAppropriateSystemMethod ("SettingsManager", "LoadSettings", null);
	}

}