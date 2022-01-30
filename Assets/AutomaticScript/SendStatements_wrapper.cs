using UnityEngine;
using FYFY;

public class SendStatements_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void initGBLXAPI()
	{
		MainLoop.callAppropriateSystemMethod (system, "initGBLXAPI", null);
	}

	public void initPlayerName(System.String sessionID)
	{
		MainLoop.callAppropriateSystemMethod (system, "initPlayerName", sessionID);
	}

}
