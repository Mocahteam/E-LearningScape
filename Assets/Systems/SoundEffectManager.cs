using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class SoundEffectManager : FSystem {
    private Family f_Sounds = FamilyManager.getFamily(new AllOfComponents(typeof(PlaySound)));
    private Family f_AudioSource = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource), typeof(AudioBank)), new NoneOfTags("Player"));
    private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new NoneOfTags("InventoryElements"));

    private AudioSource audioSource;
    private AudioBank audioBank;

    public SoundEffectManager()
    {
        if (Application.isPlaying)
        {
            audioSource = f_AudioSource.First().GetComponent<AudioSource>();
            audioBank = f_AudioSource.First().GetComponent<AudioBank>();

            f_Sounds.addEntryCallback(onNewSoundToPlay);

            foreach (GameObject go in f_buttons)
                go.GetComponent<Button>().onClick.AddListener(delegate { onButtonClick(go); });
        }
    }

    void onButtonClick(GameObject go)
    {
        if (go.name != "Play") // particular case for Play button that has its own specific sound
        {
            if (go.tag == "ValidationButton")
                GameObjectManager.addComponent<PlaySound>(go, new { id = 5 }); // id refer to FPSController AudioBank
            else
                GameObjectManager.addComponent<PlaySound>(go, new { id = 6 }); // id refer to FPSController AudioBank
        }
    }

    private void onNewSoundToPlay(GameObject go)
    {
        foreach (PlaySound ps in go.GetComponents<PlaySound>())
        {
            audioSource.PlayOneShot(audioBank.audioBank[ps.id]);
            GameObjectManager.removeComponent(ps);
        }
    }
}