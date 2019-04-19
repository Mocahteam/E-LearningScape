using UnityEngine;

public class IntensityLight : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Light sunlight;

    public void setIntensity (float newValue)
    {
        sunlight.intensity = newValue;
    }

}