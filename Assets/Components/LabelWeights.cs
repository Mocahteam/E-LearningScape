using UnityEngine;
using System.Collections.Generic;

public class LabelWeights : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Dictionary<string, float> weights;
}