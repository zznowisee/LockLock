using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NodeType { Normal, Switch, WayPoint }
public class NormalNode : MonoBehaviour
{
    [SerializeField] protected NodeInfo nodeInfo;
    public float radius = 4f;
    public Action OnTrainArrived;

    protected CustomSystem customSystem;
    protected Transform beSelectGraphic;

    public Line pfLine;

    public List<LineInfo> lineInfos;

    LineController lineController;
    bool isLineController = false;

    public NodeInfo NodeInfo { get { return nodeInfo; } }
    public NodeType NodeType { get { return nodeInfo.nodeType; } }
    public Vector2Int GlobalIndex { get { return nodeInfo.nodeIndex; } }
    public Station Station { get { return nodeInfo.station; } }
    public Train Train { get { return nodeInfo.train; } }
    public Line DrawingLine { get { return nodeInfo.drawingLine; } }
    public NodeSlot NodeSlot { get { return nodeInfo.nodeSlot; } }
    public void SetTrain(Train newTrain) => nodeInfo.train = newTrain;
    public void ClearTrain() => nodeInfo.train = null;
    public void SetStation(Station station_) => nodeInfo.station = station_;

    public List<LineInfo> LineInfos { get { return lineInfos; } }
    protected NodeAnimationManager nodeAnimationManager;

    public void ClearStation()
    {
        nodeInfo.station.gameObject.SetActive(false);
        nodeInfo.station = null;
    }

    private void Awake()
    {
        customSystem = FindObjectOfType<CustomSystem>();

        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");
        lineController = transform.Find("lineController").GetComponent<LineController>();

        OnTrainArrived += OnTrainArrivedNode;

        lineController.gameObject.SetActive(false);

        nodeInfo.nodeType = NodeType.Normal;
    }

    public void SetAsLineController(Line target)
    {
        lineController.gameObject.SetActive(true);

        lineController.Setup(target);
        isLineController = true;
    }

    public virtual void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        gameObject.name = "Node_" + nodeIndex.x + "_" + nodeIndex.y;

