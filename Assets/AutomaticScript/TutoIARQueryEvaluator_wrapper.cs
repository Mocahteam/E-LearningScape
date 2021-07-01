using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class TutoIARQueryEvaluator_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void TutoIarOnEndEditAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod ("TutoIARQueryEvaluator", "TutoIarOnEndEditAnswer", query);
	}

	public void TutoIarCheckAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod ("TutoIARQueryEvaluator", "TutoIarCheckAnswer", query);
	}

	public void IarOnEndEditAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod ("TutoIARQueryEvaluator", "IarOnEndEditAnswer", query);
	}

	public void IarCheckAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod ("TutoIARQueryEvaluator", "IarCheckAnswer", query);
	}

}
