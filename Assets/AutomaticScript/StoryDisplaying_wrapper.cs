using UnityEngine;
using FYFY;

public class StoryDisplaying_wrapper : BaseWrapper
{
	public UnityEngine.GameObject menuCamera;
	public MovingModeSelector movingModeSelector;
	public UnityEngine.GameObject mainHUD;
	public TMPro.TextMeshProUGUI sdText;
	public UnityEngine.UI.Image fadingImage;
	public UnityEngine.UI.Image background;
	public UnityEngine.GameObject clickFeedback;
	public UnityEngine.GameObject endText;
	public StoryText storyText;
	public Timer timer;
	public UnityEngine.GameObject Chronometer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "menuCamera", menuCamera);
		MainLoop.initAppropriateSystemField (system, "movingModeSelector", movingModeSelector);
		MainLoop.initAppropriateSystemField (system, "mainHUD", mainHUD);
		MainLoop.initAppropriateSystemField (system, "sdText", sdText);
		MainLoop.initAppropriateSystemField (system, "fadingImage", fadingImage);
		MainLoop.initAppropriateSystemField (system, "background", background);
		MainLoop.initAppropriateSystemField (system, "clickFeedback", clickFeedback);
		MainLoop.initAppropriateSystemField (system, "endText", endText);
		MainLoop.initAppropriateSystemField (system, "storyText", storyText);
		MainLoop.initAppropriateSystemField (system, "timer", timer);
		MainLoop.initAppropriateSystemField (system, "Chronometer", Chronometer);
	}

	public void OpenEndLink()
	{
		MainLoop.callAppropriateSystemMethod (system, "OpenEndLink", null);
	}

	public void OpenDebriefingLink()
	{
		MainLoop.callAppropriateSystemMethod (system, "OpenDebriefingLink", null);
	}

	public void ResetGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "ResetGame", null);
	}

	public void LoadStoryProgression(System.Int32 storyProgressionCount)
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadStoryProgression", storyProgressionCount);
	}

}
