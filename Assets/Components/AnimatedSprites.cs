using UnityEngine;

public class AnimatedSprites : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Sprite[] sprites;
    public bool animate = true;
    public bool loop = true;
    public bool disableWhenFinished = false;
    public int usedSpriteID;
}