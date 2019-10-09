using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPerso : MonoBehaviour {

    public Camera ThirdCamera;
    public Camera FirstCamera;
    public bool fpsCam = true;
    public GameObject skin;
    public GameObject iarRef;

    private void Start()
    {
        FirstCamera.enabled = fpsCam;
        ThirdCamera.enabled = !fpsCam;
        GameObjectManager.setGameObjectState(skin, !fpsCam);
        GameObjectManager.setGameObjectState(skin.GetComponent<LinkedWith>().link, !fpsCam);
    }

    void Update () {
        if (Input.GetButtonDown("SwitchView") && !iarRef.activeInHierarchy)
        {
            fpsCam = !fpsCam;
            forceUpdate();
        }
	}
   
    public void forceUpdate()
    {
        FirstCamera.enabled = fpsCam;
        ThirdCamera.enabled = !fpsCam;
        GameObjectManager.setGameObjectState(skin, !fpsCam);
        GameObjectManager.setGameObjectState(skin.GetComponent<LinkedWith>().link, !fpsCam);
    }
}
