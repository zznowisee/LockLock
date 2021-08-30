using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class LineVisual : MonoBehaviour
{
    public List<Vector2> points;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    EdgeCollider2D edgeCollider;

    float width = 0.5f;
    float cellSize = 6f;

    public Material material;

    float finishLineLength;
    float totalLineLength;

    List<int> wayPointsIndex;
    List<IndexLength> indexLengths;

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

        meshRenderer.sharedMaterial = material;
        
        edgeCollider.edgeRadius = width / 2f;
        LineMesh = GenerateMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            points.Add(Vector2.one);
        }
    }

    public void ConnectWayPoint(Vector2 position)
    {
        finishLineLength += cellSize;
        totalLineLength = finishLineLength;

        points[LastIndex] = position;
        indexLengths[LastIndex].length = totalLineLength;
        //anocher point
        points.Add(position);
        indexLengths.Add(new IndexLength(LastIndex, finishLineLength));
        wayPointsIndex.Add(points.Count - 2);
        //move point
        points.Add(position);
        indexLengths.Add(new IndexLength(LastIndex, finishLineLength));

        LineMesh = GenerateMesh();
    }

    public void SeparateWayPoint()
    {
        finishLineLength -= cellSize;
        points.RemoveRange(LastIndex - 1, 2);
        indexLengths.RemoveRange(LastIndex - 1, 2);

        wayPointsIndex.RemoveAt(wayPointsIndex.Count - 1);
        LineMesh = GenerateMesh();
    }

    public void ConnectNode(Vector2 position)
    {
        finishLineLength += cellSize;
        totalLineLength = finishLineLength;

        points[LastIndex] = position;
        indexLengths[LastIndex].length = finishLineLength;

        edgeCollider.points = points.ToArray();
        Material.SetFloat("_Length", finishLineLength);
        LineMesh = GenerateMesh();
    }

    public void UpdateMesh(Vector2 position)
    {
        points[LastIndex] = position;

        float length = (points[points.Count - 1] - points[points.Count - 2]).magnitude;
        totalLineLength = finishLineLength + length;

        indexLengths[LastIndex].length = totalLineLength;

        LineMesh = GenerateMesh();
        Material.SetFloat("_Length", totalLineLength);
    }

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
            float uvValue = indexLengths[i].length / totalLineLength;
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
