using UnityEngine;
using FYFY;

public class DreamFragmentCollecting_wrapper : BaseWrapper
{
	public UnityEngine.GameObject dfUI;
	public UnityEngine.GameObject onlineButton;
	public System.Boolean firstFragmentFound;
	public UnityEngine.GameObject itemCollectedNotif;
	public MovingModeSelector movingModeSelector;
	public UnityEngine.GameObject player;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "dfUI", dfUI);
		MainLoop.initAppropriateSystemField (system, "onlineButton", onlineButton);
		MainLoop.initAppropriateSystemField (system, "firstFragmentFound", firstFragmentFound);
		MainLoop.initAppropriateSystemField (system, "itemCollectedNotif", itemCollectedNotif);
		MainLoop.initAppropriateSystemField (system, "movingModeSelector", movingModeSelector);
		MainLoop.initAppropriateSystemField (system, "player", player);
	}

	public void CloseFragmentUI()
	{
		MainLoop.callAppropriateSystemMethod (system, "CloseFragmentUI", null);
	}

	public void TurnOffDreamFragment(UnityEngine.GameObject fragment)
	{
		MainLoop.callAppropriateSystemMethod (system, "TurnOffDreamFragment", fragment);
	}

	public void OpenFragmentLink(UnityEngine.GameObject selectedFragment)
	{
		MainLoop.callAppropriateSystemMethod (system, "OpenFragmentLink", selectedFragment);
	}

}
