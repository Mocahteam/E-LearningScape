using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureFromCamera : MonoBehaviour
{    // Grab the camera's view when this variable is true.
    public static bool draw = false;

    // The "m_Display" is the GameObject whose Texture will be set to the captured image.
    public Image m_Display;

    private Camera myCamera;
    private RenderTexture rt;
    private bool first = true;

    private void Start()
    {
        myCamera = this.GetComponent<Camera>();
        rt = new RenderTexture(myCamera.pixelWidth, myCamera.pixelHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        myCamera.targetTexture = rt;
    }

    private void OnPostRender()
    {
        if (draw)
        {
            //Create a new texture with the width and height of the screen
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
            texture.Apply();

            //Check that the display field has been assigned in the Inspector
            if (m_Display != null)
            {
                //Give your GameObject with the renderer this texture
                m_Display.material.mainTexture = texture;
                m_Display.RecalculateMasking();
            }
            //Reset the grab state
            draw = false;
            if (first)
            {
                this.transform.parent.gameObject.GetComponentInChildren<Image>().color = Color.white;
                first = false;
            }
        }
    }
}
