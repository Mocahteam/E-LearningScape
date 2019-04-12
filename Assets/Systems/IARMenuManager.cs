using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class IARMenuManager : FSystem {

    // manage IAR menu (last tab)

    private Family f_buttons = FamilyManager.getFamily(new AnyOfTags("IARMenuButton"));

    public IARMenuManager()
    {
        if (Application.isPlaying)
        {
            f_buttons.getAt(0).GetComponentInChildren<Button>().onClick.AddListener(Resume);
            //getAt(1) is game setting button and it doesn't need of IARMenuManager. It's just position at the second place 
            f_buttons.getAt(2).GetComponentInChildren<Button>().onClick.AddListener(Restart);
            f_buttons.getAt(3).GetComponentInChildren<Button>().onClick.AddListener(ExitGame);
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
        GameObjectManager.loadScene(SceneManager.GetActiveScene().name);
    }

    void ExitGame() //open menu scene
    {
        Application.Quit();
    }
}