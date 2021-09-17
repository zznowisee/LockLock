using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{

    public static Transform GenerateMesh(Material material, float radius, string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        MeshData meshData = new MeshData(radius);
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;

        meshRenderer.material = material;
        meshFilter.mesh = mesh;
        obj.transform.parent = parent;

        return obj.transform;
    }

    class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;

        public MeshData(float width)
        {
            vertices = new Vector3[]
            {
                new Vector2(0, width / 2),
                new Vector2(1, width / 2),
                new Vector2(0, -width / 2),
                new Vector2(1, -width / 2)
            };

            triangles = new int[]
            {
                0,1,2,1,3,2
            };
        }
    }
}
