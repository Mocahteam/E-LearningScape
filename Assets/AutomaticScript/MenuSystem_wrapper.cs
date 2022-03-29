using UnityEngine;
using FYFY;

public class MenuSystem_wrapper : BaseWrapper
{
	public MenuCamera menuCamera;
	public System.Single switchDelay;
	public UnityEngine.UI.Image fadingBackground;
	public System.Single amplitude;
	public System.Int32 nbFrame;
	public UnityEngine.Transform rooms;
	public UnlockedRoom unlockedRoom;
	public UnityEngine.GameObject mainMenu;
	public UnityEngine.GameObject settingsMainMenu;
	public UnityEngine.GameObject IARMenuContent;
	public UnityEngine.GameObject Chronometer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "menuCamera", menuCamera);
		MainLoop.initAppropriateSystemField (system, "switchDelay", switchDelay);
		MainLoop.initAppropriateSystemField (system, "fadingBackground", fadingBackground);
		MainLoop.initAppropriateSystemField (system, "amplitude", amplitude);
		MainLoop.initAppropriateSystemField (system, "nbFrame", nbFrame);
		MainLoop.initAppropriateSystemField (system, "rooms", rooms);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
		MainLoop.initAppropriateSystemField (system, "mainMenu", mainMenu);
		MainLoop.initAppropriateSystemField (system, "settingsMainMenu", settingsMainMenu);
		MainLoop.initAppropriateSystemField (system, "IARMenuContent", IARMenuContent);
		MainLoop.initAppropriateSystemField (system, "Chronometer", Chronometer);
	}

	public void StartGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "StartGame", null);
	}

	public void QuitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "QuitGame", null);
	}

	public void RestartGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "RestartGame", null);
	}

	public void StartTuto()
	{
		MainLoop.callAppropriateSystemMethod (system, "StartTuto", null);
	}

}
