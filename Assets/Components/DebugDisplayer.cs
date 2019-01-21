using UnityEngine;
using System.Collections.Generic;

public class DebugDisplayer : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public float timer = 0;
    public float totalWeightedMetaActions = 0;
    public float playerHintTimer = float.MinValue;
    public float systemHintTimer = float.MinValue;

    public float labelCount = 0;
    public int nbFeedBackGiven = 0;
    public float feedbackStep1 = 0.75f;
    public float feedbackStep2 = 2;
    public float correctCoef = -2;
    public float errorCoef = 2;
    public float otherCoef = 1;
    public float errorStep = 50;
    public float otherStep = 75;

    public float enigmaRatio = 0;
    public float timeRatio = 0;
    public float progressionRatio = 0;

    public List<int> availableComponentMonitoringIDs;
}