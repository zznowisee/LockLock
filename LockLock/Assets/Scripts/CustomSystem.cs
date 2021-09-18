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
    Line currentLine = null;

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

    public void CheckAfterOnceReaction()
    {
        if(activeElectrons.Count == 0)
        {
            gameState = GameState.Edit;
        }
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

    void Update()
    {
        if(gameState == GameState.Edit)
        {
            GetSelect();
            switch (inputState)
            {
                case InputState.NodeSelect:
                    StartDrawingLine();
                    SpawnNewElectron();
                    DeleteElectron();
                    DeleteNode();
                    SwitchNodeState();
                    break;
                case InputState.LineSelect:
                    ChangeLineDirection();
                    DeleteLine();
                    break;
                case InputState.DrawLine:
                    DrawLine();
                    break;
                case InputState.NodeSlotSelect:
                    SpawnNode();
                    break;
            }
        }

        hasDeletedSth = false;
    }

    #region Buttons

    public void StartBtn()
    {
        if (gameState == GameState.Play)
        {
            return;
        }

        if (activeElectrons.Count == 0)
        {
            return;
        }

        gameState = GameState.Play;

        ReactionManager.Instance.RunMachine();
    }

    public void StopBtn()
    {
        if (gameState == GameState.Edit)
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

            switchInfos[i].switchNode.Switch();
            switchInfos[i].passTimes = 0;
        }
    }

    public void ClearBtn()
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

        ReactionManager.Instance.ResetReaction();
        gameState = GameState.Edit;
    }

    public void ResetBtn()
    {
        foreach (NodeSlot slot in disableNodeSlots)
        {
            Node node = slot.Node;
            for (int i = node.lineInfos.Count - 1; i >= 0; i--)
            {
                Line line = node.lineInfos[i].line;
                line.DeleteLine();
            }
        }

        for (int i = 0; i < activeElectrons.Count; i++)
        {
            Destroy(activeElectrons[i].gameObject);
        }

        ReactionManager.Instance.ResetReaction();

        gameState = GameState.Edit;
    }

    #endregion

    #region Node Selecting
    void StartDrawingLine()
    {
        if (Input.GetMouseButton(0))
        {
            if (currentNode.NodeType == NodeType.WayPoint) return;
            inputState = InputState.DrawLine;
        }
    }

    void DeleteElectron()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if(currentNode.electrons.Count == 0)
            {
                return;
            }

            for (int i = 0; i < currentNode.electrons.Count; i++)
            {
                Destroy(currentNode.electrons[i].gameObject);
            }

            currentNode.electrons.Clear();
            hasDeletedSth = true;
        }
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

    #endregion

    #region NodeSlot Selecting
    void SpawnNode()
    {
        if (currentNodeSlot.IsEmpty)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                Node node = Instantiate(pfNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<Node>();
                node.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
                currentNodeSlot.SetNode(node);
                currentNodeSlot.gameObject.SetActive(false);
                disableNodeSlots.Add(currentNodeSlot);
                inputState = InputState.NullSelect;
                currentNodeSlot.CancelSelecting();
                currentNodeSlot = null;
                return;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                SwitchNode switchNode = Instantiate(pfSwitchNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<SwitchNode>();
                switchNode.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
                currentNodeSlot.SetNode(switchNode);
                currentNodeSlot.gameObject.SetActive(false);
                disableNodeSlots.Add(currentNodeSlot);
                inputState = InputState.NullSelect;
                currentNodeSlot.CancelSelecting();
                currentNodeSlot = null;
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                WayPointNode wayPointNode = Instantiate(pfWayPointNode, currentNodeSlot.transform.position, Quaternion.identity, nodeParent).GetComponent<WayPointNode>();
                wayPointNode.Setup(currentNodeSlot.GlobalIndex, currentNodeSlot);
                currentNodeSlot.SetNode(wayPointNode);
                currentNodeSlot.gameObject.SetActive(false);
                disableNodeSlots.Add(currentNodeSlot);
                inputState = InputState.NullSelect;
                currentNodeSlot.CancelSelecting();
                currentNodeSlot = null;
                return;
            }
        }
    }
    #endregion

    #region Line Selecting
    void ChangeLineDirection()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            currentLine.SetLineDirection();
        }
    }

    void DeleteLine()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if(currentLine != null)
            {
                currentLine.DeleteLine();
                currentLine = null;
            }
        }
    }
    #endregion

    #region Drawing Line
    void DrawLine()
    {
        if (Input.GetMouseButton(0))
        {
            currentNode.DrawLine();

            Vector2 position = currentNode.DrawingLine.GetComponent<LineVisual>().LastPointPosition;
            GameObject nodeObj = InputHelper.GetObjectUnderPosition(nodeLayer, position);
            if(nodeObj != null)
            {
                if(nodeObj.GetComponent<Node>().NodeType == NodeType.WayPoint)
                {
                    WayPointNode wayPoint = nodeObj.GetComponent<WayPointNode>();
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
                if (currentNode.IsConnectValid(node) && node.NodeType != NodeType.WayPoint)
                {
                    currentNode.FinishDraw(node);

                    CancelAllSelect();

                    inputState = InputState.NullSelect;
                    return;
                }
                currentNode.DeleteLine();
                inputState = InputState.NodeSelect;
            }
            else
            {
                currentNode.DeleteLine();
                inputState = InputState.NodeSelect;
            }
        }
    }
    #endregion

    #region Selecting
    void GetSelect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject nodeObj = InputHelper.GetObjectUnderMousePosition(nodeLayer);
            GameObject lineObj = InputHelper.GetObjectUnderMousePosition(lineLayer);
            GameObject nodeSlotObj = InputHelper.GetObjectUnderMousePosition(nodeSlotLayer);

            if (nodeSlotObj)
            {
                CancelAllSelect();

                NodeSlot nodeSlot = nodeSlotObj.GetComponent<NodeSlot>();
                nodeSlot.BeSelecting();
                currentNodeSlot = nodeSlot;
                inputState = InputState.NodeSlotSelect;
                return;
            }

            if (nodeObj)
            {
                CancelAllSelect();
                Node node = nodeObj.GetComponent<Node>();
                node.BeSelect();
                currentNode = node;
                inputState = InputState.NodeSelect;
                return;
            }

            if (lineObj)
            {
                CancelAllSelect();
                Line line = lineObj.GetComponent<Line>();
                line.BeSelect();
                currentLine = line;
                inputState = InputState.LineSelect;
                return;
            }

            CancelAllSelect();
            inputState = InputState.NullSelect;
        }
    }

    void CancelAllSelect()
    {
        if (currentNode)
        {
            currentNode.CancelSelecting();
        }

        if (currentNodeSlot)
        {
            currentNodeSlot.CancelSelecting();
        }

        if (currentLine)
        {
            currentLine.CancelSelecting();
        }

        currentNodeSlot = null;
        currentNode = null;
        currentLine = null;
    }
    #endregion
    public class SwitchInfo
    {
        public int passTimes;
        public SwitchNode switchNode;
    }
}
