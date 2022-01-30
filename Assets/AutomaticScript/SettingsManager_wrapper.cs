using UnityEngine;
using FYFY;

public class SettingsManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void OnNewText(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "OnNewText", go);
	}

	public void SwitchFont(System.Boolean accessibleFont)
	{
		MainLoop.callAppropriateSystemMethod (system, "SwitchFont", accessibleFont);
	}

	public void UpdateCursorSize(System.Single newSize)
	{
		MainLoop.callAppropriateSystemMethod (system, "UpdateCursorSize", newSize);
	}

	public void UpdateAlpha(System.Single newAlpha)
	{
		MainLoop.callAppropriateSystemMethod (system, "UpdateAlpha", newAlpha);
	}

	public void ToggleTextAnimation(System.Boolean newState)
	{
		MainLoop.callAppropriateSystemMethod (system, "ToggleTextAnimation", newState);
	}

	public void ResetDefaultValues()
	{
		MainLoop.callAppropriateSystemMethod (system, "ResetDefaultValues", null);
	}

	public void SaveSettings()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveSettings", null);
	}

	public void LoadSettings()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadSettings", null);
	}

}
