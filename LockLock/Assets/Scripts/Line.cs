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

    //LineRenderer lineRenderer;
    EdgeCollider2D edgeColl;

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

    List<WayPoint> wayPoints;

    int length = 1;

    LineVisual lineVisual;
    [SerializeField] float lineWidth = 0.5f;

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

    public void Setup(Vector3 position, Node startNode, LineState lineState)
    {
        hasBeenSetup = true;
        
        //lineRenderer = GetComponent<LineRenderer>();
        edgeColl = GetComponent<EdgeCollider2D>();
        wayPoints = new List<WayPoint>();

        lineVisual = transform.Find("lineVisual").GetComponent<LineVisual>();
        lineVisual.Setup(position);

        //lineRenderer.positionCount = 2;
        //lineRenderer.SetPosition(0, position);
        //lineRenderer.SetPosition(1, position);

        switchStartNode = startNode;
        this.lineState = lineState;
        
        oneWayMark = transform.Find("oneWayMark");
        switch (this.lineState)
        {
            case LineState.Using:
                lineVisual.Material = dottedMat;
                lineVisual.Material.color = palette.dottedUsingCol;

                //lineRenderer.material = dottedMat;
                //lineRenderer.material.color = palette.dottedUsingCol;
                break;
            case LineState.Waiting:
                lineVisual.Material = dottedMat;
                lineVisual.Material.color = palette.dottedWaitingCol;

                //lineRenderer.material = dottedMat;
                //lineRenderer.material.color = palette.dottedWaitingCol;
                break;
            case LineState.Normal:
                lineVisual.Material = normalMat;
                lineVisual.Material.color = palette.defaultCol;

                //lineRenderer.material = normalMat;
                //lineRenderer.material.color = palette.defaultCol;
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
            /*if(lineRenderer == null)
            {
                print("BUG");
                return;
            }*/
            //lineRenderer.SetPosition(length, mousePosition);
            lineVisual.UpdateMesh(mousePosition);
        }

        addWayPoint = false;
    }

    void SetLinePosition(Vector3 position)
    {
        //lineRenderer.SetPosition(length, position);

        //length++;
        //lineRenderer.positionCount++;

        //addWayPoint = true;
    }

    /*public void Highlight()
    {
        lineRenderer.material.color = palette.highlightCol;
    }*/

    public void BeSelect()
    {
        //lineRenderer.material.color = palette.beSelectCol;
        lineVisual.Material.color = palette.beSelectCol;
    }

    public void CancelSelect()
    {
        //lineRenderer.material.color = lineState == LineState.Waiting ? palette.dottedWaitingCol : palette.defaultCol;
        lineVisual.Material.color = lineState == LineState.Waiting ? palette.dottedWaitingCol : palette.defaultCol;
    }

    public void FinishLine(Node endNode)
    {
        //lineRenderer.SetPosition(length, endNode.transform.position);
        lineVisual.ConnectNode(endNode.transform.position);
        Vector2[] points = edgeColl.points;
        points[0] = Vector2.zero;
        points[1] = endNode.transform.position - transform.position;
        edgeColl.points = points;

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
            //lineRenderer.material.color = palette.dottedWaitingCol;
            lineVisual.Material.color = palette.dottedWaitingCol;
        }
        else if(lineState == LineState.Waiting)
        {
            lineState = LineState.Using;
            //lineRenderer.material.color = palette.dottedUsingCol;
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

    public void ConnectWayPoint(WayPoint wayPoint)
    {
        //wayPoints.Add(wayPoint);
        //SetLinePosition(wayPoint.transform.position);

    }

    public void SeparateWayPoint(WayPoint wayPoint)
    {
        wayPoints.Remove(wayPoint);


    }
}
