﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarnItem : MonoBehaviour {
    [HideInInspector]
    public float alpha = 0;
    [HideInInspector]
    public int way = 1;

    public float maxAlpha = 0.5f;
    public float speed = 0.8f;
}