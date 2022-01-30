using UnityEngine;

public class ZoomManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("ZoomIn"))
        {
            Camera.main.fieldOfView -= 1;
            if (Camera.main.fieldOfView < 20)
                Camera.main.fieldOfView = 20;
        }

        if (Input.GetButton("ZoomOut"))
        {
            Camera.main.fieldOfView += 1;
            if (Camera.main.fieldOfView > 60)
                Camera.main.fieldOfView = 60;
        }
    }
}
