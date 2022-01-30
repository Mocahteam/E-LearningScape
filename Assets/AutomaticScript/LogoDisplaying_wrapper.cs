using UnityEngine;
using FYFY;

public class LogoDisplaying_wrapper : BaseWrapper
{
	public UnityEngine.GameObject logoGo;
	public UnityEngine.UI.Image fadingImage;
	public UnityEngine.UI.Image background;
	public ImgBank logos;
	public UnityEngine.GameObject loadingFragment;
	public UnityEngine.Renderer lfMat;
	public UnityEngine.Vector3 currentColor;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "logoGo", logoGo);
		MainLoop.initAppropriateSystemField (system, "fadingImage", fadingImage);
		MainLoop.initAppropriateSystemField (system, "background", background);
		MainLoop.initAppropriateSystemField (system, "logos", logos);
		MainLoop.initAppropriateSystemField (system, "loadingFragment", loadingFragment);
		MainLoop.initAppropriateSystemField (system, "lfMat", lfMat);
		MainLoop.initAppropriateSystemField (system, "currentColor", currentColor);
	}

}
