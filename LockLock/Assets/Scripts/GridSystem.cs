using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{

    public static GridSystem Instance;

    [SerializeField] float cameraAreaKeep;
    [SerializeField] float cellSize;
    [SerializeField] int width;
    [SerializeField] int height;

    Grid<GridSlot> grid;
    [SerializeField] NodeSlot pfNodeSlot;

    public int GridWidth { get { return width; } }
    public int GridHeight { get { return height; } }

    public float CellSize { get { return cellSize; } }

    private void OnValidate()
    {
        if (width <= 0)
        {
            width = 1;
        }
        if (height <= 0)
        {
            height = 1;
        }
        if (cellSize <= 0)
        {
            cellSize = 0.01f;
        }
    }

    public NormalNode GetNodeFromGlobalIndex(Vector2Int index)
    {
        if (index.x < 0 || index.x > width - 1 || index.y < 0 || index.y > height - 1) return null;

        NodeSlot nodeSlot = grid.array[index.x, index.y].nodeSlot;
        if(nodeSlot != null)
        {
            return nodeSlot.Node;
        }

        return null;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateNode();
    }

    public void GenerateNode()
    {
        Camera main = Camera.main;
        Vector3 originPos = Vector3.zero;
        grid = new Grid<GridSlot>(width, height, cellSize, originPos, (Grid<GridSlot> g, int x, int y) => new GridSlot(x, y, g));
        Vector3 minPos = grid.GetWorldPosition(0, 0);
        Vector3 maxPos = grid.GetWorldPosition(width - 1, height - 1);
        Vector3 cameraPos = (maxPos - minPos) / 2f;
        cameraPos = new Vector3(cameraPos.x, cameraPos.y, -10f);
        main.transform.position = cameraPos;
        main.orthographicSize = ((height - 1) / 2f + width) * cellSize / main.aspect / 2f + cameraAreaKeep;
        string holderName = "nodeSlotParent";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform nodeParent = new GameObject(holderName).transform;
        nodeParent.transform.parent = transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int globalIndex = new Vector2Int(x, y);
                Vector2 position = grid.GetWorldPosition(x, y);
                // setup node slot
                GridSlot gridSlot = grid.GetValue(x, y);
                NodeSlot nodeSlot = Instantiate(pfNodeSlot, position, Quaternion.identity, nodeParent);
                gridSlot.SetNodeSlot(nodeSlot);

                nodeSlot.Setup(globalIndex);
                nodeSlot.gameObject.name = "NodeSlot_" + x + "_" + y;
            }
        }
    }
}

public class GridSlot
{
    int x;
    int y;
    Grid<GridSlot> grid;

    public NodeSlot nodeSlot;

    public GridSlot(int x_, int y_, Grid<GridSlot> grid_)
    {
        x = x_;
        y = y_;
        grid = grid_;
    }

    public void SetNodeSlot(NodeSlot nodeSlot_) => nodeSlot = nodeSlot_;

}