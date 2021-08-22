using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum LineState { Using, Waiting, Normal }
public class Line : MonoBehaviour
{
    LineState lineState;
    
    public LinePalette palette;
    [Header("Material")]
    [SerializeField] Material dottedMat;
    [SerializeField] Material normalMat;
    [Header("Mark Prefabs")]
    [SerializeField] Transform pfMarkLine;

    public bool hasBeenSetup = false;

    bool isSwitchConnectingLine = false;

    bool isControlByNode = false;
    bool isActive = true;

    bool addWayPoint = false;

    // Switch base
    public Node switchStartNode;
    public Node switchEndNode;
    // Direction base
    public Node dirStartNode;
    public Node dirEndNode;

    [SerializeField] bool isOnewayLine = false;
    bool hasChangeDirection = false;

    Vector2 lineCenterPos;
    Transform oneWayMark;

    Transform switchVisualMark;

    List<WayPointNode> wayPoints;

    LineVisual lineVisual;

    [SerializeField] float lineWidth = 0.5f;
    float cellSize = 6f;

    public Vector2 LineCenterPos
    {
        get
        {
            return lineCenterPos;
        }
    }

    public Transform SwitchVisualMark
    {
        get
        {
            return switchVisualMark;
        }
    }

    public List<WayPointNode> WayPoints
    {
        get
        {
            if(wayPoints == null)
            {
                wayPoints = new List<WayPointNode>();
            }
            return wayPoints;
        }
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
    public void SetSwitchConnectingLine()
    {
        isSwitchConnectingLine = true;
    }

    public void Setup(Node startNode, LineState lineState)
    {
        hasBeenSetup = true;
        
        wayPoints = new List<WayPointNode>();

        lineVisual = GetComponent<LineVisual>();
        lineVisual.Setup(Vector2.zero, lineWidth, cellSize);

        switchStartNode = startNode;
        this.lineState = lineState;
        
        oneWayMark = transform.Find("oneWayMark");
        switch (this.lineState)
        {
            case LineState.Using:
                lineVisual.Material = dottedMat;
                lineVisual.Material.color = palette.dottedUsingCol;
                break;
            case LineState.Waiting:
                lineVisual.Material = dottedMat;
                lineVisual.Material.color = palette.dottedWaitingCol;
                break;
            case LineState.Normal:
                lineVisual.Material = normalMat;
                lineVisual.Material.color = palette.defaultCol;
                break;
        }
    }

    public void SetLineDirection()
    {
        if (!isOnewayLine)
        {
            isOnewayLine = true;

            dirStartNode = switchStartNode;
            dirEndNode = switchEndNode;

            Vector2 dir = (dirEndNode.transform.position - dirStartNode.transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            oneWayMark.gameObject.SetActive(true);
            oneWayMark.transform.position = lineCenterPos;
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

                dirStartNode = null;
                dirEndNode = null;

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

    public void CancelSelect()
    {
        lineVisual.Material.color = lineState == LineState.Waiting ? palette.dottedWaitingCol : palette.defaultCol;
    }

    public void FinishLine(Node endNode)
    {
        #region LineRenderer
        //lineRenderer.SetPosition(length, endNode.transform.position);
        /*        Vector2[] points = edgeColl.points;
                points[0] = Vector2.zero;
                points[1] = endNode.transform.position - transform.position;
                edgeColl.points = points;*/
        #endregion

        lineVisual.ConnectNode(endNode.transform.position - transform.position);


        switchEndNode = endNode;

        lineCenterPos = (switchStartNode.transform.position + endNode.transform.position) / 2f;

        if(lineState == LineState.Using || lineState == LineState.Waiting)
        {
            LineRenderer mark = Instantiate(pfMarkLine, transform.position, Quaternion.identity,transform).GetComponent<LineRenderer>();
            mark.SetPosition(0, transform.position);
            mark.SetPosition(1, lineCenterPos);
            mark.startColor = palette.beSelectCol;
            mark.endColor = palette.defaultCol;

            switchVisualMark = mark.transform;
        }
    }

    public void Switch()
    {
        if(lineState == LineState.Using)
        {
            lineState = LineState.Waiting;
            lineVisual.Material.color = palette.dottedWaitingCol;
        }
        else if(lineState == LineState.Waiting)
        {
            lineState = LineState.Using;
            lineVisual.Material.color = palette.dottedUsingCol;
        }
        #region ChangeOneWayLineDirection
        /*        if(otherLineStartNode == null || otherLineEndNode == null)
                {
                    isOnewayLine = false;

                    dirStartNode = null;
                    dirEndNode = null;

                    oneWayMark.gameObject.SetActive(false);
                }
                else
                {
                    isOnewayLine = true;
                    oneWayMark.gameObject.SetActive(true);

                    dirStartNode = switchStartNode;
                    dirEndNode = switchEndNode;

                    oneWayMark.transform.position = lineCenterPos;

                    Vector2 dir;
                    if (dirStartNode == otherLineStartNode || dirEndNode == otherLineEndNode)
                    {
                        dir = (dirEndNode.transform.position - dirStartNode.transform.position).normalized;
                    }
                    else
                    {
                        dir = (dirStartNode.transform.position - dirEndNode.transform.position).normalized;
                        var temp = dirStartNode;
                        dirStartNode = dirEndNode;
                        dirEndNode = temp;
                    }

                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    oneWayMark.rotation = Quaternion.Euler(0, 0, angle);
                }*/
        #endregion
    }

    public void DeleteLine()
    {
        switchStartNode.Disconnect(ref switchEndNode, this);
        Destroy(gameObject);
    }

    public void ConnectWayPoint(WayPointNode wayPoint)
    {
        wayPoints.Add(wayPoint);
        lineVisual.ConnectWayPoint(wayPoint.transform.position - transform.position);
    }

    public void SeparateWayPoint(WayPointNode wayPoint)
    {
        wayPoints.Remove(wayPoint);
        lineVisual.SeparateWayPoint();

    }
}
