using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynchroAudioAnim : MonoBehaviour {

    public AudioSource audioSource;

    public void playClip(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
}
