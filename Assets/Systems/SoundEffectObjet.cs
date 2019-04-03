using UnityEngine;
using FYFY;

public class SoundEffectObjet : FSystem {
    private Family f_soundObj = FamilyManager.getFamily(new AllOfComponents(typeof(AudioBank), typeof(AudioSource)));
    private Family f_lightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)), new NoneOfTags("LockIntro"));
    private Family f_lightVerrouObj = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)), new AnyOfTags("LockIntro"));
    private Family f_selectLightIndiceObjet = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted), typeof(LinkedWith)), new NoneOfTags("LockIntro","Box"));
    private Family f_selectLightBallbox = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted)), new AnyOfTags("Box"));
    private Family f_findDreamFragment = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

    //Il faut une famille qui comprend tout ce qui est taggé DreamFragmentUI et ceux-ci ne s'activent que quand la popup du fragment s'ouvre 
    //Donc on précise en plus qu'il faut qu'ils soient actifs dans la hiérarchie
    private Family f_dreamFragmentOpenned = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); 

    public static SoundEffectObjet instance;
    private RaycastHit hit;
    //int idFragment;
    bool playedSound;
    
    //private GameObject detectedFragment;


    public SoundEffectObjet()
    {
        if (Application.isPlaying)
        {
            f_lightIndiceObjet.addEntryCallback(onNeedHighlighted);
            f_lightVerrouObj.addEntryCallback(onNeedLockHighlighted);
            f_dreamFragmentOpenned.addEntryCallback(onDreamFragmentOpenned);
           
        }
        instance = this; 
    }

    public void onNeedLockHighlighted (GameObject go)
    {
        if (go.GetComponent<Highlighted>())
        {
            foreach (GameObject lightLock in f_lightVerrouObj)
            {
                Debug.Log("Passage Souris on lock obj");
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[13]);
            }
        }
    }

    //On joue le son 8 quand la souris passe sur un objet qui s'illumine en jaune 
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

    //On joue le son 9 quand on clique sur un dream fragment et que sa popup s'ouvre
    //Pas de besoin de préciser input.getMouseButtonDown puisque le composant ne rentre dans la famille que quand la popup s'ouvre et elle s'ouvre quand on a cliqué dessus
    public void onDreamFragmentOpenned(GameObject go)
    {
        f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[9]);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        Debug.Log("a");
        if (Input.GetMouseButtonDown(0)) //Si on clique sur un objet lumineux alors on joue le son 10 
        {
            Debug.Log("b");

            foreach (GameObject selectObjHightLight in f_selectLightIndiceObjet)
            {
                Debug.Log("Clicked on hightlighted object");
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[10]);
            }

            //rajouter si on a la clef qu'elle est activé et qu'on ouvre en cliquant sur la boite
            foreach (GameObject selectBallboxLight in f_selectLightBallbox)
            {
                Debug.Log("Clicked on hightlighted ballbox");
                f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[14]);
            }
        }

        
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            
            // try to find a fragment touched by the raycast
            if (f_findDreamFragment.contains(hit.transform.gameObject.GetInstanceID()))
            {
                
                Debug.Log("Detection dream fragment");
                foreach (GameObject DreamFrag in f_findDreamFragment)
                {
                    Debug.Log("Reconnaissance dream fragment");
                    if (playedSound == false)
                    {
                        Debug.Log("ss ");
                        f_soundObj.First().GetComponent<AudioSource>().PlayOneShot(f_soundObj.First().GetComponent<AudioBank>().audioBank[8]);
                        playedSound = true;
                    }
                    else
                        Debug.Log("Son déjà joué");
                    
                }
                
            }
            if (!f_findDreamFragment.contains(hit.transform.gameObject.GetInstanceID()))
            {
                playedSound = false;
            }

        }
    }

}