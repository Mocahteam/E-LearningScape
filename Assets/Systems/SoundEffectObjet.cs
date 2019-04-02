using UnityEngine;
using FYFY;

public class SoundEffectObjet : FSystem {
    private Family f_soundObj = FamilyManager.getFamily(new AllOfComponents(typeof(AudioBank), typeof(AudioSource)));
    private Family f_lightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)));
    private Family f_selectLightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted), typeof(LinkedWith)));
    private Family f_findFragmentReve = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

    public static SoundEffectObjet instance; 
    
    public SoundEffectObjet()
    {
        if (Application.isPlaying)
        {
            f_lightIndiceObjet.addEntryCallback(onNeedHighlighted);
        }
        instance = this; 
    }

    
    public void onNeedHighlighted(GameObject go)
    {
        if (go.GetComponent<Highlighted>())
        {
            foreach (GameObject lightClues in f_lightIndiceObjet)
            {
                Debug.Log("Passage Souris on obj");
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[8]);
            }
        }
    }
    
    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        Debug.Log("a");
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("b");
            
            foreach (GameObject selectObjHightLight in f_selectLightIndiceObjet)
            {
                Debug.Log("Clicked on hightlighted object");
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[10]);
            }
        }

        if (DreamFragmentCollecting.instance.Pause == false)
        {

            Debug.Log("aaa");
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("bbb");
                if (DreamFragmentCollecting.instance.Pause && IARTabNavigation.instance.Pause)
                {
                    Debug.Log("c");
                    foreach (GameObject selectDreamFragment in f_findFragmentReve)
                    {
                        Debug.Log("Clicked on dream fragment");
                        f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[9]);

                    }
                }
            }

        }    
            
        
        

    }
}