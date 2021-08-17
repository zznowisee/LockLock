using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Test test = target as Test;
        if(GUILayout.Button("Generate Mesh"))
        {
            //test.GenerateMesh();
        }
    }
}
