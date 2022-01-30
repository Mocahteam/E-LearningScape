using UnityEngine;
using FYFY;

public class SoundEffectManager_wrapper : BaseWrapper
{
	public UnityEngine.AudioSource audioSource;
	public AudioBank audioBank;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "audioSource", audioSource);
		MainLoop.initAppropriateSystemField (system, "audioBank", audioBank);
	}

}
