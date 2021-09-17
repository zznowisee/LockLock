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
    float maxLengthBtwNodes;

    public Material material;

    float finishLineLength;
    float totalLineLength;

    List<int> wayPointsIndex;
    List<IndexLength> indexLengths;







    List<Vector3> displacedVertices;











    public Mesh LineMesh { get { return meshFilter.mesh; } set { meshFilter.mesh = value; } }
    public Material Material { get { return meshRenderer.material; } set { meshRenderer.material = value; } }
    public int PointsLastIndex { get { return points.Count - 1; } }

    public Vector3 LastPointPosition { get { return (Vector3)points[PointsLastIndex] + transform.position; } }

    public void Setup(Vector2 startPoint, float width_, float cellSize_)
    {
        width = width_;
        cellSize = cellSize_;
        maxLengthBtwNodes = FindObjectOfType<GridSystem>().CellSize;
        points = new List<Vector2>();
        indexLengths = new List<IndexLength>();
        wayPointsIndex = new List<int>();
        displacedVertices = new List<Vector3>();

        points.Add(startPoint);
        indexLengths.Add(new IndexLength(PointsLastIndex, 0f));

        points.Add(startPoint);
        indexLengths.Add(new IndexLength(PointsLastIndex, 0f));

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();

        meshRenderer.sharedMaterial = material;
        
        edgeCollider.edgeRadius = width / 2f;
        LineMesh = GenerateMesh();
    }

    public void ConnectWayPoint(Vector2 position)
    {
        finishLineLength += cellSize;
        totalLineLength = finishLineLength;

        points[PointsLastIndex] = position;
        indexLengths[PointsLastIndex].length = totalLineLength;
        //anocher point
        points.Add(position);
        indexLengths.Add(new IndexLength(PointsLastIndex, finishLineLength));
        wayPointsIndex.Add(points.Count - 2);
        //move point
        points.Add(position);
        indexLengths.Add(new IndexLength(PointsLastIndex, finishLineLength));

        LineMesh = GenerateMesh();
    }

    public void SeparateWayPoint()
    {
        finishLineLength -= cellSize;

        points.RemoveRange(PointsLastIndex - 1, 2);
        indexLengths.RemoveRange(indexLengths.Count - 2, 2);

        float length = (points[points.Count - 1] - points[points.Count - 2]).magnitude;
        totalLineLength = finishLineLength + length;

        indexLengths[PointsLastIndex].length = totalLineLength;

        wayPointsIndex.RemoveAt(wayPointsIndex.Count - 1);
        LineMesh = GenerateMesh();
    }

    public void ConnectNode(Vector2 position)
    {
        finishLineLength += cellSize;
        totalLineLength = finishLineLength;

        points[PointsLastIndex] = position;
        indexLengths[PointsLastIndex].length = finishLineLength;

        edgeCollider.points = points.ToArray();
        Material.SetFloat("_Length", finishLineLength);
        LineMesh = GenerateMesh();
    }

    public void UpdateMesh(Vector2 position)
    {
        float lengthBtwTwoNodes = (position - points[points.Count - 2]).magnitude;

        if (lengthBtwTwoNodes > maxLengthBtwNodes)
        {
            lengthBtwTwoNodes = maxLengthBtwNodes;
        }

        totalLineLength = finishLineLength + lengthBtwTwoNodes;


        points[PointsLastIndex] = position;

        float length = (points[points.Count - 1] - points[points.Count - 2]).magnitude;
        if (length > maxLengthBtwNodes) length = maxLengthBtwNodes;
        totalLineLength = finishLineLength + length;

        indexLengths[PointsLastIndex].length = totalLineLength;

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

    [System.Serializable]
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
