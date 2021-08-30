using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NodeState { Normal, Switch, WayPoint }
public class Node : MonoBehaviour
{
    [SerializeField] protected NodeInfo nodeInfo;
    public Action OnTrainArrived;

    protected CustomSystem customSystem;
    protected Transform beSelectGraphic;

    public Line pfLine;

    public List<LineInfo> lineInfos;

    LineController lineController;
    bool isLineController = false;

    public NodeState NodeState { get { return nodeInfo.nodeState; } }
    public Vector2Int GlobalIndex { get { return nodeInfo.nodeIndex; } }
    public Station Station { get { return nodeInfo.station; } }
    public Train Train { get { return nodeInfo.train; } }
    public Line DrawingLine { get { return nodeInfo.drawingLine; } }
    public NodeSlot NodeSlot { get { return nodeInfo.nodeSlot; } }
    public void SetTrain(Train newTrain) => nodeInfo.train = newTrain;
    public void ClearTrain() => nodeInfo.train = null;
    public void SetStation(Station station_) => nodeInfo.station = station_;

    public List<LineInfo> LineInfos { get { return lineInfos; } }

    public void ClearStation()
    {
        Destroy(nodeInfo.station.gameObject);
        nodeInfo.station = null;
    }

    private void Awake()
    {
        customSystem = FindObjectOfType<CustomSystem>();

        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");
        lineController = transform.Find("lineController").GetComponent<LineController>();

        OnTrainArrived += OnTrainArrivedNode;

        lineController.gameObject.SetActive(false);

        nodeInfo.nodeState = NodeState.Normal;
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

        lineInfos = new List<LineInfo>();
    }

    public void BeSelect() => beSelectGraphic.gameObject.SetActive(true);
    public void CancelSelect() => beSelectGraphic.gameObject.SetActive(false);

    public virtual void DrawLine(Vector3 endPosition)
    {
        if (nodeInfo.drawingLine == null)
        {
            nodeInfo.drawingLine = Instantiate(pfLine, transform.position, Quaternion.identity, customSystem.lineParent);
            nodeInfo.drawingLine.Setup(this, LineState.NormalLine);
        }

        nodeInfo.drawingLine.Draw(endPosition);
    }

    public virtual void FinishDraw(ref Node nextNode)
    {
        nodeInfo.drawingLine.FinishLine(nextNode);
        Connect(ref nextNode);

        CancelSelect();
        nodeInfo.drawingLine = null;
    }

    public virtual void DeleteLine()
    {
        Destroy(nodeInfo.drawingLine.gameObject);
        nodeInfo.drawingLine = null;
    }

    public bool IsConnectValid(Node endNode)
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

    public virtual void Connect(ref Node connectNode)
    {
        LineInfo lineInfo = nodeInfo.drawingLine.lineInfo;
        lineInfo.endNode = connectNode;

        for (int i = 0; i < lineInfo.wayPointNodes.Count; i++)
        {
            WayPointNode wayPointNode = lineInfo.wayPointNodes[i];
            wayPointNode.lineInfos.Add(lineInfo);
        }

        lineInfos.Add(lineInfo);
        connectNode.lineInfos.Add(lineInfo);
    }

    public virtual void Disconnect(ref Node deleteNode, Line line)
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

    public Node GetNodeFromConnectingNodesWithLine(Line line)
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

    public virtual Line GetLineFromConnectingNodesWithNode(Node node)
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

    public virtual Node GetNextNode(Node preNode, Node lineEnterNode)
    {
        Line passedLine = preNode.GetLineFromConnectingNodesWithNode(this);
        List<Node> canPassLines = new List<Node>();

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
    #region Refac
    /*    public virtual Node GetNextNode(Node preNode, Node beginNode)
        {
            if (lineInfos.Count >= 2)
            {
                int canPassNum = 0;
                for (int i = 0; i < lineInfos.Count; i++)
                {
                    if(lineInfos[i].GetAnotherNode(this) == preNode)
                    {
                        continue;
                    }

                    Line line = lineInfos[i].line;
                    if (line.IsOneWayLine())
                    {
                        if (!line.CanPassBaseOnOneWay(this)) continue;
                    }

                    if (!line.CanPassBaseOnController()) continue;

                    if (lineInfos[i].canPass)
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

            for (int i = 0; i < lineInfos.Count; i++)
            {
                if (lineInfos[i].GetAnotherNode(beginNode) == this) continue;

                if (lineInfos[i].wayPointNodes.Count > 0 && lineInfos[i].wayPointNodes[lineInfos[i].wayPointNodes.Count - 1] != preNode)
                {
                    print("Enter First Way Point");
                    return lineInfos[i].wayPointNodes[0];
                }

                if (lineInfos[i].GetAnotherNode(this) == preNode)
                {
                    continue;
                }

                if (!lineInfos[i].canPass)
                {
                    print("can't pass");
                    continue;
                }

                if (lineInfos[i].line.IsOneWayLine())
                {
                    if (!lineInfos[i].line.CanPassBaseOnOneWay(this)) continue;
                }

                if (!lineInfos[i].line.CanPassBaseOnController())
                {
                    continue;
                }

                return lineInfos[i].GetAnotherNode(this);
            }

            return null;
        }*/
    #endregion

    public virtual void OnTrainArrivedNode()
    {

    }
}

[Serializable]
public struct NodeInfo
{
    public Vector2Int nodeIndex;
    public NodeState nodeState;
    public NodeSlot nodeSlot;
    public Line drawingLine;
    public Station station;
    public Train train;
}
