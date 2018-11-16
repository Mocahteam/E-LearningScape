using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorWindowWithPopupDestroyed : EditorWindow
{
    void OnGUI()
    {
        {
            GUILayout.Label("Object Destroyed", EditorStyles.boldLabel);
        }
    }
}
