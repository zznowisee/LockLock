using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    CustomSystem inputSystem;

    [SerializeField] float cameraAreaKeep;
    [SerializeField] float cellSize;
    [SerializeField] int width;
    [SerializeField] int height;

    Grid<GridSlot> grid;

    [SerializeField] Node pfNode;
    [SerializeField] NodeSlot pfNodeSlot;

    [HideInInspector] public List<Train> activeTrains = new List<Train>();

    public TrainSetup trainSetup;


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

    void Awake()
    {
        Instance = this;
        inputSystem = FindObjectOfType<CustomSystem>();
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
                gridSlot.SetNodeSlot(nodeSlot.transform);

                nodeSlot.Setup(globalIndex);
                nodeSlot.gameObject.name = "NodeSlot_" + x + "_" + y;
                // setup node
                //Node node = Instantiate(pfNode, position, Quaternion.identity, nodeParent);
                //node.gameObject.name = "Node_" + x + "_" + y;
                //gridSlot.SetNode(node);

                if(x == 1 && y == 0)
                {
                    nodeSlot.gameObject.SetActive(false);
                }
            }
        }
    }
}

public class GridSlot
{
    int x;
    int y;
    Grid<GridSlot> grid;

    Transform nodeSlot;

    public GridSlot(int x_, int y_, Grid<GridSlot> grid_)
    {
        x = x_;
        y = y_;
        grid = grid_;
    }

    public void SetNodeSlot(Transform nodeSlot_) => nodeSlot = nodeSlot_;

}