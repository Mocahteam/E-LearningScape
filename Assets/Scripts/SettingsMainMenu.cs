using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMainMenu : MonoBehaviour {
    public GameObject window;

	public void Show ()
    {
        window.SetActive(true);
    }
    public void Hide ()
    {
        window.SetActive(false);
    }
}
