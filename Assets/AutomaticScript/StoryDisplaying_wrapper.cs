using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class StoryDisplaying_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void OpenEndLink()
	{
		MainLoop.callAppropriateSystemMethod ("StoryDisplaying", "OpenEndLink", null);
	}

	public void ResetGame()
	{
		MainLoop.callAppropriateSystemMethod ("StoryDisplaying", "ResetGame", null);
	}

	public void LoadStoryProgression(System.Int32 storyProgressionCount)
	{
		MainLoop.callAppropriateSystemMethod ("StoryDisplaying", "LoadStoryProgression", storyProgressionCount);
	}

}
