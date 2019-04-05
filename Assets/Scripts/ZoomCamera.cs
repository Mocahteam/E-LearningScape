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
        //Zoom avant de 10 en 10
        if (Input.GetKeyDown(KeyCode.Space))
            viewCam.fieldOfView -= 10;

        //Zoom arrière de 10 en 10
        if (Input.GetKeyDown(KeyCode.B))
            viewCam.fieldOfView += 10;

        //Zoom par défaut
        if (Input.GetKeyDown(KeyCode.C))
            viewCam.fieldOfView = 60;
    }
    
}
