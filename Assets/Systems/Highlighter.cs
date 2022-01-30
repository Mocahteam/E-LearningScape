using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class Highlighter : FSystem {

    // Highlight all in game interactable GameObjects
    private Family f_needHighlight = FamilyManager.getFamily(new AnyOfComponents(typeof(Selectable), typeof(ToggleableGO), typeof(Pickable)), new AllOfComponents(typeof(PointerOver)), new NoneOfComponents(typeof(Highlighted)));
    private Family f_needUnhighlight = FamilyManager.getFamily(new AnyOfComponents(typeof(Selectable), typeof(ToggleableGO), typeof(Pickable)), new AllOfComponents(typeof(Highlighted)), new NoneOfComponents(typeof(PointerOver)));
    private Family f_Highlighted = FamilyManager.getFamily(new AllOfComponents(typeof(Highlighted), typeof(PointerOver)));

    public static Highlighter instance;

    public Highlighter()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_needUnhighlight.addEntryCallback(unhighlight);
    }

    private void unhighlight(GameObject currentHighlight)
    {
        Renderer[] tmpRendererList = currentHighlight.GetComponentsInChildren<Renderer>();
        int nb = tmpRendererList.Length;
        for (int i = 0; i < nb; i++)
        {
            if (tmpRendererList[i].material.HasProperty("_EmissionColor") && tmpRendererList[i].GetComponent<Highlighted>() && !(tmpRendererList[i] is LineRenderer))
            {
                Color c = tmpRendererList[i].GetComponent<Highlighted>().previousColor;
                if (c != null)
                    tmpRendererList[i].material.SetColor("_EmissionColor", c);
                // Remove Highlighted component to this GameObject
                GameObjectManager.removeComponent<Highlighted>(tmpRendererList[i].gameObject);
            }
        }

        // check if currentHighlight doesn't contain Renderer component, if ture remove Highlighted component
        if (!currentHighlight.GetComponent<Renderer>())
            GameObjectManager.removeComponent<Highlighted>(currentHighlight);

    }

    private void highlight (GameObject currentHighlight)
    {
        // Update renderer and highlight game object
        Renderer[] tmpRendererList = currentHighlight.GetComponentsInChildren<Renderer>();
        int nb = tmpRendererList.Length;
        for (int i = 0; i < nb; i++)
        {
            if (tmpRendererList[i].material.HasProperty("_EmissionColor") && !(tmpRendererList[i] is LineRenderer) && tmpRendererList[i].GetComponentInParent<DreamFragment>() == null) // avoid to process lineRenderer and dream fragment childs
            {
                // Add Highlighted component to this GameObject and save current emission color
                GameObjectManager.addComponent<Highlighted>(tmpRendererList[i].gameObject, new { previousColor = tmpRendererList[i].material.GetColor("_EmissionColor") });
                // Enable emission and hightlight target
                if (tmpRendererList[i].gameObject.name != ("BoardTexture"))
                {
                    tmpRendererList[i].material.EnableKeyword("_EMISSION");
                    tmpRendererList[i].material.SetColor("_EmissionColor", Color.yellow * Mathf.LinearToGammaSpace(0.8f));
                }
            }
        }

        // check if currentHighlight doesn't contain Renderer component, if true add anyway an Highlighted component
        if (!currentHighlight.GetComponent<Renderer>())
            GameObjectManager.addComponent<Highlighted>(currentHighlight);

        GameObjectManager.addComponent<PlaySound>(currentHighlight, new { id = 2 }); // id refer to FPSController AudioBank
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        foreach (GameObject hl in f_Highlighted)
            unhighlight(hl);
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {

    }

    protected override void onProcess(int familiesUpdateCount)
    {
        foreach (GameObject needHL in f_needHighlight) // not use callback on this family to avoid to call highlight on Pause
            highlight(needHL);
    }
}