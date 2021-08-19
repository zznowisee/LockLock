using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Material material;
    Transform mesh;

    private void Update()
    {
        //mesh.GetComponent<MeshFilter>().mesh.vertices[1] = 
    }

    public void GenerateMesh()
    {
        Transform mesh = MeshGenerator.GenerateMesh(material, 0.5f, "Bird", transform);
    }
}
