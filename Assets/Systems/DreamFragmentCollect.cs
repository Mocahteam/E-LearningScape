using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class DreamFragmentCollect : FSystem {

    private Family dfFamily = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));
    private Family dfUIFamily = FamilyManager.getFamily(new AnyOfTags("DreamFragmentUI"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private GameObject dfUI;

    private RaycastHit hit;
    private GameObject selectedFragment;
    private DreamFragment tmpDFComponent;
    private bool onIAR = false;

    public DreamFragmentCollect()
    {
        if (Application.isPlaying)
        {
            dfUI = dfUIFamily.First();
            dfUI.GetComponentInChildren<Button>().onClick.AddListener(CloseWindow);
        }
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            int nb = dfFamily.Count;
            for(int i = 0; i < nb; i++)
            {
                if (Object.ReferenceEquals(hit.transform.gameObject, dfFamily.getAt(i)))
                {
                    if (Input.GetMouseButtonDown(0) && !onIAR)
                    {
                        player.First().GetComponent<FirstPersonController>().enabled = false;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        selectedFragment = dfFamily.getAt(i);
                        dfUI.SetActive(true);
                        tmpDFComponent = selectedFragment.GetComponent<DreamFragment>();
                        if(tmpDFComponent.type == 0)
                        {
                            dfUI.GetComponentInChildren<TextMeshProUGUI>().text = string.Concat("Cherchez le numéro ", tmpDFComponent.id, " et récupérez l'item \"", tmpDFComponent.itemName, "\"");
                        }
                        else if (tmpDFComponent.type == 1 || tmpDFComponent.type == 2)
                        {
                            dfUI.GetComponentInChildren<TextMeshProUGUI>().text = string.Concat("\"", tmpDFComponent.itemName, "\"");
                        }
                    }
                    break;
                }
            }
        }
        onIAR = IARTab.onIAR;

    }

    private void CloseWindow()
    {
        if (selectedFragment.GetComponent<DreamFragment>().type != 2)
        {
            if (selectedFragment.GetComponentInChildren<ParticleSystem>())
            {
                selectedFragment.GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);
            }
            foreach (Transform child in selectedFragment.transform)
            {
                if (child.gameObject.tag == "DreamFragmentLight")
                {
                    child.gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                    break;
                }
            }
        }
        selectedFragment = null;
        dfUI.SetActive(false);
        player.First().GetComponent<FirstPersonController>().enabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}