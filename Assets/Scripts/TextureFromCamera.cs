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

    private Texture2D texture;

    private void Start()
    {
        myCamera = this.GetComponent<Camera>();
        float correctRatio = (float)1375 / 955;
        int newCameraX=0, newCameraY=0;
        if ((float)myCamera.pixelWidth/myCamera.pixelHeight > correctRatio)
        {
            Debug.Log(string.Concat(myCamera.pixelWidth, "/", myCamera.pixelHeight, " > ", 1375, "/", 955));
            newCameraX = (int) ((float)myCamera.pixelHeight * correctRatio);
            newCameraY = myCamera.pixelHeight;
        }
        else
        {
            Debug.Log(string.Concat(myCamera.pixelWidth, "/", myCamera.pixelHeight, " < ", 1375, "/", 955));
            newCameraX = myCamera.pixelWidth;
            newCameraY = (int)((float)myCamera.pixelWidth / correctRatio);
        }
        Debug.Log(string.Concat(newCameraX, " ", newCameraY));
        int x = (myCamera.pixelWidth - newCameraX) / 2;
        int y = (myCamera.pixelHeight - newCameraY) / 2;
        myCamera.pixelRect = new Rect(x, y, newCameraX, newCameraY);
        //changer en fonction de width mais aussi de height
        //m_Display.GetComponent<RectTransform>().localScale += Vector3.right * (0.89632f*(float)newCameraX/1375- m_Display.GetComponent<RectTransform>().localScale.x);

        m_Display.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(myCamera.pixelWidth, myCamera.pixelHeight);
        m_Display.color = new Color(1, 1, 1, 0);
        draw = true;
        rt = new RenderTexture(myCamera.pixelWidth, myCamera.pixelHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        myCamera.targetTexture = rt;
    }

    private void OnPostRender()
    {
        if (draw)
        {
            //Create a new texture with the width and height of the screen
            texture = new Texture2D(myCamera.pixelWidth, myCamera.pixelHeight, TextureFormat.RGB24, false);
            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, myCamera.pixelWidth, myCamera.pixelHeight), 0, 0, false);
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
                m_Display.color = Color.white;
                first = false;
            }
        }
    }
}
