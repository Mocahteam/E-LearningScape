using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCamera : MonoBehaviour {
    private Camera viewCam;
    

    private void Start()
    {
        viewCam = GetComponent<Camera>();
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
            viewCam.fieldOfView -= 10;
        if (viewCam.fieldOfView == 10)
            viewCam.fieldOfView = 10;

        if (Input.GetKeyDown(KeyCode.M))
            viewCam.fieldOfView += 10;
    }

    /*public void zoomCam (float loupe)
    {
        zoom = loupe;
    }*/
    
}
