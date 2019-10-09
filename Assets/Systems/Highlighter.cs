using UnityEngine;
using FYFY;
using System.Collections.Generic;

public class Highlighter : FSystem {

    // Highlight all in game interactable GameObjects

    //all in game interactive objects
    // in game element linked whith UI game object are collectable and so highlightable (except animated sprites)
	private Family f_highlitable = FamilyManager.getFamily(new AnyOfComponents(typeof(Selectable), typeof(ToggleableGO), typeof(LinkedWith)), new NoneOfComponents(typeof(AnimatedSprites)), new NoneOfLayers(1));

    private Renderer[] tmpRendererList;
    
    private GameObject previousHighlight = null;
    private Queue<Color> previousColor;

    public static Highlighter instance;

    public Highlighter()
    {
        if (Application.isPlaying)
            previousColor = new Queue<Color>();
        instance = this;
    }

    private void unhighlight(GameObject currentHighlight)
    {
        if (previousHighlight != null)
        {
            if (previousHighlight.GetComponent<Selectable>())
                GameObjectManager.addComponent<ActionPerformedForLRS>(previousHighlight, new { verb = "exitedHighlight", objectType = "interactable", objectName = previousHighlight.name });
            else if (previousHighlight.GetComponent<ToggleableGO>())
                GameObjectManager.addComponent<ActionPerformedForLRS>(previousHighlight, new { verb = "exitedHighlight", objectType = "toggable", objectName = previousHighlight.name });
            else
                GameObjectManager.addComponent<ActionPerformedForLRS>(previousHighlight, new { verb = "exitedHighlight", objectType = "item", objectName = previousHighlight.name });
            tmpRendererList = previousHighlight.GetComponentsInChildren<Renderer>();
            int nb = tmpRendererList.Length;
            for (int i = 0; i < nb; i++)
            {
                if (tmpRendererList[i].material.HasProperty("_EmissionColor"))
                {
                    Color c = previousColor.Dequeue();
                    // avoid to change fragments color
                    if (!tmpRendererList[i].GetComponentInParent<DreamFragment>())
                        tmpRendererList[i].material.SetColor("_EmissionColor", c);
                }
            }
            // Remove Highlighted component to this GameObject
            GameObjectManager.removeComponent<Highlighted>(previousHighlight);
        }
        previousHighlight = currentHighlight;
    }

    private void highlight (GameObject currentHighlight)
    {
        if(currentHighlight.GetComponent<Selectable>())
            GameObjectManager.addComponent<ActionPerformedForLRS>(currentHighlight, new { verb = "highlighted", objectType = "interactable", objectName = currentHighlight.name });
        else if(currentHighlight.GetComponent<ToggleableGO>())
            GameObjectManager.addComponent<ActionPerformedForLRS>(currentHighlight, new { verb = "highlighted", objectType = "toggable", objectName = currentHighlight.name });
        else
            GameObjectManager.addComponent<ActionPerformedForLRS>(currentHighlight, new { verb = "highlighted", objectType = "item", objectName = currentHighlight.name });
        if (previousHighlight == null)
            previousHighlight = currentHighlight;
        // Update renderer and highlight game object
        tmpRendererList = currentHighlight.GetComponentsInChildren<Renderer>();
        int nb = tmpRendererList.Length;
        for (int i = 0; i < nb; i++)
        {
            if (tmpRendererList[i].material.HasProperty("_EmissionColor"))
            {
                // Save current emission color
                previousColor.Enqueue(tmpRendererList[i].material.GetColor("_EmissionColor"));
                // Enable emission and hightlight target (avoid to change fragments color)
                if (!tmpRendererList[i].GetComponentInParent<DreamFragment>() && tmpRendererList[i].gameObject.name != ("BoardTexture"))
                {
                    tmpRendererList[i].material.EnableKeyword("_EMISSION");
                    tmpRendererList[i].material.SetColor("_EmissionColor", Color.yellow * Mathf.LinearToGammaSpace(0.8f));
                }
            }
        }
        // Add Highlighted component to this GameObject
        GameObjectManager.addComponent<Highlighted>(currentHighlight);

        GameObjectManager.addComponent<PlaySound>(currentHighlight, new { id = 2 }); // id refer to FPSController AudioBank
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        unhighlight(null);
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        RaycastHit hit;
        GameObject currentHighlight = null;
        // Launch a ray
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            // Check if object is close to the camera
            if ((hit.transform.gameObject.transform.position - Camera.main.transform.position).sqrMagnitude < 49)
            {
                // Evaluate if we have to check family => if object (or its parents) is not the previous one
                bool checkFamily = previousHighlight != hit.transform.gameObject && hit.transform.parent && previousHighlight != hit.transform.parent.gameObject && hit.transform.parent.parent && previousHighlight != hit.transform.parent.parent.gameObject;
                if (checkFamily)
                {
                    // Check if this game object is included into family
                    if (f_highlitable.contains(hit.transform.gameObject.GetInstanceID()))
                    {
                        // save this new highlight game object
                        currentHighlight = hit.transform.gameObject;
                        highlight(currentHighlight);
                    }
                    // Check if parents of hited game object is an interactive game object and this game object doesn't contain a dream fragment
                    else if (!hit.transform.gameObject.GetComponent<DreamFragment>() && hit.transform.parent)
                    {
                        if (f_highlitable.contains(hit.transform.parent.gameObject.GetInstanceID())){
                            // save the parent of this game object as the new highlighted game object
                            currentHighlight = hit.transform.parent.gameObject;
                            highlight(currentHighlight);
                        } else if (hit.transform.parent.parent && f_highlitable.contains(hit.transform.parent.parent.gameObject.GetInstanceID())){
                            // save the grandparent of this game object as the new highlighted game object
                            currentHighlight = hit.transform.parent.parent.gameObject;
                            highlight(currentHighlight);
                        }
                    }
                }
                else
                {
                    if (!hit.transform.gameObject.GetComponent<DreamFragment>() && (previousHighlight == hit.transform.gameObject || (hit.transform.parent && previousHighlight == hit.transform.parent.gameObject) || (hit.transform.parent && hit.transform.parent.parent && previousHighlight == hit.transform.parent.parent.gameObject)))
                        currentHighlight = previousHighlight;
                }
            }
        }

        // if a previous one exists and it's different from the new one => reset default emission color
        if (previousHighlight != null && currentHighlight != previousHighlight)
            unhighlight(currentHighlight);
    }
}