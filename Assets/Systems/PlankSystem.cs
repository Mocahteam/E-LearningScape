using UnityEngine;
using System.Collections;
using FYFY;

public class PlankSystem : FSystem {
    
    private Family plank = FamilyManager.getFamily(new AnyOfTags("Plank"));//all takable objects
    private Family plankTexts = FamilyManager.getFamily(new AnyOfTags("PlankText"));
    private Family connectors = FamilyManager.getFamily(new AnyOfTags("PlankConnector"));

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
        ArrayList selected = new ArrayList();
        foreach (GameObject go in plankTexts)
        {
            Pointable p = go.GetComponent<Pointable>();
            if (!p) continue;
            if (p.selected) selected.Add(go);
        }

        // Réinitialiser les connecteurs
        foreach (GameObject go in connectors) go.SetActive(false);

        // Si il y a moins de 2 mots selectionnés, ou plus de 3, on s'arrete
        if (selected.Count > 3 || selected.Count < 2) return;
        GameObject[] sArray = (GameObject[]) selected.ToArray(typeof(GameObject));

        // Afficher les connecteurs
        for (int i=0; i<selected.Count; i++)
        {
            GameObject elt1 = sArray[i];
            GameObject elt2 = sArray[(i + 1) % selected.Count];

            GameObject c = connectors.getAt(i);
            c.transform.position = Vector3.Lerp(elt1.transform.position, elt2.transform.position, .5f);
            c.transform.LookAt(elt2.transform);
            c.transform.localScale = new Vector3(c.transform.localScale.x, c.transform.localScale.y, Vector3.Distance(elt1.transform.position, elt2.transform.position));
            c.SetActive(true);
        }
	}
}