using UnityEngine;

public class Ball : MonoBehaviour {//enigma03's balls
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Vector3 initialPosition; //position of the ball when the box is closed
    public int id;                  //balls are taken out of the box in order by id
    public bool outOfBox;
    public Color color;

    //information displayed on the ball
    public string text;             //the pedagogic word associated to the ball
    public int number;
}