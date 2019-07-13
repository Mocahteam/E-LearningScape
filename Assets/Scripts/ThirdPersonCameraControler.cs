using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraControler : MonoBehaviour {

    public float mouseSpeed = 3;
    public Transform player;
    public Camera yourCam;

    private void Update()
    {
        float X = Input.GetAxis("Mouse X") * mouseSpeed;
        float Y = Input.GetAxis("Mouse Y") * mouseSpeed;

        player.Rotate(0, X, 0); // Player rotates on Y axis, your Cam is child, then rotates too

        // To scurity check to not rotate 360º 
        if (yourCam.transform.eulerAngles.x + (-Y) <= 80 || yourCam.transform.eulerAngles.x + (-Y) >= 280)
        {
            yourCam.transform.RotateAround(player.position, yourCam.transform.right, -Y);
        }
    }
}
