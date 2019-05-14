using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMainMenu : MonoBehaviour
{
    public GameObject parent; //define popup when window popup close
    public GameObject defaultUIinParent; //define on which button cursor must be on parent window 
    public GameObject window; //define actual popup
    public GameObject defaultUIinWindow; //define on which button curso must be on window where gamer is

    public void Show ()
    {
        GameObjectManager.setGameObjectState(window, true);
        EventSystem.current.SetSelectedGameObject(defaultUIinWindow); //Always position cursor on default button define in inspector object 
        //EventSystem.current.currentSelectedGameObject;
        if (parent)
            GameObjectManager.setGameObjectState(parent, false);
    }
    public void Hide ()
    {
        GameObjectManager.setGameObjectState(window, false);
        if (parent)
        {
            GameObjectManager.setGameObjectState(parent, true);
            EventSystem.current.SetSelectedGameObject(defaultUIinParent); //if we back on parent window so position cursor on default button choose in inspector object 
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }
}
