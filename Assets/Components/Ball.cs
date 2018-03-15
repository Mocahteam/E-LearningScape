using UnityEngine;

public class Ball : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Vector3 initialPosition;
    public int id;
    public string text;
    public int number;
    public bool outOfBox;
    public Color color;
}