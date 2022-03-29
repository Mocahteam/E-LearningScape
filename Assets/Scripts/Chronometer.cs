using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chronometer : MonoBehaviour
{
    public TMPro.TMP_Text UI_Chrono;
    public GameObject muteBt;
    public float sessionTimer;
    public Timer timer;

    private float d = -1; // while d == -1 no LateUpdate called

    // Update is called once per frame
    void LateUpdate()
    {
        d = sessionTimer - (Time.time - timer.startingTime);
        if (d < 0)
            d = 0;
        int hours = (int)d / 3600;
        int minutes = (int)(d % 3600) / 60;
        int seconds = (int)(d % 3600) % 60;
        UI_Chrono.text = (hours > 0 ? (hours.ToString("D2") + ":") : "") + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        UI_Chrono.color = new Color(1, d / sessionTimer, d / sessionTimer);
        if (d == 0)
        {
            GameObjectManager.setGameObjectState(muteBt, true);
            // Show mouse cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    private void OnDisable()
    {
        if (d == 0 && !MovingSystem_FPSMode.instance.Pause)
        {
            // hide mouse cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
