using UnityEngine;
using FYFY;
using Valve.VR;

public class TabletSystem : FSystem {

    private Family tabletR1buttons = FamilyManager.getFamily(new AnyOfTags("Q-R1"), (new AllOfComponents(typeof(Pointable)))); //questions of the room 2 (tablet)

    private GameObject q;

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
        foreach(GameObject go in tabletR1buttons)
        {
            Pointable p = go.GetComponent<Pointable>();
            // Si le boutton est selectionné, on ouvre le clavier
            if(p.selected)
            {
                p.selected = false;
                SteamVR.instance.overlay.ShowKeyboard(0, 0, "Input", 50, "", false, 0);
                q = go;
                SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
            }
        }
	}

    // Fermeture du clavier
    public void OnKeyboardClosed(VREvent_t args)
    {
        System.Text.StringBuilder textBuilder = new System.Text.StringBuilder(1024);
        SteamVR.instance.overlay.GetKeyboardText(textBuilder, 1024);
        string text = textBuilder.ToString();
        Debug.Log(q);
    }
}