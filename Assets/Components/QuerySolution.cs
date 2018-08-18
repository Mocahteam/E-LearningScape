using UnityEngine;
using System.Collections.Generic;

public class QuerySolution : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public List<string> orSolutions; // player answer has to include at least one of these solutions but each solution can be used only once if it is shared between several queries
	public List<string> andSolutions; // player answer has to include all of these solutions
}