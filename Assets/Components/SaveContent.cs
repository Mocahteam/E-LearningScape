using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Content of the progression saved
/// </summary>
[Serializable]
public class SaveContent {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	/// <summary>
	/// Struct used to store information about hints
	/// </summary>
	[Serializable]
	public struct HintData
    {
		public int monitorID;
		public string name;
		public string text;
        public string link;
        public string level;
        public bool seen;

		public HintData(int mID, string n, string hintText, string hintLink, string hintlevel, bool hintSeen)
        {
			monitorID = mID;
			name = n;
            text = hintText;
            link = hintLink;
            level = hintlevel;
			seen = hintSeen;
        }
    }

	/// <summary>
	/// The time at which the progression was saved on the file
	/// </summary>
	public DateTime saveDate = DateTime.Now;
	public string sessionID = "";
    public string UUID = "";


    /// <summary>
    /// Navigation mode selected: 0 means FPS, 1 means Teleport, 2 means UI buttons
    /// </summary>
    public int navigationMode = 0;
    /// <summary>
    /// Navigation view selected: true means FPS, false means TPS
    /// </summary>
    public bool FpsView = true;
    /// <summary>
    /// Keep track of the progression in history texts
    /// </summary>
    public int storyTextCount = 0;
    /// <summary>
    /// LastRoomUnlocked
    /// </summary>
    public int lastRoomUnlocked = 0;
    /// <summary>
    /// Wheels number of Intro lock
    /// </summary>
    public int[] lockIntroPositions = new int[3];
    /// <summary>
    /// Wheels number of Room2 lock
    /// </summary>
    public int[] lockRoom2Positions = new int[3];
    /// <summary>
    /// Player position
    /// </summary>
    public float[] playerPosition = new float[3];
    /// <summary>
    /// The time spent playing
    /// </summary>
    public float playingDuration = 0;

	/// <summary>
	/// Contains the state of each collectable items:
	/// 0 - not collected, 1 - collected, 2 - collected and used (removed from iar).
	/// There are 18 collectable items (including virtual puzzles):
	/// introScroll, keyBallBox, wire, keySatchel, mirror, glasses1, glasses2,
	/// scrolls 1 to 5, lamp, virtuelPuzzleSets 1 to 5
	/// </summary>
	public Dictionary<string, int> collectableItemsStates = new Dictionary<string, int>();
    /// <summary>
	/// Contains puzzle position in IAR
	/// </summary>
    public Dictionary<string, float[]> puzzlePosition = new Dictionary<string, float[]>();
    
    /// <summary>
    /// Contains the state of each dream fragment:
    /// 0 - not collected, 1 - collected but not seen in IAR, 2 - collected and seen in IAR.
    /// </summary>
    public Dictionary<string, int> dreamFragmentsStates = new Dictionary<string, int>();

    /// <summary>
    /// True if the "press Y" is enabled
    /// </summary>
    public bool pressY_displayed = false;

    /// <summary>
    /// True if ball box is unlocked
    /// </summary>
    public bool ballbox_opened = false;

    /// <summary>
    /// True if wire is on plank
    /// </summary>
    public bool wireOnPlank = false;

    /// <summary>
    /// True if satchel is unlocked
    /// </summary>
    public bool satchel_opened = false;

    /// <summary>
    /// True if mirror is discovered
    /// </summary>
    public bool plankDiscovered = false;

    /// <summary>
    /// True if mirror is on plank
    /// </summary>
    public bool mirrorOnPlank = false;

    /// <summary>
    /// Contains the state of each toggleable objects (true if "turned on").
    /// There are 8 toggleable objects:
    /// correctChair, chairs 1 to 5, table, chest
    /// </summary>
    public Dictionary<string, bool> toggleablesStates = new Dictionary<string, bool>();
    /// <summary>
    /// The texture the player drew to erase words
    /// </summary>
    public byte[] boardEraseTexture = null;
	/// <summary>
	/// The final position of the eraser on the board
	/// </summary>
	public float[] boardEraserPosition = new float[3];

	/// <summary>
	/// Contains the state of the questions in IAR (true if answered).
	/// There are 13 questions.
	/// </summary>
	public Dictionary<string, bool> iarQueriesStates = new Dictionary<string, bool>();
    public Dictionary<string, string> iarQueriesAnswer = new Dictionary<string, string>();
    public Dictionary<string, string> iarQueriesDesc = new Dictionary<string, string>();
    /// <summary>
    /// State of gear enigma
    /// 0 - not displayed, 1 - displayed and not resolved, 2 - resolved
    /// </summary>
    public int gearEnigmaState = 0;

    /// <summary>
    /// List of the hints accessible to the player (already shown inside IAR).
    /// </summary>
    public List<HintData> accessibleHints = new List<HintData>();
    /// <summary>
    /// List of string containing complete and filtered petri nets markings given by Laalys
    /// </summary>
    public List<string> petriNetsMarkings = new List<string>();
    /// <summary>
    /// Time left before being able to ask for an new hint
    /// </summary>
    public float hintCooldown = 0;
	/// <summary>
	/// Contains the value of HelpSystem's systemHintTimer used to count the time spent
	/// since the system last gave a hint to the player with labelCount
	/// </summary>
	public float systemHintTimer = 0;
    /// <summary>
    /// Contains the value of HelpSystem's labelCount.
    /// When a label is received from Laalys, its weight is added to labelCount.
    /// </summary>
    public float helpLabelCount = 0;
    /// <summary>
    /// List of the bank hints still available for the player.
    /// </summary>
    public Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>> HintDictionary = new Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>>();
    public Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> HintWrongAnswerFeedbacks = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>();
    public Dictionary<string, int> pnNetsRequiredStepsOnStart = new Dictionary<string, int>();

}