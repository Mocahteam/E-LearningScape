using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMainMenu : MonoBehaviour
{
    public GameObject parent;
    public GameObject window;

	public void Show ()
    {
        GameObjectManager.setGameObjectState(window, true);
        if (parent)
            GameObjectManager.setGameObjectState(parent, false);
    }
    public void Hide ()
    {
        GameObjectManager.setGameObjectState(window, false);
        if (parent)
            GameObjectManager.setGameObjectState(parent, true);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }
}
