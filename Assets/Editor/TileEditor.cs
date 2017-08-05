using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tile)), CanEditMultipleObjects]
public class TileEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Connect Neighbours"))
        {
            (target as Tile).SetNeighbours();
        }
    }
}
