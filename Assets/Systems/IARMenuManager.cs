using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class IARMenuManager : FSystem {

    // manage IAR menu (last tab)

    private Family buttons = FamilyManager.getFamily(new AnyOfTags("IARMenuButton"));

    public IARMenuManager()
    {
        if (Application.isPlaying)
        {
            buttons.getAt(0).GetComponentInChildren<Button>().onClick.AddListener(Resume);
            buttons.getAt(1).GetComponentInChildren<Button>().onClick.AddListener(Restart);
            buttons.getAt(2).GetComponentInChildren<Button>().onClick.AddListener(ExitGame);
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

	}

    void Resume()   //resume game
    {
        IARTabNavigation.instance.closeIar();
    }

    void Restart() //restart game from beginning
    {
        GameObjectManager.loadScene("Sapiens");
    }

    void ExitGame() //open menu scene
    {
        Application.Quit();
    }
}