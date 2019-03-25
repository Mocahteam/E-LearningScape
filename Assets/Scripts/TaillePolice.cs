using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaillePolice : MonoBehaviour {
    private TMP_Text TM_Police;
    private float tm_FontSize = 14.0f;
    // Use this for initialization
    void Start () {
        TM_Police = GetComponent<TMP_Text>();
	}
	
	// Update is called once per frame
	void Update () {
        TM_Police.fontSize = tm_FontSize;
	}

    public void SetSize (float size)
    {
        tm_FontSize = size;
    }


}

