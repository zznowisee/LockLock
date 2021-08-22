using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomSystem : MonoBehaviour
{
    public static Action OnStartPlay;

    [Header("Layer")]
    [SerializeField] LayerMask nodeLayer;
    [SerializeField] LayerMask lineLayer;
    [SerializeField] LayerMask nodeSlotLayer;
    [SerializeField] LayerMask wayPointLayer;
    [Header("Prefab")]
    [SerializeField] Transform pfTrain;
    [SerializeField] Transform pfStation;
    [SerializeField] Transform pfNode;

    List<Station> stations = new List<Station>();

    public List<SwitchInfo> switchInfos;
    public List<TrainInfo> activeTrains;

    public enum InputState
    {
        NullSelect,
        NodeSlotSelect,
        NodeSelect,
        LineSelect,
        DrawLine
    }

    public enum GameState
    {
        Edit,
        Play
    }

    int[] stationIndices;
    int[] trainIndices;

    public GameState gameState = GameState.Edit;
    public InputState inputState = InputState.NullSelect;

    NodeSlot currentNodeSlot = null;
    Node currentNode = null;
    Line selectLine = null;
    Line currentControlLine = null;
    Line currentDrawingLine = null;

    WayPointNode currentWayPoint = null;
    WayPointNode preWayPoint = null;

    private Transform nodeParent;

    Transform train;

    private void Awake()
    {
        nodeParent = transform.Find("nodeParent");

        trainIndices = stationIndices = new int[]
        {
            -1,-1,-1,-1
        };
    }

    void Start()
    {
        activeTrains = new List<TrainInfo>();
        switchInfos = new List<SwitchInfo>();
    }

    public void AddSwitchInfo(Node targetNode)
    {
        bool hasNode = false;
        for (int i = 0; i < switchInfos.Count; i++)
        {
            if (switchInfos[i].switchNode == targetNode)
            {
                SwitchInfo si = new SwitchInfo() { passTimes = switchInfos[i].passTimes + 1, switchNode = switchInfos[i].switchNode };
                hasNode = true;
                switchInfos[i] = si;
            }
        }
        if (!hasNode)
        {
            switchInfos.Add(new SwitchInfo() { passTimes = 1, switchNode = targetNode });
        }
    }

    public void Run()
    {
        if(gameState == GameState.Play)
        {
            return;
        }

        gameState = GameState.Play;
        foreach (Train train in GridSystem.Instance.activeTrains)
        {
            train.MoveToNextNode();
        }
        OnStartPlay?.Invoke();
    }

    public void Stop()
    {
        if(gameState == GameState.Edit)
        {
            return;
        }

        gameState = GameState.Edit;
        for (int i = 0; i < switchInfos.Count; i++)
        {
            if (switchInfos[i].passTimes % 2 == 0)
            {
                continue;
            }

            var temp = new SwitchInfo() { passTimes = 0, switchNode = switchInfos[i].switchNode };
            switchInfos[i].switchNode.Switch();
            switchInfos[i] = temp;
        }

        for (int i = 0; i < activeTrains.Count; i++)
        {
            Node currentNode = activeTrains[i].movedTrain.CurrentNode;
            currentNode.ClearTrain();
        }

        for (int i = 0; i < activeTrains.Count; i++)
        {
            Node startNode = activeTrains[i].startNode;
            Train train = activeTrains[i].movedTrain;
            train.StopAllCoroutines();
            startNode.SetTrain(train);
            train.transform.position = startNode.transform.position;
            train.transform.rotation = activeTrains[i].rotation;
            train.Setup(startNode, train.StationIndex);
        }
    }

    void SwitchNodeState()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentNode)
            {
                currentNode.SwitchState();
            }
        }
    }

    void Update()
    {
        if(gameState == GameState.Edit)
        {
            /*if (Input.GetKeyDown(KeyCode.Space))
            {
                GameObject obj = InputHelper.GetObjectUnderMousePosition(nodeLayer);
                if (obj)
                {
                    Node node = obj.GetComponent<Node>();
                    node.SwitchState();
                }
            }*/

            switch (inputState)
            {
                case InputState.NullSelect:
                    SelectNode();
                    break;
                case InputState.NodeSelect:
                    SelectNode();
                    SpawnNewTrain();
                    SpawnNewStation();
                    SetNodeAsLineController();
                    DeleteTrain();
                    DeleteStation();
                    SwitchNodeState();
                    break;
                case InputState.LineSelect:
                    SelectNode();
                    ChangeLineDirection();
                    DeleteLine();
                    SetCurrentControlLine();
                    break;
                case InputState.DrawLine:
                    DrawLine();
                    break;
                case InputState.NodeSlotSelect:
                    SelectNode();
                    SpawnNode();
                    break;
            }
        }
    }

    void DeleteTrain()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (currentNode.Train != null)
            {
                int index = -1;
                switch (currentNode.Train.TrainNumber)
                {
                    case StationNumber.A:
                        index = 0;
                        break;
                    case StationNumber.B:
                        index = 1;
                        break;
                    case StationNumber.C:
                        index = 2;
                        break;
                    case StationNumber.D:
                        index = 3;
                        break;
                }

                for (int i = 0; i < trainIndices.Length; i++)
                {
                    if(i == index)
                    {
                        trainIndices[i] = -1;
                        break;
                    }
                }

                for (int i = 0; i < activeTrains.Count; i++)
                {
                    if(activeTrains[i].movedTrain == currentNode.Train)
                    {
                        activeTrains.RemoveAt(i);
                        break;
                    }
                }

                Destroy(currentNode.Train.gameObject);
                currentNode.ClearTrain();
            }
        }
    }

    void DeleteStation()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if(currentNode.Station != null)
            {
                int index = -1;
                switch (currentNode.Station.StationNumber)
                {
                    case StationNumber.A:
                        index = 0;
                        break;
                    case StationNumber.B:
                        index = 1;
                        break;
                    case StationNumber.C:
                        index = 2;
                        break;
                    case StationNumber.D:
                        index = 3;
                        break;
                }

                for (int i = 0; i < stationIndices.Length; i++)
                {
                    if (i == index)
                    {
                        stationIndices[i] = -1;
                        break;
                    }
                }

                Destroy(currentNode.Station.gameObject);
                currentNode.ClearStation();
            }
        }
    }

    void SpawnNewTrain()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (currentNode.Train != null)
            {
                return;
            }

            int index = -1;
            for (int i = 0; i < trainIndices.Length; i++)
            {
                if (trainIndices[i] == -1)
                {
                    trainIndices[i] = i;
                    index = i;
                    break;
                }
            }

            if (index == -1) return;
            Train train = Instantiate(pfTrain, currentNode.transform.position, Quaternion.identity).GetComponent<Train>();
            currentNode.SetTrain(train);
            train.Setup(currentNode, index);

            activeTrains.Add(new TrainInfo() { movedTrain = train, startNode = currentNode, rotation = train.transform.rotation });
        }
    }

    void SpawnNewStation()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if(currentNode.Station != null)
            {
                return;
            }

            int index = -1;
            for (int i = 0; i < stationIndices.Length; i++)
            {
                if (stationIndices[i] == -1)
                {
                    stationIndices[i] = i;
                    index = i;
                    break;
                }
            }

            if (index == -1) return;
            Station station = Instantiate(pfStation, currentNode.transform.position, Quaternion.identity).GetComponent<Station>();
            currentNode.SetStation(station);
            station.Setup(currentNode, index);
            stations.Add(station);
        }
    }

    void SetCurrentControlLine()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentControlLine = selectLine;
        }
    }

    void SetNodeAsLineController()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentControlLine != null)
            {
                currentNode.SetAsLineController(currentControlLine);
                currentControlLine.SetControlByNode();
                print($"{currentNode.gameObject.name} : Set As Line Controller");
            }
        }
    }

    void SpawnNode()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Node node = Instantiate(pfNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
            node.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
            currentNodeSlot.SetNode(node);
            currentNodeSlot.gameObject.SetActive(false);

            node.gameObject.name = "Node_" + currentNodeSlot.GlobalIndex.x + "_" + currentNodeSlot.GlobalIndex.y;
        }
    }

    void ChangeLineDirection()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            selectLine.SetLineDirection();
        }
    }

    void DeleteLine()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if(selectLine != null)
            {
                selectLine.DeleteLine();
                selectLine = null;
            }
        }
    }

    void DrawLine()
    {
        if (Input.GetMouseButton(0))
        {
            currentDrawingLine = currentNode.currentLine;
            GameObject wayPointObj = InputHelper.GetObjectUnderMousePosition(wayPointLayer);
            if(wayPointObj != null)
            {
                WayPointNode wayPoint = wayPointObj.GetComponent<WayPointNode>();
                currentWayPoint = wayPoint;
                // if current way point is not last frame stored way point
                if (currentWayPoint != preWayPoint)
                {
                    // one more IF check current already in this way point
                    if (wayPoint.CanReceiveLine(currentDrawingLine))
                    {
                        print("can receive");
                        wayPoint.Connect(currentDrawingLine);
                        currentDrawingLine.ConnectWayPoint(wayPoint);
                    }
                    else
                    {
                        wayPoint.Separate(currentDrawingLine);
                        currentDrawingLine.SeparateWayPoint(wayPoint);
                    }

                    preWayPoint = currentWayPoint;
                    currentWayPoint = null;
                }
            }
            else
            {
                preWayPoint = null;
                currentWayPoint = null;
            }

            Vector2 endPosition = GetLineEndPosition(InputHelper.MouseWorldPositionIn2D);
            currentNode.DrawLine(endPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            GameObject nodeObj = InputHelper.GetObjectUnderMousePosition(nodeLayer);
            if (nodeObj)
            {
                Node node = nodeObj.GetComponent<Node>();
                if (currentNode.IsConnectValid(node))
                {
                    currentNode.FinishDraw(ref node);
                    currentNode = node;
                    currentNode.BeSelect();
                    inputState = InputState.NodeSelect;
                }
                else
                {
                    currentNode.DeleteLine();
                    inputState = InputState.NodeSelect;
                    return;
                }
            }
            else
            {
                currentNode.DeleteLine();
                inputState = InputState.NodeSelect;
            }
        }
    }

    Vector2 GetLineEndPosition(Vector2 mousePos)
    {
        Node closestNode = null;
        Collider2D[] aroundNodes = Physics2D.OverlapCircleAll(mousePos, 6f, nodeLayer);

        float minSqrDst = float.MaxValue;
        foreach(Collider2D collider in aroundNodes)
        {
            //if()
        }

        return mousePos;
    }

    void SelectNode()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject nodeObj = InputHelper.GetObjectUnderMousePosition(nodeLayer);
            GameObject lineObj = InputHelper.GetObjectUnderMousePosition(lineLayer);
            GameObject nodeSlotObj = InputHelper.GetObjectUnderMousePosition(nodeSlotLayer);

            if (nodeSlotObj)
            {
                NodeSlot nodeSlot = nodeSlotObj.GetComponent<NodeSlot>();
                if (currentNodeSlot != null)
                {
                    currentNodeSlot.CancelSelect();
                }
                nodeSlot.BeSelect();
                currentNodeSlot = nodeSlot;
                inputState = InputState.NodeSlotSelect;
                return;
            }
            else
            {
                if (currentNodeSlot != null)
                {
                    currentNodeSlot.CancelSelect();
                }
                currentNodeSlot = null;
                inputState = InputState.NullSelect;
            }

            if (nodeObj)
            {
                Node node = nodeObj.GetComponent<Node>();
                if (currentNode != null)
                {
                    currentNode.CancelSelect();
                }
                node.BeSelect();
                currentNode = node;
                inputState = InputState.NodeSelect;

                if (selectLine)
                {
                    selectLine.CancelSelect();
                }

                return;
            }
            else
            {
                if (currentNode != null)
                {
                    currentNode.CancelSelect();
                }
                currentNode = null;
                inputState = InputState.NullSelect;
            }

            if (lineObj)
            {
                Line line = lineObj.GetComponent<Line>();
                if (selectLine != null)
                {
                    selectLine.CancelSelect();
                }
                line.BeSelect();
                selectLine = line;
                inputState = InputState.LineSelect;
            }
            else
            {
                if (selectLine != null)
                {
                    selectLine.CancelSelect();
                }
                selectLine = null;
                inputState = InputState.NullSelect;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (currentNode != null)
            {
                inputState = InputState.DrawLine;
            }
        }
    }

    public struct SwitchInfo
    {
        public int passTimes;
        public Node switchNode;
    }

    public struct TrainInfo
    {
        public Train movedTrain;
        public Node startNode;
        public Quaternion rotation;
    }
}
