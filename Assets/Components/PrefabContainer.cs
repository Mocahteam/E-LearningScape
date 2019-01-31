using UnityEngine;

/// <summary>
/// Component used to set a prefab from UnityEditor and then use it in a system
/// </summary>
public class PrefabContainer : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public GameObject prefab;
}