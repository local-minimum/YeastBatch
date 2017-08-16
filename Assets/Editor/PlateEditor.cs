using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Board))]
public class PlateEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Compose and Cast Media"))
        {
            (target as Board).SetMediaComposition();
            (target as Board).CastMedia();
        }
    }
}
