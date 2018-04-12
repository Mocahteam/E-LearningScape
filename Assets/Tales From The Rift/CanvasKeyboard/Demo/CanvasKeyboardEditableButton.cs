using UnityEngine;
using UnityEngine.UI;
using TalesFromTheRift;

public class CanvasKeyboardEditableButton : MonoBehaviour {

	public Canvas CanvasKeyboardObject;
	public string text
	{
		get { return GetComponentInChildren<Text>().text; }
		set { GetComponentInChildren<Text>().text = value; }
	}

	public void Open() 
	{		
		CanvasKeyboard.Open(CanvasKeyboardObject, gameObject);
	}

	public void Close() 
	{		
		CanvasKeyboard.Close ();
	}

}
