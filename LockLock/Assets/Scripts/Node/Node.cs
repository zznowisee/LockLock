using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NodeType { Normal, Switch, WayPoint, Receiver }
public class Node : MonoBehaviour
{
    [HideInInspector] public NodeInfo nodeInfo;

    public NodeType NodeType { get { return nodeInfo.nodeType; } }
    public Vector2Int GlobalIndex { get { return nodeInfo.nodeIndex; } }
    public Line DrawingLine { get { return nodeInfo.drawingLine; } }
    public NodeSlot NodeSlot { get { return nodeInfo.nodeSlot; } }

    protected CustomSystem customSystem;
    protected Transform beSelectGraphic;

    public float radius = 4f;
    [SerializeField] protected Line pfLine;
    [SerializeField] protected Electron pfElectron;
    public List<LineInfo> lineInfos;
    public List<Electron> waitingElectrons;

    Transform electronParent;

    protected NodeAnimationManager nodeAnimationManager;

    private void Awake()
    {
        nodeInfo.nodeType = NodeType.Normal;
    }

    public virtual void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        gameObject.name = "Node_" + nodeIndex.x + "_" + nodeIndex.y;
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");
        customSystem = FindObjectOfType<CustomSystem>();
        electronParent = customSystem.transform.Find("electronParent");
        nodeInfo.nodeSlot = nodeSlot;
        nodeInfo.nodeIndex = nodeIndex;
        nodeAnimationManager = GetComponent<NodeAnimationManager>();
        lineInfos = new List<LineInfo>();
    }
    public void BeSelect() => beSelectGraphic.gameObject.SetActive(true);
    public void CancelSelecting() => beSelectGraphic.gameObject.SetActive(false);

    #region Draw Line
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

    public virtual void FinishDraw(Node nextNode)
    {
        nodeInfo.drawingLine.FinishLine(nextNode);
        Connect(nextNode);

        CancelSelecting();
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

    public bool IsConnectValid(Node endNode)
    {
        if (endNode == this) return false;

        for (int i = 0; i < lineInfos.Count; i++)
        {
            LineInfo lineInfo = lineInfos[i];
            if (lineInfo.GetAnotherNode(this) == endNode)
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

    public virtual void Connect(Node connectNode)
    {
        LineInfo lineInfo = nodeInfo.drawingLine.lineInfo;
        lineInfo.endNode = connectNode;

        lineInfos.Add(lineInfo);
        connectNode.lineInfos.Add(lineInfo);
    }

    public virtual void Disconnect(Node deleteNode, Line line)
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

    #endregion

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

    Line GetLineFromConnectingNodesWithNode(Node node)
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

    public virtual List<Node> GetTargetsBaseOnPreNodes(List<Node> preNodes = null)
    {
        List<Node> targets = new List<Node>();
        Dictionary<LineInfo, Node> lineInfoNodeDictionary = new Dictionary<LineInfo, Node>();
        for (int i = 0; i < lineInfos.Count; i++)
        {
            lineInfoNodeDictionary[lineInfos[i]] = lineInfos[i].GetAnotherNode(this);
        }

        foreach (LineInfo l in lineInfoNodeDictionary.Keys)
        {
            Node node = lineInfoNodeDictionary[l];
            if (preNodes.Contains(node))
            {
                continue;
            }

            if (!l.canPass) continue;
            if (l.line.IsOneWayLine())
            {
                if (!l.line.CanPassBaseOnOneWay(this))
                {
                    continue;
                }
            }

            targets.Add(node);
        }
        
        return targets;
    }

    public virtual void RegisterNewElectron(Electron e)
    {
        AddElectron(e);
    }

    public void AddElectron(Electron electron) => waitingElectrons.Add(electron);

    void Split(List<Node> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Electron electron = Instantiate(pfElectron, transform.position, Quaternion.identity, electronParent);

            Line line = GetLineFromConnectingNodesWithNode(targets[i]);
            line.passingElectrons.Add(electron);
            electron.SetupInfo(this, 0, line);

            waitingElectrons.Add(electron);

            ReactionManager.Instance.RegisterElectron();
        }
    }

    List<Node> GetPreNodesAndCombine()
    {
        List<Node> preNodes = new List<Node>();
        for (int i = 0; i < waitingElectrons.Count; i++)
        {
            preNodes.Add(waitingElectrons[i].startNode);
            if (waitingElectrons[i].passingLine != null)
            {
                waitingElectrons[i].passingLine.passingElectrons.Clear();
            }

            Destroy(waitingElectrons[i].gameObject);
        }
        waitingElectrons.Clear();
        return preNodes;
    }

    public virtual void Reaction()
    {
        if (waitingElectrons.Count != 0)
        {
            List<Node> preNodes = GetPreNodesAndCombine();
            List<Node> targets = GetTargetsBaseOnPreNodes(preNodes);
            Split(targets);
        }
    }

    public virtual void OnEnable() => ReactionManager.Instance.activeNodes.Add(this);
    public virtual void OnDisable() => ReactionManager.Instance.activeNodes.Remove(this);
}

[Serializable]
public class NodeInfo
{
    public Vector2Int nodeIndex;
    public NodeType nodeType;
    public NodeSlot nodeSlot;
    public Line drawingLine;
}