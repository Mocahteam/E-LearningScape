using UnityEngine;

public class ViveController : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    [HideInInspector]
    public GameObject collidingObject;
    [HideInInspector]
    public GameObject objectInHand;
}
