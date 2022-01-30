using UnityEngine;
using FYFY;

public class IARNewQuestionsAvailable_wrapper : BaseWrapper
{
	public UnityEngine.GameObject questionNotif;
	public UnlockedRoom unlockedRoom;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "questionNotif", questionNotif);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
	}

}
