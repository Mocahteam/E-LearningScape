using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DetectDestroy : MonoBehaviour {

    private void OnDestroy()
    {
        if (Time.frameCount != 0 && Time.renderedFrameCount != 0)
        {
            Debug.Log(string.Concat(this.gameObject.name, " has been destroyed."));
            EditorWindow window = EditorWindow.CreateInstance<EditorWindowWithPopupDestroyed>();
            window.Show();
        }
    }
}
