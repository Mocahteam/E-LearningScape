using UnityEngine;
using FYFY;

public class SoundEffectManager : FSystem {
    private Family f_Sounds = FamilyManager.getFamily(new AllOfComponents(typeof(PlaySound)));
    private Family f_AudioSource = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource), typeof(AudioBank)), new NoneOfTags("Player"));

    private AudioSource audioSource;
    private AudioBank audioBank;

    public SoundEffectManager()
    {
        if (Application.isPlaying)
        {
            audioSource = f_AudioSource.First().GetComponent<AudioSource>();
            audioBank = f_AudioSource.First().GetComponent<AudioBank>();

            f_Sounds.addEntryCallback(onNewSoundToPlay);
        }
    }

    private void onNewSoundToPlay(GameObject go)
    {
        PlaySound ps = go.GetComponent<PlaySound>();
        audioSource.PlayOneShot(audioBank.audioBank[ps.id]);
        GameObjectManager.removeComponent<PlaySound>(go);
    }
}