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
            //Debug.Log(string.Concat(myCamera.pixelWidth, "/", myCamera.pixelHeight, " > ", 1375, "/", 955));
            newCameraX = (int) ((float)myCamera.pixelHeight * correctRatio);
            newCameraY = myCamera.pixelHeight;
        }
        else
        {
            //Debug.Log(string.Concat(myCamera.pixelWidth, "/", myCamera.pixelHeight, " < ", 1375, "/", 955));
            newCameraX = myCamera.pixelWidth;
            newCameraY = (int)((float)myCamera.pixelWidth / correctRatio);
        }
        float a1 = (0.7829f - 0.89632f) / (1403 - 1375);
        float b1 = 0.89632f - a1 * 1375;
        float y1 = a1 * newCameraX + b1;
        float a2 = (1.0625f - 1.0847f) / (975 - 955);
        float b2 = 1.0847f - a2 * 955;
        float y2 = a2 * newCameraY + b2;
        m_Display.GetComponent<RectTransform>().localScale = new Vector3(y1, y2, m_Display.GetComponent<RectTransform>().localScale.z);
        int x = (myCamera.pixelWidth - newCameraX) / 2;
        int y = (myCamera.pixelHeight - newCameraY) / 2;
        myCamera.pixelRect = new Rect(x, y, newCameraX, newCameraY);

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
