using UnityEngine;
using FYFY;

public class SaveManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject player;
	public Timer timer;
	public UnlockedRoom unlockedRoom;
	public UnityEngine.GameObject IAR_Tabs;
	public UnityEngine.GameObject gears;
	public UnityEngine.GameObject IAR_Room2;
	public UnityEngine.GameObject pressY;
	public UnityEngine.GameObject eraser;
	public UnityEngine.Renderer boardTexture;
	public UnityEngine.GameObject crouchHint;
	public UnityEngine.GameObject outOfFirstRoom;
	public UnityEngine.GameObject inventoryHUD;
	public UnityEngine.GameObject dreamFragmentHUD;
	public UnityEngine.GameObject HelpHUD;
	public UnityEngine.GameObject gameRooms;
	public MovingModeSelector movingModeSelector;
	public UnityEngine.GameObject loadPopup;
	public UnityEngine.UI.Button menuLoadButton;
	public UnityEngine.GameObject savePopup;
	public UnityEngine.UI.Button menuSaveButton;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "player", player);
		MainLoop.initAppropriateSystemField (system, "timer", timer);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
		MainLoop.initAppropriateSystemField (system, "IAR_Tabs", IAR_Tabs);
		MainLoop.initAppropriateSystemField (system, "gears", gears);
		MainLoop.initAppropriateSystemField (system, "IAR_Room2", IAR_Room2);
		MainLoop.initAppropriateSystemField (system, "pressY", pressY);
		MainLoop.initAppropriateSystemField (system, "eraser", eraser);
		MainLoop.initAppropriateSystemField (system, "boardTexture", boardTexture);
		MainLoop.initAppropriateSystemField (system, "crouchHint", crouchHint);
		MainLoop.initAppropriateSystemField (system, "outOfFirstRoom", outOfFirstRoom);
		MainLoop.initAppropriateSystemField (system, "inventoryHUD", inventoryHUD);
		MainLoop.initAppropriateSystemField (system, "dreamFragmentHUD", dreamFragmentHUD);
		MainLoop.initAppropriateSystemField (system, "HelpHUD", HelpHUD);
		MainLoop.initAppropriateSystemField (system, "gameRooms", gameRooms);
		MainLoop.initAppropriateSystemField (system, "movingModeSelector", movingModeSelector);
		MainLoop.initAppropriateSystemField (system, "loadPopup", loadPopup);
		MainLoop.initAppropriateSystemField (system, "menuLoadButton", menuLoadButton);
		MainLoop.initAppropriateSystemField (system, "savePopup", savePopup);
		MainLoop.initAppropriateSystemField (system, "menuSaveButton", menuSaveButton);
	}

	public void TrySaving(System.Boolean checkName)
	{
		MainLoop.callAppropriateSystemMethod (system, "TrySaving", checkName);
	}

	public void LoadSelectedSave()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadSelectedSave", null);
	}

	public void CloseSavePopup()
	{
		MainLoop.callAppropriateSystemMethod (system, "CloseSavePopup", null);
	}

	public void SetSaveNoticeState(System.Boolean enabled)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetSaveNoticeState", enabled);
	}

}
