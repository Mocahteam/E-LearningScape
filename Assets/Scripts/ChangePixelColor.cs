using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePixelColor : MonoBehaviour {

    private int radius = 50;
    private Color eraseColor;

	// Use this for initialization
	void Start ()
    {
        eraseColor = this.transform.parent.gameObject.GetComponent<Renderer>().material.color;
        if (!this.GetComponent<Renderer>().material.mainTexture)
            this.GetComponent<Renderer>().material.mainTexture = new Texture2D(1480, 1070);
        this.GetComponent<Renderer>().material.renderQueue = 2002;
        Texture2D tex = (Texture2D) this.GetComponent<Renderer>().material.mainTexture;
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
        if (WhiteBoardManager.eraserDragged)
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
                    hit.transform.gameObject.GetComponent<Renderer>().material.mainTexture = new Texture2D(1480, 1070);
                }
                Texture2D tex = (Texture2D)hit.transform.gameObject.GetComponent<Renderer>().material.mainTexture;
                int pointX = (int)(uv.x * tex.width);
                int pointY = (int)(uv.y * tex.height);
                int xSym, ySym;
                for(int x = pointX - radius; x < pointX; x++)
                {
                    for (int y = pointY - radius; y < pointY; y++)
                    {
                        if((x- pointX) * (x - pointX) + (y - pointY) * (y - pointY) < radius * radius)
                        {
                            xSym = pointX * 2 - x;
                            ySym = pointY * 2 - y;
                            if(x > 0 && x < tex.width)
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
                tex.Apply();
            }
        }
    }
}
