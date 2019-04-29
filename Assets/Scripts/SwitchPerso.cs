using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPerso : MonoBehaviour {

    public Camera ThirdCamera;
    public Camera FirstCamera;
    public bool fpsCam = true;
    public GameObject capsule;

    private void Start()
    {
        FirstCamera.enabled = fpsCam;
        ThirdCamera.enabled = !fpsCam;
        GameObjectManager.setGameObjectState(capsule, !fpsCam);
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
        GameObjectManager.setGameObjectState(capsule, !fpsCam);
    }
}
