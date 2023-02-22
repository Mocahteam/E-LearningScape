using UnityEngine;
using FYFY;

public class LoadGameContent_wrapper : BaseWrapper
{
	public ImgBank Logos;
	public StoryText storyText;
	public UnityEngine.GameObject iarTabs;
	public UnityEngine.GameObject gears;
	public UnityEngine.GameObject dreamFragmentHUD;
	public UnityEngine.GameObject login;
	public BagImage bagImage;
	public UnityEngine.UI.Image mirrorPlank;
	public Locker lockerRoom2;
	public GameHints gameHints;
	public InternalGameHints internalGameHints;
	public TMPro.TextMeshProUGUI passwordRoom2;
	public LabelWeights labelWeights;
	public TMPro.TextMeshProUGUI creditText;
	public TMPro.TMP_FontAsset AccessibleFont;
	public TMPro.TMP_FontAsset AccessibleFontUI;
	public TMPro.TMP_FontAsset DefaultFont;
	public TMPro.TMP_FontAsset DefaultFontUI;
	public TMPro.TMP_Text GameType;
	public Chronometer chronometer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "Logos", Logos);
		MainLoop.initAppropriateSystemField (system, "storyText", storyText);
		MainLoop.initAppropriateSystemField (system, "iarTabs", iarTabs);
		MainLoop.initAppropriateSystemField (system, "gears", gears);
		MainLoop.initAppropriateSystemField (system, "dreamFragmentHUD", dreamFragmentHUD);
		MainLoop.initAppropriateSystemField (system, "login", login);
		MainLoop.initAppropriateSystemField (system, "bagImage", bagImage);
		MainLoop.initAppropriateSystemField (system, "mirrorPlank", mirrorPlank);
		MainLoop.initAppropriateSystemField (system, "lockerRoom2", lockerRoom2);
		MainLoop.initAppropriateSystemField (system, "gameHints", gameHints);
		MainLoop.initAppropriateSystemField (system, "internalGameHints", internalGameHints);
		MainLoop.initAppropriateSystemField (system, "passwordRoom2", passwordRoom2);
		MainLoop.initAppropriateSystemField (system, "labelWeights", labelWeights);
		MainLoop.initAppropriateSystemField (system, "creditText", creditText);
		MainLoop.initAppropriateSystemField (system, "AccessibleFont", AccessibleFont);
		MainLoop.initAppropriateSystemField (system, "AccessibleFontUI", AccessibleFontUI);
		MainLoop.initAppropriateSystemField (system, "DefaultFont", DefaultFont);
		MainLoop.initAppropriateSystemField (system, "DefaultFontUI", DefaultFontUI);
		MainLoop.initAppropriateSystemField (system, "GameType", GameType);
		MainLoop.initAppropriateSystemField (system, "chronometer", chronometer);
	}

	public void SetFragments(System.Boolean virtualDreamFragment)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetFragments", virtualDreamFragment);
	}

	public void CopySessionID()
	{
		MainLoop.callAppropriateSystemMethod (system, "CopySessionID", null);
	}

}
