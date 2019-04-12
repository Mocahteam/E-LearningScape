using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class ZoomCamera : MonoBehaviour {
    private Camera viewCam;
    public float minZoom;
    public float maxZoom;
    

    private void Start()
    {
        viewCam = GetComponent<Camera>();
    }

    private void Update()
    {
        //Zoom avant de 10 en 10
        if (Input.GetKeyDown(KeyCode.PageUp) && viewCam.fieldOfView > minZoom)
        {
            viewCam.fieldOfView -= 10;
            this.transform.parent.GetComponent<FirstPersonController>().m_MouseLook.XSensitivity -= 0.5f;
            this.transform.parent.GetComponent<FirstPersonController>().m_MouseLook.YSensitivity -= 0.5f;
        }
            

        //Zoom arrière de 10 en 10
        if (Input.GetKeyDown(KeyCode.PageDown) && viewCam.fieldOfView < maxZoom)
        {
            viewCam.fieldOfView += 10;
            this.transform.parent.GetComponent<FirstPersonController>().m_MouseLook.XSensitivity += 0.5f;
            this.transform.parent.GetComponent<FirstPersonController>().m_MouseLook.YSensitivity += 0.5f;
        }
           

        //Zoom par défaut
        if (Input.GetKeyDown(KeyCode.Home))
        {
            viewCam.fieldOfView = 60;
            this.transform.parent.GetComponent<FirstPersonController>().m_MouseLook.XSensitivity = 2.5f;
            this.transform.parent.GetComponent<FirstPersonController>().m_MouseLook.YSensitivity = 2.5f;
        }
            
    }
    
}
