using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TailleContour : MonoBehaviour {
    private TMP_Text TM_contour;
    private float widthContour;
	// Use this for initialization
	void Start () {
        TM_contour = GetComponent<TMP_Text>();
	}
	
	// Update is called once per frame
	void Update () {
        TM_contour.outlineWidth = widthContour;
		
	}

    public void setWidth (float size)
    {
        widthContour = size;
    }
}
