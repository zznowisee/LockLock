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
    [SerializeField] Transform pfSwitchNode;
    [SerializeField] Transform pfWayPointNode;

    public List<SwitchInfo> switchInfos;
    public List<TrainInfo> activeTrainInfos;
    public List<StationInfo> activeStationInfos;
    public List<NodeSlot> disableNodeSlots;

    bool hasDeletedSth = false;

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
    private Transform trainParent;
    private Transform stationParent;
    [HideInInspector] public Transform lineParent;

    Transform train;
    
    private void Awake()
    {
        nodeParent = transform.Find("nodeParent");
        trainParent = transform.Find("trainParent");
        stationParent = transform.Find("stationParent");
        lineParent = transform.Find("lineParent");

        trainIndices = new int[]
        {
            -1,-1,-1,-1
        };
        stationIndices = new int[]
        {
            -1,-1,-1,-1
        };
    }

    void Start()
    {
        activeTrainInfos = new List<TrainInfo>();
        activeStationInfos = new List<StationInfo>();
        switchInfos = new List<SwitchInfo>();
        disableNodeSlots = new List<NodeSlot>();
    }

    public void AddSwitchInfo(SwitchNode targetNode)
    {
        bool hasNode = false;
        for (int i = 0; i < switchInfos.Count; i++)
        {
            if (switchInfos[i].switchNode == targetNode || switchInfos[i].switchNode.CombineNodesContain(targetNode))
            {
                switchInfos[i].passTimes++;
                hasNode = true;
                break;
            }
        }

        if (!hasNode)
        {
            switchInfos.Add(new SwitchInfo() { passTimes = 1, switchNode = targetNode });
        }
    }

    #region Buttons
    public void Run()
    {
        if(gameState == GameState.Play)
        {
            return;
        }

        gameState = GameState.Play;
        for (int i = 0; i < activeTrainInfos.Count; i++)
        {
            Train train = activeTrainInfos[i].activeTrain;
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

        for (int i = 0; i < activeTrainInfos.Count; i++)
        {
            Node currentNode = activeTrainInfos[i].activeTrain.CurrentNode;
            currentNode.ClearTrain();
        }

        for (int i = 0; i < activeTrainInfos.Count; i++)
        {
            Node startNode = activeTrainInfos[i].startNode;
            Train train = activeTrainInfos[i].activeTrain;

            train.StopAllCoroutines();
            startNode.SetTrain(train);
            train.transform.position = startNode.transform.position;
            train.transform.rotation = activeTrainInfos[i].rotation;
            train.Setup(startNode, train.StationIndex);

            if (!train.gameObject.activeSelf)
            {
                train.gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < activeStationInfos.Count; i++)
        {
            Node startNode = activeStationInfos[i].startNode;
            Station station = activeStationInfos[i].activeStation;
            startNode.SetStation(station);

            if (!station.gameObject.activeSelf)
            {
                station.gameObject.SetActive(true);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < trainParent.childCount; i++)
        {
            Destroy(trainParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < stationParent.childCount; i++)
        {
            Destroy(stationParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < nodeParent.childCount; i++)
        {
            Destroy(nodeParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < disableNodeSlots.Count; i++)
        {
            disableNodeSlots[i].gameObject.SetActive(true);
            disableNodeSlots[i].ClearNode();
        }

        disableNodeSlots.Clear();

        for (int i = 0; i < lineParent.childCount; i++)
        {
            Destroy(lineParent.GetChild(i).gameObject);
        }

        trainIndices = new int[]
        {
            -1,-1,-1,-1
        };
        stationIndices = new int[]
        {
            -1,-1,-1,-1
        };

        activeTrainInfos.Clear();
        activeStationInfos.Clear();

        gameState = GameState.Edit;
    }

    public void Restart()
    {
        foreach(NodeSlot slot in disableNodeSlots)
        {
            Node node = slot.Node;
            for (int i = node.lineInfos.Count - 1; i >= 0; i--)
            {
                Line line = node.lineInfos[i].line;
                line.DeleteLine();
            }

            if(node.NodeState == NodeState.Switch)
            {
                Node normalNode = Instantiate(pfNode, node.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
                slot.SetNode(normalNode);
                normalNode.Setup(slot.GlobalIndex, slot);
                Destroy(node.gameObject);
            }

            if(node.Station != null)
            {
                Destroy(node.Station.gameObject);
                node.ClearStation();
            }

            if (node.Train != null)
            {
                Destroy(node.Train.gameObject);
                node.ClearTrain();
            }
        }

        trainIndices = new int[]
        {
            -1,-1,-1,-1
        };
        stationIndices = new int[]
        {
            -1,-1,-1,-1
        };

        activeTrainInfos.Clear();
        activeStationInfos.Clear();
    }

    #endregion
    void SwitchNodeState()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NodeSlot nodeSlot = currentNode.NodeSlot;

            for (int i = currentNode.lineInfos.Count - 1; i >= 0; i--)
            {
                Line line = currentNode.lineInfos[i].line;
                line.DeleteLine();
            }

            switch (currentNode.NodeState)
            {
                case NodeState.Normal:
                    // setup new node
                    SwitchNode switchNode = Instantiate(pfSwitchNode, currentNode.transform.position, Quaternion.identity, nodeParent).GetComponent<SwitchNode>();
                    nodeSlot.SetNode(switchNode);
                    switchNode.Setup(nodeSlot.GlobalIndex, nodeSlot);
                    switchNode.BeSelect();
                    if (currentNode.Train != null)
                    {
                        Train train = currentNode.Train;
                        switchNode.SetTrain(currentNode.Train);
                        train.Setup(switchNode, train.StationIndex);
                    }
                    if (currentNode.Station != null)
                    {
                        Station station = currentNode.Station;
                        switchNode.SetStation(currentNode.Station);
                        station.Setup(switchNode, station.StationIndex);
                    }

                    // delete old node
                    Destroy(currentNode.gameObject);
                    currentNode = switchNode;
                    break;
                case NodeState.Switch:
                    // setup new node
                    Node normalNode = Instantiate(pfNode, currentNode.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
                    nodeSlot.SetNode(normalNode);
                    normalNode.Setup(nodeSlot.GlobalIndex, nodeSlot);
                    normalNode.BeSelect();
                    if (currentNode.Train != null)
                    {
                        Train train = currentNode.Train;
                        normalNode.SetTrain(currentNode.Train);
                        train.Setup(normalNode, train.StationIndex);
                    }

                    if (currentNode.Station != null)
                    {
                        Station station = currentNode.Station;
                        normalNode.SetStation(currentNode.Station);
                        station.Setup(normalNode, station.StationIndex);
                    }
                    // delete old node
                    Destroy(currentNode.gameObject);
                    currentNode = normalNode;
                    break;
            }
        }
    }

    void Update()
    {
        if(gameState == GameState.Edit)
        {
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
                    DeleteNode();
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

        hasDeletedSth = false;
    }

    void DeleteNode()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hasDeletedSth) return;
            if(currentNode.Train == null && currentNode.Station == null)
            {
                currentNode.NodeSlot.gameObject.SetActive(true);
                currentNode.NodeSlot.ClearNode();

                for (int i = currentNode.lineInfos.Count - 1; i >= 0; i--)
                {
                    Line line = currentNode.lineInfos[i].line;
                    line.DeleteLine();
                }

                Destroy(currentNode.gameObject);
                hasDeletedSth = true;
            }
        }
    }

    void DeleteTrain()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hasDeletedSth) return;
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

                for (int i = 0; i < activeTrainInfos.Count; i++)
                {
                    if(activeTrainInfos[i].activeTrain == currentNode.Train)
                    {
                        activeTrainInfos.RemoveAt(i);
                        break;
                    }
                }

                Destroy(currentNode.Train.gameObject);
                currentNode.ClearTrain();
                hasDeletedSth = true;
            }
        }
    }

    void DeleteStation()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hasDeletedSth) return;
            if (currentNode.Station != null)
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
                hasDeletedSth = true;
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
            Train train = Instantiate(pfTrain, currentNode.transform.position, Quaternion.identity,trainParent).GetComponent<Train>();
            currentNode.SetTrain(train);
            train.Setup(currentNode, index);

            activeTrainInfos.Add(new TrainInfo() { activeTrain = train, startNode = currentNode, rotation = train.transform.rotation });
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
            Station station = Instantiate(pfStation, currentNode.transform.position, Quaternion.identity, stationParent).GetComponent<Station>();
            currentNode.SetStation(station);
            station.Setup(currentNode, index);

            activeStationInfos.Add(new StationInfo() { activeStation = station, startNode = currentNode });
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
            if (currentNodeSlot.IsEmpty)
            {
                Node node = Instantiate(pfNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
                node.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
                currentNodeSlot.SetNode(node);
                currentNodeSlot.gameObject.SetActive(false);
                disableNodeSlots.Add(currentNodeSlot);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentNodeSlot.IsEmpty)
            {
                SwitchNode switchNode = Instantiate(pfSwitchNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<SwitchNode>();
                switchNode.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
                currentNodeSlot.SetNode(switchNode);
                currentNodeSlot.gameObject.SetActive(false);
                disableNodeSlots.Add(currentNodeSlot);
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentNodeSlot.IsEmpty)
            {
                WayPointNode wayPointNode = Instantiate(pfWayPointNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<WayPointNode>();
                wayPointNode.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
                currentNodeSlot.SetNode(wayPointNode);
                currentNodeSlot.gameObject.SetActive(false);
                disableNodeSlots.Add(currentNodeSlot);
            }
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
            //currentDrawingLine = currentNode.DrawingLine;
            GameObject wayPointObj = InputHelper.GetObjectUnderMousePosition(wayPointLayer);
            if(wayPointObj != null)
            {
                WayPointNode wayPoint = wayPointObj.GetComponent<WayPointNode>();
                currentWayPoint = wayPoint;
                // if current way point is not last frame stored way point
                if (currentWayPoint != preWayPoint)
                {
                    // one more IF check current already in this way point
                    if (wayPoint.CanReceiveLine(currentNode.DrawingLine))
                    {
                        print("can receive");
                        wayPoint.RegisterNewLine(currentNode.DrawingLine);
                        currentNode.DrawingLine.ConnectWayPoint(wayPoint);
                    }
                    else
                    {
                        wayPoint.UnregisterLine(currentNode.DrawingLine);
                        currentNode.DrawingLine.SeparateWayPoint(wayPoint);
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

            currentNode.DrawLine();
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

/*    Vector2 GetLineEndPosition(Vector2 mousePos)
    {
        Node closestNode = null;
        Collider2D[] aroundNodes = Physics2D.OverlapCircleAll(mousePos, 6f, nodeLayer);

        float minSqrDst = float.MaxValue;
        foreach(Collider2D collider in aroundNodes)
        {
            //if()
        }

        return mousePos;
    }*/

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

    public class SwitchInfo
    {
        public int passTimes;
        public SwitchNode switchNode;
    }

    public struct TrainInfo
    {
        public Train activeTrain;
        public Node startNode;
        public Quaternion rotation;
    }

    public struct StationInfo
    {
        public Station activeStation;
        public Node startNode;
    }
}