        nodeInfo.nodeSlot = nodeSlot;
        nodeInfo.nodeIndex = nodeIndex;
        nodeAnimationManager = GetComponent<NodeAnimationManager>();
        lineInfos = new List<LineInfo>();
    }

    public void BeSelect() => beSelectGraphic.gameObject.SetActive(true);
    public void CancelSelect() => beSelectGraphic.gameObject.SetActive(false);

    public virtual void DrawLine()
    {
        if (nodeInfo.drawingLine == null)
        {
            nodeInfo.drawingLine = Instantiate(pfLine, transform.position, Quaternion.identity, customSystem.lineParent);
            nodeInfo.drawingLine.Setup(this, LineState.NormalLine);
        }

        LineInfo lineInfo = nodeInfo.drawingLine.lineInfo;
        Vector2 pos = InputHelper.CalculateEndPosition(InputHelper.MouseWorldPositionIn2D, lineInfo.wayPointNodes.Count == 0 ? this : lineInfo.wayPointNodes[lineInfo.wayPointNodes.Count - 1]);
        nodeAnimationManager.CheckLineEndPosition(pos);
        nodeInfo.drawingLine.Draw(pos);
    }

    public virtual void FinishDraw(ref NormalNode nextNode)
    {
        nodeInfo.drawingLine.FinishLine(nextNode);
        Connect(ref nextNode);

        CancelSelect();
        nodeInfo.drawingLine = null;
    }

    public virtual void DeleteLine()
    {
        for (int i = 0; i < nodeInfo.drawingLine.lineInfo.wayPointNodes.Count; i++)
        {
            WayPointNode wpn = nodeInfo.drawingLine.lineInfo.wayPointNodes[i];
            wpn.UnregisterLine(nodeInfo.drawingLine);
        }

        Destroy(nodeInfo.drawingLine.gameObject);
        nodeInfo.drawingLine = null;
    }

    public bool IsConnectValid(NormalNode endNode)
    {
        if (endNode == this) return false;

        for (int i = 0; i < lineInfos.Count; i++)
        {
            LineInfo lineInfo = lineInfos[i];
            if(lineInfo.GetAnotherNode(this) == endNode)
            {
                return false;
            }
        }

        List<WayPointNode> wayPointNodes = DrawingLine.lineInfo.wayPointNodes;

        int endX = endNode.GlobalIndex.x;
        int beginX = wayPointNodes.Count == 0 ? GlobalIndex.x : wayPointNodes[wayPointNodes.Count - 1].GlobalIndex.x;
        int endY = endNode.GlobalIndex.y;
        int beginY = wayPointNodes.Count == 0 ? GlobalIndex.y : wayPointNodes[wayPointNodes.Count - 1].GlobalIndex.y;

        int offsetX = Mathf.Abs(endX - beginX);
        int offsetY = Mathf.Abs(endY - beginY);

        if (offsetX >= 2 || offsetY >= 2)
        {
            print(wayPointNodes.Count);
            print("BeginX:" + beginX + "_" + "BeginY:" + beginY);
            print("Not Valid");
            return false;
        }
        if (beginX > endX && beginY > endY) return false;
        if (beginX < endX && beginY < endY) return false;

        return true;
    }

    public virtual void Connect(ref NormalNode connectNode)
    {
        LineInfo lineInfo = nodeInfo.drawingLine.lineInfo;
        lineInfo.endNode = connectNode;

        lineInfos.Add(lineInfo);
        connectNode.lineInfos.Add(lineInfo);
    }

    public virtual void Disconnect(ref NormalNode deleteNode, Line line)
    {
        for (int i = 0; i < lineInfos.Count; i++)
        {
            LineInfo lineInfo = lineInfos[i];
            if (lineInfo.line == line)
            {
                lineInfos.Remove(lineInfo);
                break;
            }
        }

        for (int i = 0; i < deleteNode.lineInfos.Count; i++)
        {
            LineInfo lineInfo = deleteNode.lineInfos[i];
            if (lineInfo.line == line)
            {
                deleteNode.lineInfos.Remove(lineInfo);
                break;
            }
        }
    }

    public NormalNode GetNodeFromConnectingNodesWithLine(Line line)
    {
        for (int i = 0; i < lineInfos.Count; i++)
        {
            if (lineInfos[i].line == line)
            {
                return lineInfos[i].GetAnotherNode(this);
            }
        }

        return null;
    }

    public virtual Line GetLineFromConnectingNodesWithNode(NormalNode node)
    {
        for (int i = 0; i < lineInfos.Count; i++)
        {
            if (lineInfos[i].IsTwoNodeMatch(this, node))
            {
                return lineInfos[i].line;
            }
        }
        return null;
    }

    public virtual NormalNode GetNextNode(NormalNode preNode, NormalNode lineEnterNode)
    {
        Line passedLine = preNode.GetLineFromConnectingNodesWithNode(this);
        List<NormalNode> canPassLines = new List<NormalNode>();

        for (int i = 0; i < lineInfos.Count; i++)
        {
            LineInfo lineInfo = lineInfos[i];
            // the previous node
            if (lineInfo.GetAnotherNode(preNode) == this) continue;
            // inverse dir to One way line
            if (lineInfo.line.IsOneWayLine())
                if (!lineInfo.line.CanPassBaseOnOneWay(this)) continue;
            // red line
            if (!lineInfo.canPass) continue;
            // the same line with way points

            if(lineInfo.wayPointNodes.Count > 0)
            {
                if (lineInfo.startNode == this && passedLine != lineInfo.line)
                {
                    canPassLines.Add(lineInfo.wayPointNodes[0]);
                    continue;
                }

                if (lineInfo.endNode == this && passedLine != lineInfo.line)
                {
                    canPassLines.Add(lineInfo.wayPointNodes[lineInfo.wayPointNodes.Count - 1]);
                    continue;
                }
            }
            else
            {
                canPassLines.Add(lineInfo.GetAnotherNode(this));
            }
        }

        if(canPassLines.Count > 1)
        {
            print("More than 2 choices");
            return null;
        }
        else if(canPassLines.Count == 0)
        {
            print("Null choices");
            return null;
        }
        else
        {
            return canPassLines[0];
        }
    }

    public virtual void OnTrainArrivedNode()
    {

    }
}

[Serializable]
public class NodeInfo
{
    public Vector2Int nodeIndex;
    public NodeType nodeType;
    public NodeSlot nodeSlot;
    public Line drawingLine;
    public Station station;
    public Train train;
}
