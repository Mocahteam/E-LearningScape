using UnityEngine;
using FYFY;

public class SoundEffectObjet : FSystem {
    private Family f_soundObj = FamilyManager.getFamily(new AllOfComponents(typeof(AudioBank), typeof(AudioSource)));
    private Family f_lightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)));
    private Family f_selectLightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted), typeof(LinkedWith)));
    
    public SoundEffectObjet()
    {
        if (Application.isPlaying)
        {
            f_lightIndiceObjet.addEntryCallback(onNeedHighlighted);
        }
    }

    public void onNeedHighlighted(GameObject go)
    {
        //Highlighted light = go.GetComponent<Highlighted>();
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
        if (Input.GetMouseButtonDown(0))
        {
            foreach (GameObject selectObjHightLight in f_selectLightIndiceObjet)
            {
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[10]);
            }
            
        }
    }
}