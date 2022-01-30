using UnityEngine;
using FYFY;

public class IARQueryEvaluator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject queriesRoom2;
	public UnlockedRoom unlockedRoom;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "queriesRoom2", queriesRoom2);
		MainLoop.initAppropriateSystemField (system, "unlockedRoom", unlockedRoom);
	}

	public void IarOnEndEditAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod (system, "IarOnEndEditAnswer", query);
	}

	public void IarCheckAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod (system, "IarCheckAnswer", query);
	}

}
