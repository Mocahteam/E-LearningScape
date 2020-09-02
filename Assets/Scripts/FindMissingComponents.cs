using UnityEngine;

[ExecuteInEditMode]
public class FindMissingComponents : MonoBehaviour {

	// Use this for initialization
	void Start () { 
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            foreach (Transform childTransform in root.GetComponentsInChildren<Transform>(true))
            { // include root transform
              // Check if current game object is not an excluded game object or a child of an excluded game object
                foreach (Component c in childTransform.gameObject.GetComponents<Component>())
                {
                    if (c == null)
                    { // it is possible if a GameObject contains a breaked component (Missing script)
                        Transform pathTransform = childTransform;
                        string path = pathTransform.gameObject.name;
                        while (pathTransform.parent != null) {
                            pathTransform = pathTransform.parent;
                            path = pathTransform.gameObject.name + "/" + path;
                        }

                        Debug.Log("Missing component in "+ path);
                    }
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
