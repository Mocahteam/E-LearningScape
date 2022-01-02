using UnityEngine;
using FYFY;

public class ToggleObject : FSystem {

    // This system enables to manage in game toggleable objects

	private Family f_toggleable = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleableGO), typeof(Highlighted), typeof(Animator))); // Highlighted is dynamically added by Highlither system
    private Family f_wrongChair = FamilyManager.getFamily(new AnyOfTags("Chair"), new AllOfComponents(typeof(ToggleableGO)), new NoneOfComponents(typeof(IsSolution)));

    //temporary variables
    private GameObject tmpGO;
    private ToggleableGO tmpToggleableGO;

    public static ToggleObject instance;

    public ToggleObject(){
        if (Application.isPlaying)
        {
            instance = this;
        }
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (Input.GetButtonDown("Fire1"))
        {
            int nbToggleable = f_toggleable.Count;
            for (int i = 0; i < nbToggleable; i++)
            {
                tmpGO = f_toggleable.getAt(i);
                tmpToggleableGO = tmpGO.GetComponent<ToggleableGO>();
                Animator anim = tmpGO.GetComponent<Animator>();

                tmpToggleableGO.toggled = !tmpToggleableGO.toggled;
                if (tmpToggleableGO.toggled)
                {
                    anim.SetTrigger("turnOn");
                    if (tmpGO.name == "boite")
                    {
                        // puzzle enigma require chest opened
                        GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = LoadGameContent.internalGameContent.virtualPuzzle ? "turnOn_VirtualPuzzleEnigma" : "turnOn_PhysicalPuzzleEnigma", performedBy = "player" });
                        // lamp enigma require chest opened also
                        GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = "turnOn_LampEnigma", performedBy = "player" });
                    }
                    else
                        // case for chairs and table
                        GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { name = "turnOn", performedBy = "player", family = tmpGO.GetComponent<IsSolution>() ? null : f_wrongChair });
                }
                else
                {
                    anim.SetTrigger("turnOff");
                    if (tmpGO.name == "boite")
                    {
                        // When the chest is closed, we have to propagate information both in puzzle and lamp enigmas
                        GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = LoadGameContent.internalGameContent.virtualPuzzle ? "turnOff_VirtualPuzzleEnigma" : "turnOff_PhysicalPuzzleEnigma", performedBy = "player" });
                        GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = "turnOff_LampEnigma", performedBy = "player" });
                    }
                    else
                        GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { name = "turnOff", performedBy = "player", family = tmpGO.GetComponent<IsSolution>() ? null : f_wrongChair });
                }

                GameObjectManager.addComponent<ActionPerformedForLRS>(tmpGO, new { verb = "interacted", objectType = "toggable", objectName = tmpGO.name });
            }
        }
    }
}