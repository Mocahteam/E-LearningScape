using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPerso : MonoBehaviour {

    public Camera ThirdCamera;
    public Camera FirstCamera;
    bool fpsCam = true;

    private void Start()
    {
        FirstCamera.enabled = fpsCam;
        ThirdCamera.enabled = !fpsCam;
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.L))
        {
            fpsCam = !fpsCam;
            FirstCamera.enabled = fpsCam;
            ThirdCamera.enabled = !fpsCam;
        }
	}
   

}
