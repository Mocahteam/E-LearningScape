using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
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
#endif