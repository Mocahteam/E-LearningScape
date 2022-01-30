using UnityEngine;
using FYFY;

public class EndManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject player;
	public UnityEngine.GameObject gameRooms;
	public UnityEngine.GameObject waterFloor;
	public UnlockedRoom unlockedRoom;
	public StoryText storyText;
	public UnityEngine.UI.Image fadingBackground;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "gameRooms", gameRooms);
		MainLoop.initAppropriateSystemField (system, "waterFloor", waterFloor);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
		MainLoop.initAppropriateSystemField (system, "storyText", storyText);
		MainLoop.initAppropriateSystemField (system, "fadingBackground", fadingBackground);
	}

}
