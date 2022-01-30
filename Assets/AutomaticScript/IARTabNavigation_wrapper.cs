using UnityEngine;
using FYFY;

public class IARTabNavigation_wrapper : BaseWrapper
{
	public UnityEngine.Sprite selectedTabSprite;
	public UnityEngine.Sprite defaultTabSprite;
	public UnityEngine.GameObject mainHUD;
	public UnlockedRoom unlockedRoom;
	public UnityEngine.GameObject iar;
	public System.Boolean skipNextClose;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "selectedTabSprite", selectedTabSprite);
		MainLoop.initAppropriateSystemField (system, "defaultTabSprite", defaultTabSprite);
		MainLoop.initAppropriateSystemField (system, "mainHUD", mainHUD);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
		MainLoop.initAppropriateSystemField (system, "iar", iar);
		MainLoop.initAppropriateSystemField (system, "skipNextClose", skipNextClose);
	}

	public void openLastQuestions()
	{
		MainLoop.callAppropriateSystemMethod (system, "openLastQuestions", null);
	}

	public void openIar(System.Int32 tabId)
	{
		MainLoop.callAppropriateSystemMethod (system, "openIar", tabId);
	}

	public void closeIar()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeIar", null);
	}

	public void SwitchTab(UnityEngine.GameObject newSelectedTab)
	{
		MainLoop.callAppropriateSystemMethod (system, "SwitchTab", newSelectedTab);
	}

}
