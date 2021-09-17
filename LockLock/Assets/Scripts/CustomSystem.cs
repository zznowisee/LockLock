using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomSystem : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField] LayerMask nodeLayer;
    [SerializeField] LayerMask lineLayer;
    [SerializeField] LayerMask nodeSlotLayer;
    [SerializeField] LayerMask wayPointLayer;
    [Header("Prefab")]
    [SerializeField] Transform pfNode;
    [SerializeField] Transform pfElectron;
    [SerializeField] Transform pfSwitchNode;
    [SerializeField] Transform pfWayPointNode;

    public List<SwitchInfo> switchInfos;
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

    public GameState gameState = GameState.Edit;
    public InputState inputState = InputState.NullSelect;

    NodeSlot currentNodeSlot = null;
    Node currentNode = null;
    Line selectLine = null;

    WayPointNode currentWayPoint = null;
    WayPointNode preWayPoint = null;

    private Transform nodeParent;
    private Transform electronParent;

    public static List<Electron> activeElectrons;

    [HideInInspector] public Transform lineParent;
    
    private void Awake()
    {
        nodeParent = transform.Find("nodeParent");
        electronParent = transform.Find("electronParent");
        lineParent = transform.Find("lineParent");
    }

    void Start()
    {
        switchInfos = new List<SwitchInfo>();
        disableNodeSlots = new List<NodeSlot>();
        activeElectrons = new List<Electron>();
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
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void StartGame()
    {
        if(gameState == GameState.Play)
        {
            return;
        }

        gameState = GameState.Play;
        ReactionManager.Instance.RunMachine();
    }

    public void StopGame()
    {
        if(gameState == GameState.Edit)
        {
            return;
        }
        ReactionManager.Instance.Stop();

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
    }

    public void Clear()
    {
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

        for (int i = 0; i < activeElectrons.Count; i++)
        {
            Destroy(activeElectrons[i].gameObject);
        }

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

            if(node.NodeType == NodeType.Switch)
            {
                Node normalNode = Instantiate(pfNode, node.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
                slot.SetNode(normalNode);
                normalNode.Setup(slot.GlobalIndex, slot);
                Destroy(node.gameObject);
            }
        }

        gameState = GameState.Edit;
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

            switch (currentNode.NodeType)
            {
                case NodeType.Normal:
                    // setup new node
                    SwitchNode switchNode = Instantiate(pfSwitchNode, currentNode.transform.position, Quaternion.identity, nodeParent).GetComponent<SwitchNode>();
                    nodeSlot.SetNode(switchNode);
                    switchNode.Setup(nodeSlot.GlobalIndex, nodeSlot);
                    switchNode.BeSelect();
                    // delete old node
                    Destroy(currentNode.gameObject);
                    currentNode = switchNode;
                    break;
                case NodeType.Switch:
                    // setup new node
                    Node normalNode = Instantiate(pfNode, currentNode.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
                    nodeSlot.SetNode(normalNode);
                    normalNode.Setup(nodeSlot.GlobalIndex, nodeSlot);
                    normalNode.BeSelect();
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
                    SpawnNewElectron();
                    DeleteNode();
                    SwitchNodeState();
                    break;
                case InputState.LineSelect:
                    SelectNode();
                    ChangeLineDirection();
                    DeleteLine();
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
    void SpawnNewElectron()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Electron electron = Instantiate(pfElectron, currentNode.transform.position, Quaternion.identity, electronParent).GetComponent<Electron>();
            currentNode.AddElectron(electron);
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
            currentNode.DrawLine();

            Vector2 position = currentNode.DrawingLine.GetComponent<LineVisual>().LastPointPosition;
            GameObject wayPointObj = InputHelper.GetObjectUnderPosition(wayPointLayer, position);
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
                        print("Separate WayPoint");
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
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 position = currentNode.DrawingLine.GetComponent<LineVisual>().LastPointPosition;
            GameObject nodeObj = InputHelper.GetObjectUnderPosition(nodeLayer, position);
            if (nodeObj)
            {
                Node node = nodeObj.GetComponent<Node>();
                if (currentNode.IsConnectValid(node))
                {
                    currentNode.FinishDraw(node);
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
                    currentNodeSlot.CancelSelecting();
                }
                nodeSlot.BeSelecting();
                currentNodeSlot = nodeSlot;
                inputState = InputState.NodeSlotSelect;
                return;
            }
            else
            {
                if (currentNodeSlot != null)
                {
                    currentNodeSlot.CancelSelecting();
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
}
