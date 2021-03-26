using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePixelColor : MonoBehaviour {

    private int textWidth = 265; // original 1070
    private int textHeight = 370; // original 1480

    private int radius = 10; // original 40
    private Color eraseColor;

    private int oldPointX = int.MinValue;
    private int oldPointY = int.MinValue;

    // Use this for initialization
    void Start ()
    {
        eraseColor = this.transform.parent.gameObject.GetComponent<Renderer>().material.color;
        Material localMaterial = this.GetComponent<Renderer>().material;
        if (!localMaterial.mainTexture)
            localMaterial.mainTexture = new Texture2D(textWidth, textHeight);
        //localMaterial.renderQueue = 3002; // occludable words are set to 2001 and not occludable words are set to 2003 (see WhiteBoardManager.cs)

        Texture2D tex = (Texture2D)localMaterial.mainTexture;
        Color[] colors = tex.GetPixels();
        int nb = colors.Length;
        for(int i = 0; i < nb; i++)
            colors[i] = Color.white - Color.black;
        tex.SetPixels(colors);
        tex.Apply();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        Event evt = Event.current;
        if (evt.isMouse && Input.GetButton("Fire1") && WhiteBoardManager.eraserDragged)
        {
            // Send a ray to collide with the plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (this.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                // Find the u,v coordinate of the Texture
                Vector2 uv;
                uv.x = (hit.point.x - hit.collider.bounds.min.x) / hit.collider.bounds.size.x;
                uv.y = (hit.point.y - hit.collider.bounds.min.y) / hit.collider.bounds.size.y;
                // Paint it red
                if (!(Texture2D)hit.transform.gameObject.GetComponent<Renderer>().material.mainTexture)
                {
                    hit.transform.gameObject.GetComponent<Renderer>().material.mainTexture = new Texture2D(textWidth, textHeight);
                }
                Texture2D tex = (Texture2D)hit.transform.gameObject.GetComponent<Renderer>().material.mainTexture;
                int pointX = (int)(uv.x * tex.width);
                int pointY = (int)(uv.y * tex.height);
                if (oldPointX == int.MinValue)
                    oldPointX = pointX;
                if (oldPointY == int.MinValue)
                    oldPointY = pointY;
                int stepX = 10;
                if (pointX < oldPointX)
                    stepX = -10;
                int stepY = 10;
                if (pointY < oldPointY)
                    stepY = -10;
                int xSym, ySym;
                while (oldPointX != pointX || oldPointY != pointY)
                {
                    if (Mathf.Abs(oldPointX - pointX) <= 10)
                        oldPointX = pointX;
                    if (Mathf.Abs(oldPointY - pointY) <= 10)
                        oldPointY = pointY;
                    if (oldPointX != pointX)
                        oldPointX += stepX;
                    if (oldPointY != pointY)
                        oldPointY += stepY;
                    for (int x = oldPointX - radius; x <= oldPointX; x++)
                    {
                        for (int y = oldPointY - radius; y <= oldPointY; y++)
                        {
                            if ((x - oldPointX) * (x - oldPointX) + (y - oldPointY) * (y - oldPointY) < radius * radius)
                            {
                                xSym = oldPointX * 2 - x;
                                ySym = oldPointY * 2 - y;
                                if (x > 0 && x < tex.width)
                                {
                                    if (y > 0 && y < tex.height)
                                    {
                                        tex.SetPixel(x, y, eraseColor);
                                    }
                                    if (ySym > 0 && ySym < tex.height)
                                    {
                                        tex.SetPixel(x, ySym, eraseColor);
                                    }
                                }
                                if (xSym > 0 && xSym < tex.width)
                                {
                                    if (y > 0 && y < tex.height)
                                    {
                                        tex.SetPixel(xSym, y, eraseColor);
                                    }
                                    if (ySym > 0 && ySym < tex.height)
                                    {
                                        tex.SetPixel(xSym, ySym, eraseColor);
                                    }
                                }
                            }
                        }
                    }
                }
                tex.Apply();
            }
        }
    }
}
