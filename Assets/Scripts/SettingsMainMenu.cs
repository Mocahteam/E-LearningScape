using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMainMenu : MonoBehaviour {
    public GameObject window;

	public void Show ()
    {
        GameObjectManager.setGameObjectState(window, true);
    }
    public void Hide ()
    {
        GameObjectManager.setGameObjectState(window, false);
    }
}
