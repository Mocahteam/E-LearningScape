using UnityEngine;
using FYFY;

public class SoundEffectObjet : FSystem {
    private Family f_soundObj = FamilyManager.getFamily(new AllOfComponents(typeof(AudioBank), typeof(AudioSource)));
    private Family f_lightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)));

    public SoundEffectObjet()
    {
        if (Application.isPlaying)
        {
            f_lightIndiceObjet.addEntryCallback(onNeedHighlighted);
        }
    }

    public void onNeedHighlighted(GameObject go)
    {
        Highlighted light = go.GetComponent<Highlighted>();
        if (go.GetComponent<Highlighted>())
        {
            foreach (GameObject lightClues in f_lightIndiceObjet)
            {
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[8]);
            }
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}
}