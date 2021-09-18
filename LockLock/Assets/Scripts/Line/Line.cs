using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum LineState { SwitchLine, NormalLine }
public class Line : MonoBehaviour
{
    LineState lineState;
    public LineInfo lineInfo;
    
    public LinePalette palette;

    public bool hasBeenSetup = false;

    bool isControlByNode = false;
    bool isActive = true;

    bool addWayPoint = false;

    // Direction base
    protected Node dirStartNode;
    protected Node dirEndNode;

    protected bool isOnewayLine = false;
    bool hasChangeDirection = false;

    protected Transform oneWayMark;

    protected LineVisual lineVisual;

    public List<Electron> passingElectrons;

    protected float lineWidth = 0.5f;
    protected float cellSize = 6f;

    protected string Name
    {
        get
        {
            if(lineInfo.startNode != null && lineInfo.endNode != null)
            {
                return $"{lineInfo.startNode.GlobalIndex}--{lineInfo.endNode.GlobalIndex}_{lineState}";
            }

            return "Line_" + lineState;
        }
    }

    private void Awake()
    {
        lineVisual = GetComponent<LineVisual>();
        passingElectrons = new List<Electron>();
    }

    public bool IsOneWayLine() => isOnewayLine;

    public bool CanPassBaseOnOneWay(Node currentStartNode)
    {
        return currentStartNode == dirStartNode;
    }

    public bool CanPassBaseOnController() => isControlByNode ? isActive : true;
    public void SetControlByNode()
    {
        isControlByNode = true;
    }

    public void SetActiveBaseOnController()
    {
        isActive = !isActive;


    }

    public void Setup(Node startNode, LineState lineState)
    {
        hasBeenSetup = true;

        lineVisual = GetComponent<LineVisual>();
        lineVisual.Setup(Vector2.zero, lineWidth, cellSize);

        dirStartNode = startNode;
        this.lineState = lineState;
        
        oneWayMark = transform.Find("oneWayMark");
        lineVisual.Material.color = palette.defaultCol;

        lineInfo = new LineInfo(this, startNode, true, LineState.NormalLine);
    }

    public void SetLineDirection()
    {
        if (!isOnewayLine)
        {
            isOnewayLine = true;

            Vector2 dir = (dirEndNode.transform.position - dirStartNode.transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            oneWayMark.gameObject.SetActive(true);
            oneWayMark.transform.position = lineInfo.CenterPos;
            //oneWayMark = Instantiate(pfOneWayMark, lineCenterPos, Quaternion.Euler(0, 0, angle),transform);
            oneWayMark.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            if (!hasChangeDirection)
            {
                hasChangeDirection = true;

                // switch start node and end node
                var temp = dirStartNode;
                dirStartNode = dirEndNode;
                dirEndNode = temp;

                Vector2 dir = (dirEndNode.transform.position - dirStartNode.transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                oneWayMark.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                hasChangeDirection = false;
                isOnewayLine = false;

                oneWayMark.gameObject.SetActive(false);
            }
        }
    }

    public LineState GetLineState() => lineState;

    public void Draw(Vector3 mousePosition)
    {
        if (!addWayPoint)
        {
            lineVisual.UpdateMesh(mousePosition - transform.position);
        }

        addWayPoint = false;
    }

    public void BeSelect()
    {
        lineVisual.Material.color = palette.beSelectCol;
    }

    public virtual void CancelSelecting()
    {
        lineVisual.Material.color = palette.defaultCol;
    }

    public virtual void FinishLine(Node endNode)
    {
        lineVisual.ConnectNode(endNode.transform.position - transform.position);

        dirEndNode = endNode;

        lineInfo.endNode = endNode;

        gameObject.name = Name;
        print($"Setup New NormalLine StartNode : '{ lineInfo.startNode.gameObject.name }' " +
              $"EndNode : '{ lineInfo.endNode.gameObject.name }'");
    }

    public void DeleteLine()
    {
        for (int i = 0; i < lineInfo.wayPointNodes.Count; i++)
        {
            WayPointNode wayPointNode = lineInfo.wayPointNodes[i];
            wayPointNode.UnregisterLine(this);
            wayPointNode.lineInfos.Remove(lineInfo);
        }
        lineInfo.startNode.Disconnect(lineInfo.endNode, this);
        Destroy(gameObject);
    }

    public void ConnectWayPoint(WayPointNode wayPoint)
    {
        lineInfo.AddWayPoint(wayPoint);
        lineVisual.ConnectWayPoint(wayPoint.transform.position - transform.position);
        addWayPoint = true;
    }

    public void SeparateWayPoint(WayPointNode wayPoint)
    {
        lineInfo.RemoveWayPoint(wayPoint);
        lineVisual.SeparateWayPoint();
    }

    public bool CanMeet
    {
        get
        {
            if (lineInfo.HasWayPointsInLine) return false;
            else
            {
                return passingElectrons.Count == 2;
            }
        }
    }

    public Node GetTargetFromStartNode(Node startNode)
    {
        if(startNode == lineInfo.startNode)
        {
            return lineInfo.HasWayPointsInLine ? lineInfo.wayPointNodes[0] : lineInfo.endNode;
        }
        else if(startNode == lineInfo.endNode)
        {
            return lineInfo.HasWayPointsInLine ? lineInfo.wayPointNodes[lineInfo.wayPointNodes.Count - 1] : lineInfo.startNode;
        }

        return null;
    }
}

[System.Serializable]
public class LineInfo
{
    public LineState lineState;
    public List<WayPointNode> wayPointNodes;
    public Line line;
    public Node startNode;
    public Node endNode;
    public bool canPass;
    public Vector2 CenterPos { get { return (startNode.transform.position + endNode.transform.position) / 2f; } }
    public bool HasWayPointsInLine { get { return wayPointNodes.Count > 0; } }
    public LineInfo(Line line_, Node startNode_, bool canPass_, LineState lineState_)
    {
        wayPointNodes = new List<WayPointNode>();
        line = line_;
        startNode = startNode_;
        endNode = null;

        canPass = canPass_;
        lineState = lineState_;
    }

    public void AddWayPoint(WayPointNode wayPoint)
    {
        if (wayPointNodes == null)
            wayPointNodes = new List<WayPointNode>();

        wayPointNodes.Add(wayPoint);
    }

    public void RemoveWayPoint(WayPointNode wayPoint)
    {
        wayPointNodes.Remove(wayPoint);
    }

    public Node GetAnotherNode(Node node)
    {
        if (node == startNode)
            return endNode;
        else if (node == endNode)
            return startNode;
        else
            return null;
    }

    public bool IsTwoNodeMatch(Node n1, Node n2)
    {
        return (n1 == startNode && n2 == endNode) || (n1 == endNode && n2 == startNode);
    }

    public Node GetTargetNode(WayPointNode wayPointNode, Node enterNode)
    {
        if(startNode == enterNode)
        {
            int index = wayPointNodes.IndexOf(wayPointNode);
            if(index + 1 <= wayPointNodes.Count - 1)
            {
                return wayPointNodes[index + 1];
            }
            else
            {
                return endNode;
            }
        }
        else if(endNode = enterNode)
        {
            int index = wayPointNodes.IndexOf(wayPointNode);
            if (index - 1 >= 0)
            {
                return wayPointNodes[index - 1];
            }
            else
            {
                return startNode;
            }
        }

        return null;
    }
}
