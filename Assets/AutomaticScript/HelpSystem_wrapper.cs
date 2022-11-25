using UnityEngine;
using FYFY;

public class HelpSystem_wrapper : BaseWrapper
{
	public GameHints gameHints;
	public InternalGameHints internalGameHints;
	public UnityEngine.GameObject askHelpButton;
	public LabelWeights labelWeights;
	public UnityEngine.GameObject helpTab;
	public Timer timer;
	public UnityEngine.GameObject scrollView;
	public UnityEngine.GameObject intuitionPopUp;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "gameHints", gameHints);
		MainLoop.initAppropriateSystemField (system, "internalGameHints", internalGameHints);
		MainLoop.initAppropriateSystemField (system, "askHelpButton", askHelpButton);
		MainLoop.initAppropriateSystemField (system, "labelWeights", labelWeights);
		MainLoop.initAppropriateSystemField (system, "helpTab", helpTab);
		MainLoop.initAppropriateSystemField (system, "timer", timer);
		MainLoop.initAppropriateSystemField (system, "scrollView", scrollView);
		MainLoop.initAppropriateSystemField (system, "intuitionPopUp", intuitionPopUp);
	}

	public void initHelpSystem()
	{
		MainLoop.callAppropriateSystemMethod (system, "initHelpSystem", null);
	}

	public void updatePnCompletion()
	{
		MainLoop.callAppropriateSystemMethod (system, "updatePnCompletion", null);
	}

	public void OnPlayerAskHelp()
	{
		MainLoop.callAppropriateSystemMethod (system, "OnPlayerAskHelp", null);
	}

	public void LoadHelpSystemValues()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadHelpSystemValues", null);
	}

}
