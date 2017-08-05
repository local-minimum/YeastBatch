using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Plate))]
public class PlateEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Compose and Cast Media"))
        {
            (target as Plate).SetMediaComposition();
            (target as Plate).CastMedia();
        }
    }
}
