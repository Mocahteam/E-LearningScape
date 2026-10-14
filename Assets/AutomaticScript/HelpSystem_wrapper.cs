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
	public TMPro.TMP_Text Completion;
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
		MainLoop.initAppropriateSystemField (system, "Completion", Completion);
	}

	public void initHelpSystem()
	{
		MainLoop.callAppropriateSystemMethod (system, "initHelpSystem", null);
	}

	public void updatePnCompletion()
	{
		MainLoop.callAppropriateSystemMethod (system, "updatePnCompletion", null);
	}

	public void rememberEnigmaIsOver(UnityEngine.GameObject src)
	{
		MainLoop.callAppropriateSystemMethod (system, "rememberEnigmaIsOver", src);
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
