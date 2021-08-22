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

    float totalLength;
    float currentLength;

    List<int> wayPointsIndex;
    List<IndexLength> indexLengths;

    public int VertexCount { get { return meshFilter.mesh.vertexCount; } }
    public Mesh LineMesh { get { return meshFilter.mesh; } set { meshFilter.mesh = value; } }
    public Material Material { get { return meshRenderer.material; } set { meshRenderer.material = value; } }
    public int LastIndex { get { return points.Count - 1; } }

    public void Setup(Vector2 startPoint, float width_, float cellSize_)
    {
        width = width_;
        cellSize = cellSize_;

        points = new List<Vector2>();
        indexLengths = new List<IndexLength>();
        wayPointsIndex = new List<int>();

        points.Add(startPoint);
        indexLengths.Add(new IndexLength(LastIndex, 0f));

        points.Add(startPoint);
        indexLengths.Add(new IndexLength(LastIndex, 0f));

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();

        edgeCollider.edgeRadius = width / 2f;
        LineMesh = GenerateMesh();
    }

    public void ConnectWayPoint(Vector2 position)
    {
        totalLength += cellSize;
        currentLength = totalLength;

        points[LastIndex] = position;
        indexLengths[LastIndex].length = currentLength;

        points.Add(position);
        indexLengths.Add(new IndexLength(LastIndex, currentLength));
        wayPointsIndex.Add(points.Count - 2);

        points.Add(position);
        indexLengths.Add(new IndexLength(LastIndex, currentLength));
        LineMesh = GenerateMesh();
    }

    public void SeparateWayPoint()
    {
        totalLength -= cellSize;

        points.RemoveAt(LastIndex);
        points.RemoveAt(LastIndex);
        print(points.Count);
        indexLengths.RemoveAt(indexLengths.Count - 1);
        indexLengths.RemoveAt(indexLengths.Count - 1);
        wayPointsIndex.RemoveAt(wayPointsIndex.Count - 1);
        LineMesh = GenerateMesh();
    }

    public void ConnectNode(Vector2 position)
    {
        totalLength += cellSize;
        currentLength = totalLength;

        points[LastIndex] = position;
        indexLengths[LastIndex].length = currentLength;

        //ChangeLastTwoVertPos();
        edgeCollider.points = points.ToArray();
        Material.SetFloat("_Length", totalLength);
        LineMesh = GenerateMesh();
    }

    public void UpdateMesh(Vector2 position)
    {
        float length = (points[points.Count - 1] - points[points.Count - 2]).magnitude;
        currentLength = totalLength + length;

        points[LastIndex] = position;
        indexLengths[LastIndex].length = currentLength;

        //ChangeLastTwoVertPos();
        LineMesh = GenerateMesh();
        Material.SetFloat("_Length", currentLength);
    }

    #region Refac
    /*void ChangeLastTwoVertPos()
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
    }*/
    #endregion

    Mesh GenerateMesh()
    {
        Vector3[] verts = new Vector3[points.Count * 2];
        int[] tris = new int[2 * (points.Count - 1) * 3];
        Vector2[] uvs = new Vector2[verts.Length];

        int vertIndex = 0;
        int trisIndex = 0;
        int uvIndex = 0;

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

            //float uvValue = i / ((float)points.Count - 1 - pointInWayPoint);
            //float uvValue = uvIndex / ((float)points.Count - 1 - wayPointsIndex.Count);
            float uvValue = indexLengths[i].length / currentLength;
            uvs[vertIndex] = new Vector2(uvValue, 0);
            uvs[vertIndex + 1] = new Vector2(uvValue, 1);
            if (!wayPointsIndex.Contains(i))
            {
                uvIndex++;
            }

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

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        return mesh;
    }

    public class IndexLength
    {
        public int index;
        public float length;

        public IndexLength(int index_, float length_)
        {
            index = index_;
            length = length_;
        }
    }
}
