using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class LineVisual : MonoBehaviour
{
    List<Vector2> points;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    EdgeCollider2D edgeCollider;
    float width = 0.5f;
    float cellSize = 6f;

    [SerializeField] Material m;
    public int VertexCount { get { return meshFilter.mesh.vertexCount; } }
    public Mesh LineMesh { get { return meshFilter.mesh; } }
    public Material Material { get { return meshRenderer.material; } set { meshRenderer.material = value; } }

    public void Setup(Vector2 startPoint)
    {
        points = new List<Vector2>();
        print(startPoint);
        points.Add(transform.position);
        points.Add(transform.position);

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();

        edgeCollider.edgeRadius = width / 2f;

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        GenerateMesh();
    }

    public void ConnectWayPoint(Vector2 position)
    {
        Vector2 dir = (position - points[points.Count - 2]).normalized;
        dir *= cellSize / 4f;

        Vector2 p0 = position - dir;
        Vector2 p1 = position + dir;

        points[points.Count - 1] = p0;

        points.Add(p1);
        points.Add(p1);

        FinishMesh();
    }

    public void ConnectNode(Vector2 position)
    {
        points[points.Count - 1] = position;

        FinishMesh();

        edgeCollider.points = points.ToArray();

    }

    public void UpdateMesh(Vector2 position)
    {
        points[points.Count - 1] = position;
        FinishMesh();
    }

    void FinishMesh()
    {
        Vector3[] verts = LineMesh.vertices;
        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;
        GetVertexFromLastTwoPoints(out p0, out p1, out p2, out p3);
        verts[verts.Length - 1] = p3;
        verts[verts.Length - 2] = p2;
        verts[verts.Length - 3] = p1;
        verts[verts.Length - 4] = p0;

        LineMesh.vertices = verts;
    }

    void GetVertexFromLastTwoPoints(out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
    {
        Vector2 dir = (points[points.Count - 1] - points[points.Count - 2]).normalized;
        Vector2 left = new Vector2(-dir.y, dir.x);
        p0 = points[points.Count - 2] + left * width * 0.5f;
        p1 = points[points.Count - 2] - left * width * 0.5f;
        p2 = points[points.Count - 1] + left * width * 0.5f;
        p3 = points[points.Count - 1] - left * width * 0.5f;
    }

    void GenerateMesh()
    {
        Vector3[] verts = new Vector3[points.Count * 2];
        int[] tris = new int[2 * (points.Count - 1) * 3];

        int vertIndex = 0;
        int trisIndex = 0;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 dir = Vector2.zero;
            if (i < points.Count - 1)
            {
                dir += points[i + 1] - points[i];
            }
            if (i > 0)
            {
                dir += points[i] - points[i - 1];
            }

            dir.Normalize();
            Vector2 left = new Vector2(-dir.y, dir.x);

            verts[vertIndex] = points[i] + left * width * 0.5f;
            verts[vertIndex + 1] = points[i] - left * width * 0.5f;

            if (i < points.Count - 1)
            {
                tris[trisIndex] = vertIndex;
                tris[trisIndex + 1] = vertIndex + 2;
                tris[trisIndex + 2] = vertIndex + 1;

                tris[trisIndex + 3] = vertIndex + 1;
                tris[trisIndex + 4] = vertIndex + 2;
                tris[trisIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            trisIndex += 6;
        }

        LineMesh.vertices = verts;
        LineMesh.triangles = tris;
    }
}
