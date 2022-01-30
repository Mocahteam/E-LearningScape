using UnityEngine;
using FYFY;

public class LoginManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mainWindow;
	public UnityEngine.GameObject door;
	public UnityEngine.Transform player;
	public UnityEngine.GameObject rooms;
	public UnlockedRoom unlockedRoom;
	public StoryText storyText;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainWindow", mainWindow);
		MainLoop.initAppropriateSystemField (system, "door", door);
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "rooms", rooms);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
		MainLoop.initAppropriateSystemField (system, "storyText", storyText);
	}

	public void CheckMastermindAnswer()
	{
		MainLoop.callAppropriateSystemMethod (system, "CheckMastermindAnswer", null);
	}

	public void OnEndEditMastermindAnswer()
	{
		MainLoop.callAppropriateSystemMethod (system, "OnEndEditMastermindAnswer", null);
	}

	public void UnlockLoginDoor()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockLoginDoor", null);
	}

}
