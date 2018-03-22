using UnityEngine;

public class Grabber : MonoBehaviour {
    // Device
    [HideInInspector]
    public SteamVR_TrackedObject trackedObj;

    [HideInInspector]
    public GameObject collidingObject;
    [HideInInspector]
    public GameObject objectInHand;
}
