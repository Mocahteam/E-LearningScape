using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class ThirdPersonCameraControler : MonoBehaviour {

    public Transform player;
    public Camera yourCam;

    private void Update()
    {
        if (yourCam.enabled)
        {
            float X = Input.GetAxis("Mouse X") * player.GetComponent<FirstPersonController>().m_MouseLook.XSensitivity;
            float Y = Input.GetAxis("Mouse Y") * player.GetComponent<FirstPersonController>().m_MouseLook.YSensitivity;

            player.Rotate(0, X, 0); // Player rotates on Y axis, your Cam is child, then rotates too

            // To scurity check to not rotate 360º 
            if (yourCam.transform.eulerAngles.x + (-Y) <= 80 || yourCam.transform.eulerAngles.x + (-Y) >= 280)
            {
                yourCam.transform.RotateAround(player.position, yourCam.transform.right, -Y);
            }
        }
    }
}
