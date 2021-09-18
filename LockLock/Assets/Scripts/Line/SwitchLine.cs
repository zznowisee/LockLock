using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwitchLineState { RedLine, BlueLine }
public class SwitchLine : Line
{
    SwitchLineState switchLineState;

    public NormalNode switchStartNode;
    public NormalNode switchEndNode;
    Transform switchVisualMark;

    [SerializeField] protected Transform pfMarkLine;

    public Transform SwitchVisualMark { get { return switchVisualMark; } }

    public SwitchLineState SwitchLineState { get { return switchLineState; } }

    private void Awake()
    {
        lineVisual = GetComponent<LineVisual>();
    }

    public void Setup(NormalNode startNode, LineState lineState, SwitchLineState switchLineState_)
    {
        hasBeenSetup = true;

        lineVisual = GetComponent<LineVisual>();
        lineVisual.Setup(Vector2.zero, lineWidth, cellSize);

        dirStartNode = startNode;
        switchStartNode = startNode;

        oneWayMark = transform.Find("oneWayMark");
        switchLineState = switchLineState_;
        lineInfo = new LineInfo(this, startNode, switchLineState == SwitchLineState.BlueLine, lineState);
        lineVisual.Material.color = switchLineState == SwitchLineState.BlueLine ? palette.dottedUsingCol : palette.dottedWaitingCol;
    }

    public void Switch()
    {
        if (switchLineState == SwitchLineState.BlueLine)
        {
            switchLineState = SwitchLineState.RedLine;
            lineVisual.Material.color = palette.dottedWaitingCol;
        }
        else if (switchLineState == SwitchLineState.RedLine)
        {
            switchLineState = SwitchLineState.BlueLine;
            lineVisual.Material.color = palette.dottedUsingCol;
        }
    }

    public override void FinishLine(NormalNode endNode)
    {
        lineVisual.ConnectNode(endNode.transform.position - transform.position);
        dirEndNode = endNode;
        lineInfo.endNode = endNode;
        switchEndNode = endNode;

        LineRenderer mark = Instantiate(pfMarkLine, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        mark.SetPosition(0, transform.position);
        mark.SetPosition(1, lineInfo.CenterPos);
        mark.startColor = palette.beSelectCol;
        mark.endColor = palette.defaultCol;
        switchVisualMark = mark.transform;

        gameObject.name = Name;
        print($"Setup New SwitchLine StartNode : '{ lineInfo.startNode.gameObject.name }' " +
              $"SwitchState Is '{ switchLineState }'" +
              $"EndNode : '{ lineInfo.endNode.gameObject.name }'");
    }

    public override void CancelSelecting()
    {
        lineVisual.Material.color = switchLineState == SwitchLineState.BlueLine ? palette.dottedUsingCol : palette.dottedWaitingCol;
    }
}