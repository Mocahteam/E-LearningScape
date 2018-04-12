using UnityEngine;
using System.Collections;
using TalesFromTheRift;

public class ToggleKeyboard : MonoBehaviour 
{
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			if (CanvasKeyboard.IsOpen)
			{
				CanvasKeyboard.Close();
			}
			else
			{
				CanvasKeyboard.Open(GetComponent<Canvas>());
			}
		}
	}
}
