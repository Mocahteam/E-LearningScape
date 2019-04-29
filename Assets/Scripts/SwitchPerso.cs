using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPerso : MonoBehaviour {

    public Camera ThirdCamera;
    public Camera FirstCamera;
    public bool fpsCam = true;

    private void Start()
    {
        FirstCamera.enabled = fpsCam;
        ThirdCamera.enabled = !fpsCam;
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.L))
        {
            fpsCam = !fpsCam;
            forceUpdate();
        }
	}
   
    public void forceUpdate()
    {
        FirstCamera.enabled = fpsCam;
        ThirdCamera.enabled = !fpsCam;
    }
}
