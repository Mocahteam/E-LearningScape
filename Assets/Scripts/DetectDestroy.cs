using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
public class DetectDestroy : MonoBehaviour {

#if UNITY_EDITOR
    private void OnDestroy()
    {
        if (Time.frameCount != 0 && Time.renderedFrameCount != 0)
        {
            Debug.Log(string.Concat(this.gameObject.name, " has been destroyed."));
            EditorWindow window = EditorWindow.CreateInstance<EditorWindowWithPopupDestroyed>();
            window.Show();
        }
    }
#endif
}
