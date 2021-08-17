using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridSystem))]
public class NodeSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridSystem nodeSystem = target as GridSystem;

        if(GUILayout.Button("Generate Node"))
        {
            nodeSystem.GenerateNode();
        }
    }
}
