using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARQueryEvaluator_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void IarOnEndEditAnswer(UnityEngine.GameObject query)
	{
		MainLoop.callAppropriateSystemMethod ("IARQueryEvaluator", "IarOnEndEditAnswer", query);
	}

}
