using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : MonoBehaviour
{
    public Action OnTrainArrived;

    public enum NodeState { Node, Switch }
    [SerializeField] NodeState nodeState = NodeState.Node;

    Transform lineParent;
    Transform beSelectGraphic;
    Transform switchGraphic;
    Transform switchWarning;

    LineController lineController;

    Vector2Int globalIndex;

    [SerializeField] Line pfLine;

    [HideInInspector] public Line currentLine;
    public Line waitingLine = null;
    public Line usingLine = null;

    [HideInInspector] public bool hasDrawSwitchLine = false;

    Train stayTrain;
    Station station;
    NodeSlot nodeSlot;

    public List<Node> combineNodes;
    public List<Line> combineLines;
    public List<Line2Node> connectingNodes;

    bool isLineController = false;

    public Vector2Int GlobalIndex { get { return globalIndex; } }
    public Station Station { get { return station; } }
    public Train Train { get { return stayTrain; } }

    public void SetTrain(Train newTrain) => stayTrain = newTrain;
    public void ClearTrain() => stayTrain = null;
    public void SetStation(Station station_) => station = station_;
    public void ClearStation()
    {
        Destroy(station.gameObject);
        station = null;
    }

    private void Awake()
    {
        connectingNodes = new List<Line2Node>();
        combineNodes = new List<Node>();
        combineLines = new List<Line>();

        switchWarning = transform.Find("sprite").Find("switchWarning");
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");
        switchGraphic = transform.Find("sprite").Find("switchGraphic");
        lineParent = transform.Find("lineParent");
        lineController = transform.Find("lineController").GetComponent<LineController>();

        OnTrainArrived += OnTrainArrivedNode;

        lineController.gameObject.SetActive(false);

        CheckSwitchWarning();
    }
    
    public void SetAsLineController(Line target)
    {
        lineController.gameObject.SetActive(true);

        lineController.Setup(target);
        isLineController = true;
    }

    void CheckSwitchWarning()
    {
        if(nodeState == NodeState.Switch)
        {
            switchWarning.gameObject.SetActive(waitingLine == null || usingLine == null);
        }
        else
        {
            switchWarning.gameObject.SetActive(false);
        }
    }

    public void OnTrainArrivedNode()
    {
        if (isLineController)
        {
            lineController.OnTrainArrive();
        }


    }

    public void CombineWith(ref Node target)
    {
        combineNodes.Add(target);
        target.combineNodes.Add(this);

        combineLines.Add(currentLine);
        target.combineLines.Add(currentLine);
        currentLine.SetSwitchConnectingLine();

        switch (currentLine.GetLineState())
        {
            case LineState.Using:
                usingLine = currentLine;
                target.usingLine = currentLine;
                break;
            case LineState.Waiting:
                waitingLine = currentLine;
                target.waitingLine = currentLine;
                break;
        }

        LineRenderer markLineRenderer = currentLine.SwitchVisualMark.GetComponent<LineRenderer>();
        markLineRenderer.SetPosition(1, target.transform.position);
    }

    public void SwitchState()
    {
        switch (nodeState)
        {
            case NodeState.Node:
                nodeState = NodeState.Switch;
                switchGraphic.gameObject.SetActive(true);
                break;
            case NodeState.Switch:
                nodeState = NodeState.Node;
                switchGraphic.gameObject.SetActive(false);
                if (usingLine != null)
                {
                    usingLine.DeleteLine();
                }
                if (waitingLine != null)
                {
                    waitingLine.DeleteLine();
                }
                break;
        }

        CheckSwitchWarning();
    }

    public void Setup(Vector2Int globalIndex_, NodeSlot nodeSlot_)
    {
        gameObject.name = "Node_" + globalIndex.x + "_" + globalIndex.y;
        globalIndex = globalIndex_;
        nodeSlot = nodeSlot_;
    }

    public void BeSelect() => beSelectGraphic.gameObject.SetActive(true);
    public void CancelSelect() => beSelectGraphic.gameObject.SetActive(false);

    public void DrawLine(Vector3 endPosition)
    {
        if (currentLine == null)
        {
            currentLine = Instantiate(pfLine, transform.position, Quaternion.identity);
        }
        if (!currentLine.hasBeenSetup)
        {
            switch (nodeState)
            {
                case NodeState.Node:
                    currentLine.Setup(transform.position, this, LineState.Normal);
                    break;
                case NodeState.Switch:
                    //
                    if (!hasDrawSwitchLine)
                    {
                        if (waitingLine == null)
                        {
                            currentLine.Setup(transform.position, this, LineState.Waiting);
                        }
                        if (usingLine == null)
                        {
                            currentLine.Setup(transform.position, this, LineState.Using);
                        }
                    }
                    else
                    {
                        currentLine.Setup(transform.position, this, LineState.Normal);
                    }
                    //
                    break;
            }
        }

        currentLine.Draw(endPosition);
    }

    public void FinishDraw(ref Node nextNode)
    {
        currentLine.FinishLine(nextNode);
        Connect(ref nextNode);

        if (waitingLine != null && usingLine != null)
        {
            hasDrawSwitchLine = true;
        }

        CancelSelect();
        currentLine = null;

        CheckSwitchWarning();
    }

    public void ResetLine()
    {
        Destroy(currentLine.gameObject);
        currentLine = null;

        CheckSwitchWarning();
    }

    public bool IsConnectValid(Node endNode)
    {
        if (endNode == this) return false;
        for (int i = 0; i < connectingNodes.Count; i++)
        {
            Line2Node line2Node = connectingNodes[i];
            if(line2Node.node == endNode)
            {
                return false;
            }
        }

        int endX = endNode.GlobalIndex.x;
        int beginX = GlobalIndex.x;
        int endY = endNode.GlobalIndex.y;
        int beginY = GlobalIndex.y;

        int offsetX = Mathf.Abs(endX - beginX);
        int offsetY = Mathf.Abs(endY - beginY);

        if (offsetX >= 2 || offsetY >= 2) return false;
        if (beginX > endX && beginY > endY) return false;
        if (beginX < endX && beginY < endY) return false;

        return true;
    }

    public void Connect(ref Node connectNode)
    {
        bool canPass = currentLine.GetLineState() == LineState.Waiting ? false : true;
        bool isSwitchLine = false;
        bool isUsingLine = false;

        switch (currentLine.GetLineState())
        {
            case LineState.Using:
                isSwitchLine = true;
                usingLine = currentLine;
                isUsingLine = true;
                break;
            case LineState.Waiting:
                isSwitchLine = true;
                waitingLine = currentLine;
                break;
        }

        Line2Node startToEnd = new Line2Node(currentLine, connectNode, canPass);
        Line2Node endToStart = new Line2Node(currentLine, this, canPass);

        connectingNodes.Add(startToEnd);
        connectNode.connectingNodes.Add(endToStart);

        //if connecting two nodes are both switch node
        if(connectNode.nodeState == NodeState.Switch && nodeState == NodeState.Switch && isSwitchLine)
        {
            if (isUsingLine)
            {
                if(connectNode.usingLine == null)
                {
                    CombineWith(ref connectNode);
                }
            }
            else
            {
                if(connectNode.waitingLine == null)
                {
                    CombineWith(ref connectNode);
                }
            }
        }

        connectNode.CheckSwitchWarning();
        CheckSwitchWarning();
    }

    public void Disconnect(ref Node deleteNode, Line line)
    {
        switch (line.GetLineState())
        {
            case LineState.Waiting:
                waitingLine = null;
                break;
            case LineState.Using:
                usingLine = null;
                break;
        }

        hasDrawSwitchLine = false;
        if (combineNodes.Contains(deleteNode))
        {
            combineNodes.Remove(deleteNode);
            deleteNode.combineNodes.Remove(this);
            if (line.GetLineState() == LineState.Using)
            {
                deleteNode.usingLine = null;
            }
            else if(line.GetLineState() == LineState.Waiting)
            {
                deleteNode.waitingLine = null;
            }

            deleteNode.hasDrawSwitchLine = false;
            deleteNode.CheckSwitchWarning();
        }

        if (combineLines.Contains(line))
        {
            combineLines.Remove(line);
            deleteNode.combineLines.Remove(line);
        }

        for (int i = 0; i < connectingNodes.Count; i++)
        {
            Line2Node line2Node = connectingNodes[i];
            if(line2Node.node == deleteNode)
            {
                connectingNodes.Remove(line2Node);
                break;
            }
        }

        for (int i = 0; i < deleteNode.connectingNodes.Count; i++)
        {
            Line2Node line2Node = deleteNode.connectingNodes[i];
            if (line2Node.node == this)
            {
                deleteNode.connectingNodes.Remove(line2Node);
                break;
            }
        }

        CheckSwitchWarning();
    }

    public void Switch()
    {
        List<Line> changedLines = new List<Line>();
        List<Node> changedNodes = new List<Node>();

        SwitchLines();
        changedLines.Add(usingLine);
        changedLines.Add(waitingLine);
        changedNodes.Add(this);

        for (int i = 0; i < combineNodes.Count; i++)
        {
            combineNodes[i].SwitchByCombineNode(ref changedLines, ref changedNodes);
        }
    }

    public void SwitchByCombineNode(ref List<Line> changedLines,ref List<Node> changedNodes)
    {
        if (changedNodes.Contains(this))
        {
            return;
        }

        if (!changedLines.Contains(usingLine))
        {
            usingLine.Switch();
            changedLines.Add(usingLine);
            Node usingEndNode = GetNodeFromConnectingNodesWithLine(usingLine);
            SwitchLine2Node(usingEndNode);
        }
        if (!changedLines.Contains(waitingLine))
        {
            waitingLine.Switch();
            changedLines.Add(waitingLine);
            Node waitingEndNode = GetNodeFromConnectingNodesWithLine(waitingLine);
            SwitchLine2Node(waitingEndNode);
        }

        var temp = usingLine;
        usingLine = waitingLine;
        waitingLine = temp;

        changedNodes.Add(this);

        for (int i = 0; i < combineNodes.Count; i++)
        {
            combineNodes[i].SwitchByCombineNode(ref changedLines, ref changedNodes);
        }
    }

    public void SwitchLines()
    {
        usingLine.Switch();
        waitingLine.Switch();

        Node usingEndNode = GetNodeFromConnectingNodesWithLine(usingLine);
        SwitchLine2Node(usingEndNode);

        Node waitingEndNode = GetNodeFromConnectingNodesWithLine(waitingLine);
        SwitchLine2Node(waitingEndNode);

        var temp = usingLine;
        usingLine = waitingLine;
        waitingLine = temp;
    }

    void SwitchLine2Node(Node node)
    {
        for (int i = 0; i < connectingNodes.Count; i++)
        {
            Line2Node line2Node = connectingNodes[i];
            if (line2Node.node == node)
            {
                line2Node.canPass = !line2Node.canPass;
                break;
            }
        }

        for (int i = 0; i < node.connectingNodes.Count; i++)
        {
            Line2Node line2Node = node.connectingNodes[i];
            if (line2Node.node == this)
            {
                line2Node.canPass = !line2Node.canPass;
                break;
            }
        }
    }

    public Line GetLineFromConnectingNodesWithNode(Node node)
    {
        for (int i = 0; i < connectingNodes.Count; i++)
        {
            if(connectingNodes[i].node == node)
            {
                return connectingNodes[i].line;
            }
        }
        return null;
    }

    public Node GetNodeFromConnectingNodesWithLine(Line line)
    {
        for (int i = 0; i < connectingNodes.Count; i++)
        {
            if(connectingNodes[i].line == line)
            {
                return connectingNodes[i].node;
            }
        }

        return null;
    }

    public Node GetNextNode(Node preNode)
    {
        if (connectingNodes.Count > 2)
        {
            int canPassNum = 0;
            for (int i = 0; i < connectingNodes.Count; i++)
            {
                if(connectingNodes[i].node == preNode)
                {
                    continue;
                }

                Line line = connectingNodes[i].line;
                if (line.IsOneWayLine())
                {
                    if (!line.CanPassBaseOnOneWay(this)) continue;
                }

                if (!line.CanPassBaseOnController()) continue;

                if (connectingNodes[i].canPass)
                {
                    canPassNum++;
                }
            }

            if (canPassNum >= 2)
            {
                print("More than 2 choices, Can't move");
                return null;
            }
        }

        for (int i = 0; i < connectingNodes.Count; i++)
        {
            if(connectingNodes[i].node == preNode)
            {
                continue;
            }
            if (!connectingNodes[i].canPass)
            {
                print("can't pass");
                continue;
            }
            if (connectingNodes[i].line.IsOneWayLine())
            {
                if (!connectingNodes[i].line.CanPassBaseOnOneWay(this)) continue;
            }
            if (!connectingNodes[i].line.CanPassBaseOnController())
            {
                continue;
            }
            return connectingNodes[i].node;
        }
        return null;
    }

    [System.Serializable]
    public class Line2Node
    {
        public Line line;
        public Node node;
        public bool canPass;
        public Line2Node(Line line_, Node node_, bool canPass_)
        {
            line = line_;
            node = node_;
            canPass = canPass_;
        }
    }
}
