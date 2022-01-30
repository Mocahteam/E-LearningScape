using UnityEngine;
using FYFY;

public class UIEffectPlayer_wrapper : BaseWrapper
{
	public UnityEngine.GameObject rightBG;
	public UnityEngine.GameObject wrongBG;
	public AnimatedSprites solvedAnimation;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "rightBG", rightBG);
		MainLoop.initAppropriateSystemField (system, "wrongBG", wrongBG);
		MainLoop.initAppropriateSystemField (system, "solvedAnimation", solvedAnimation);
	}

}
