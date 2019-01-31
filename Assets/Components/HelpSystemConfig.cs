using UnityEngine;

public class HelpSystemConfig {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public float sessionDuration = 60; //in minutes

    public float playerHintCooldownDuration = 300;
    public float systemHintCooldownDuration = 15;

    public float feedbackStep1 = 0.75f;
    public float feedbackStep2 = 2;
    public float labelCountStep = 20;
}