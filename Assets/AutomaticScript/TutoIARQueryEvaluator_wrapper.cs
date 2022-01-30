using UnityEngine;
using FYFY;

public class TutoIARQueryEvaluator_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void TutoIarOnEndEditAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod (null, "TutoIarOnEndEditAnswer", query);
	}

	public void TutoIarCheckAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod (null, "TutoIarCheckAnswer", query);
	}

	public void IarOnEndEditAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod (null, "IarOnEndEditAnswer", query);
	}

	public void IarCheckAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod (null, "IarCheckAnswer", query);
	}

}
