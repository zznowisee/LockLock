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
    public Line DrawingLine { get { return nodeInfo.currentLine; } }

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
        nodeInfo.ToString();
    }

    public void SetAsLineController(Line target)
    {
        lineController.gameObject.SetActive(true);

        lineController.Setup(target);
        isLineController = true;
    }

    public void OnTrainArrivedNode()
    {
        if (isLineController)
        {
            lineController.OnTrainArrive();
        }


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
        if (nodeInfo.currentLine == null)
        {
            nodeInfo.currentLine = Instantiate(pfLine, transform.position, Quaternion.identity, customSystem.lineParent);
        }

        nodeInfo.currentLine.Setup(this, LineState.NormalLine);
        nodeInfo.currentLine.Draw(endPosition);
    }

    public virtual void FinishDraw(ref Node nextNode)
    {
        nodeInfo.currentLine.FinishLine(nextNode);
        Connect(ref nextNode);

        CancelSelect();
        nodeInfo.currentLine = null;
    }

    public virtual void DeleteLine()
    {
        Destroy(nodeInfo.currentLine.gameObject);
        nodeInfo.currentLine = null;
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

    public virtual void Connect(ref Node connectNode)
    {
        LineInfo lineInfo = nodeInfo.currentLine.lineInfo;
        lineInfo.endNode = connectNode;

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
            }
        }

        for (int i = 0; i < deleteNode.lineInfos.Count; i++)
        {
            LineInfo lineInfo = deleteNode.lineInfos[i];
            if (lineInfo.line == line)
            {
                lineInfos.Remove(lineInfo);
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

    public Line GetLineFromConnectingNodesWithNode(Node node)
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

    public Node GetNextNode(Node preNode)
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
            if(lineInfos[i].GetAnotherNode(this) == preNode)
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
    }
}

[Serializable]
public struct NodeInfo
{
    public NodeState nodeState;
    public Vector2Int nodeIndex;
    public Train train;
    public Station station;
    public NodeSlot nodeSlot;
    public Line currentLine;
}
