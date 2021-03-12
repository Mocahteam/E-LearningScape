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
		public int level;
		// if the hint isn't about a wrong answer feedback then wrongAnswer is empty
		public string wrongAnswer;
		public bool seen;

		public HintData(int mID, string n, int hintLevel = -1, string wrongAnswerText = "", bool hintSeen = false)
        {
			monitorID = mID;
			name = n;
			level = hintLevel;
			wrongAnswer = wrongAnswerText;
			seen = hintSeen;
        }
    }

	/// <summary>
	/// Tha time at which the progression was saved on the file
	/// </summary>
	public DateTime saveDate;
	public string sessionID;

	/// <summary>
	/// True if the puzzle were virtual at least once
	/// If true and game loaded with physical puzzles, set them to virtual
	/// </summary>
	public bool virtualPuzzle;

	/// <summary>
	/// Keep track of the progression in history texts
	/// </summary>
	public int storyTextCount;
	/// <summary>
	/// ID of the room in which the player was when progression was saved
	/// </summary>
	public int playerPositionRoom;
	/// <summary>
	/// The time spent playing
	/// </summary>
	public float playingDuration;

	/// <summary>
	/// Contains the state of each collectable items:
	/// 0 - not collected, 1 - collected, 2 - collected and used/disabled.
	/// There are 18 collectable items (including virtual puzzles):
	/// 0-introScroll, 1-keyBallBox, 2-wire, 3-keySatchel, 4-mirror, 5-glasses1, 6-glasses2,
	/// 7 to 11-scroll 1 to 5, 12-lamp, 13 to 17-virtuelPuzzleSet 1 to 5
	/// </summary>
	public int[] collectableItemsStates;
	/// <summary>
	/// Contains the state of each dream fragment:
	/// 0 - not collected, 1 - collected but not seen in IAR, 2 - collected and seen in IAR.
	/// There are 14 dream fragments of type 0.
	/// Values 7, 14, 15, 16, 18 and 19 are used for dream fragments of type 1
	/// </summary>
	public int[] dreamFragmentsStates;
	/// <summary>
	/// Contains the state of each locked door (true if unlocked)
	/// There are 3 locked doors:
	/// 0-door between tuto and room1, 1-door between room 1 and 2, 2-door between room 2 and 3
	/// </summary>
	public bool[] lockedDoorsStates;
	/// <summary>
	/// Contains the state of each toggleable objects (true if "turned on").
	/// There are 8 toggleable objects:
	/// 0-correctChair, 1 to 5-chairs 1 to 5, 6-table, 7-chest room 3
	/// </summary>
	public bool[] toggleablesStates;
	/// <summary>
	/// The texture the player drew to erase words
	/// </summary>
	public byte[] boardEraseTexture;
	/// <summary>
	/// The final position of the eraser on the board
	/// </summary>
	public Vector3 boardEraserPosition;

	/// <summary>
	/// Contains the state of the questions in IAR (true if answered).
	/// There are 13 questions ordered by room then by question id.
	/// For the last room, IDs are: 9-puzzles, 10-enigma16, 11-lamp, 12-white board
	/// </summary>
	public bool[] iarQueriesStates;
	/// <summary>
	/// True if gear enigma was solved
	/// </summary>
	public bool gearEnigmaState;

	/// <summary>
	/// Time left before being able to ask for an new hint
	/// </summary>
	public float hintCooldown;
	/// <summary>
	/// Contains the value of HelpSystem's systemHintTimer used to count the time spent
	/// since the system last gave a hint to the player with labelCount
	/// </summary>
	public float systemHintTimer;
	/// <summary>
	/// List of the hints received by the player, ordered by date.
	/// The bool is true if the hint was seen
	/// </summary>
	public List<HintData> receivedHints;
	/// <summary>
	/// Contains the value of HelpSystem's labelCount.
	/// When a label is received from Laalys, its weight is added to labelCount.
	/// </summary>
	public float helpLabelCount;

	/// <summary>
	/// List of string containing complete and filtered petri nets markings given by Laalys
	/// </summary>
	public List<string> petriNetsMarkings;
}