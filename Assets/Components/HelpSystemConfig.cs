using UnityEngine;

/// <summary>
/// Class containing HelpSystem parameters
/// </summary>
public class HelpSystemConfig {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public float sessionDuration = 60; //in minutes

    /// <summary>
    /// Time the player has to wait before being able to ask help again
    /// </summary>
    public float playerHintCooldownDuration = 300;
    /// <summary>
    /// Time waited by the system before being able to give another hint 
    /// </summary>
    public float systemHintCooldownDuration = 15;

    /* The system calculates the number of feedback expected based on the number given, the time spent and the time left
     * Then it compares the ratio numberFeedbackExpected / numberFeedbackLeft to the 2 following steps
     * ratio < step1 : feedbackLevel = 1
     * step1 < ratio < step2 : feedbackLevel = 2
     * ratio > step2 : feedbackLevel = 3
     */
    /// <summary>
    /// numberFeedbackExpected / numberFeedbackLeft compered to this step to calculate the feedback level (more info in HelpSystemConfig)
    /// </summary>
    public float feedbackStep1 = 0.75f;
    /// <summary>
    /// numberFeedbackExpected / numberFeedbackLeft compered to this step to calculate the feedback level (more info in HelpSystemConfig)
    /// </summary>
    public float feedbackStep2 = 2;
    /// <summary>
    /// The HelpSystem increase/decrease a label count depending on the labels returned by Laalys and their weight.
    /// When the label count reaches labelCountStep it is reset to 0 and a hint is given to the player
    /// </summary>
    public float labelCountStep = 20;
}