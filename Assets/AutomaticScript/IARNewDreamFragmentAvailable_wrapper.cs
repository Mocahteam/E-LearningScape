using UnityEngine;
using FYFY;

public class IARNewDreamFragmentAvailable_wrapper : BaseWrapper
{
	public System.Boolean firstFragmentOccurs;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "firstFragmentOccurs", firstFragmentOccurs);
	}

}
