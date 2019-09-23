using UnityEngine;

/// <summary>
/// Class containing HelpSystem parameters
/// </summary>
public class HelpSystemConfig {
    public float sessionDuration = 3600; //in seconds

    /// <summary>
    /// Time the player has to wait before being able to ask help again
    /// </summary>
    public float playerHintCooldownDuration = 300;
    /// <summary>
    /// Time waited by the system before being able to give another hint 
    /// </summary>
    public float systemHintCooldownDuration = 15;

    //The system calculates the feedback level depending on actions progression and time progression
    //Then it checks if actionProgression - timeProgression is less than 0 and use thresholds to define feedback level:
    //feedbackStep1 < (actionProgression - timeProgression) < 0 : feedbackLevel = 1 => general feedback
    //feedbackStep2 < (actionProgression - timeProgression) <= feedbackStep1 : feedbackLevel = 2 => intermediate feedback
    //(actionProgression - timeProgression) <= feedbackStep2 : feedbackLevel = 3 => precise feedback

    /// <summary>
    /// First threshold to know if a feedback level 2 is required (more info in HelpSystemConfig)
    /// Default action progression is late to time progression from 10%
    /// </summary>
    public float feedbackStep1 = -0.10f;
    /// <summary>
    /// First threshold to know if a feedback level 3 is required (more info in HelpSystemConfig)
    /// Default action progression is late to time progression from 25%
    /// </summary>
    public float feedbackStep2 = -0.25f;
    /// <summary>
    /// The HelpSystem increase/decrease a label count depending on the labels returned by Laalys and their weight.
    /// When the label count reaches labelCountStep it is reset to 0 and a hint is given to the player
    /// </summary>
    public float labelCountStep = 20;
    /// <summary>
    /// The HelpSystem count the time spent without any action (no traces from Laalys)
    /// Every time the timer reaches noActionFrequency, "stagnation" weight is added to labelCount and the timer is reset
    /// </summary>
    public float noActionFrequency = 10;
}