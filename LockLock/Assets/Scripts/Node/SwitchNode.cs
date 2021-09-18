using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchNode : Node
{
    public List<SwitchNode> combineNodes;
    public List<Line> combineLines;
    Transform switchWarning;

    SwitchLine redLine;
    SwitchLine blueLine;

    [SerializeField] SwitchLine pfSwitchLine;

    public SwitchLine BlueLine { get { return blueLine; } }
    public SwitchLine RedLine { get { return redLine; } }

    bool hasDrawRedBlue = false;

    void Awake()
    {
        switchWarning = transform.Find("sprite").Find("switchWarning");
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");

        customSystem = FindObjectOfType<CustomSystem>();
    }

    public override void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        base.Setup(nodeIndex, nodeSlot);
        nodeInfo.nodeType = NodeType.Switch;

        combineNodes = new List<SwitchNode>();
        combineLines = new List<Line>();

        gameObject.name = "SwitchNode_" + nodeIndex.x + "_" + nodeIndex.y;
    }

    void CheckSwitchWarning()
    {
        switchWarning.gameObject.SetActive(redLine == null || blueLine == null);
    }

    public bool CombineNodesContain(SwitchNode switchNode) => combineNodes.Contains(switchNode);

    public void CombineWith(SwitchNode target)
    {
        SwitchLine switchLine = nodeInfo.drawingLine.GetComponent<SwitchLine>();

        combineNodes.Add(target);
        target.combineNodes.Add(this);

        combineLines.Add(nodeInfo.drawingLine);
        target.combineLines.Add(nodeInfo.drawingLine);

        switch (switchLine.SwitchLineState)
        {
            case SwitchLineState.BlueLine:
                blueLine = switchLine;
                target.blueLine = switchLine;
                break;
            case SwitchLineState.RedLine:
                redLine = switchLine;
                target.redLine = switchLine;
                break;
        }

        LineRenderer markLineRenderer = switchLine.SwitchVisualMark.GetComponent<LineRenderer>();
        markLineRenderer.SetPosition(1, target.transform.position);

        hasDrawRedBlue = redLine != null && blueLine != null;
        target.hasDrawRedBlue = target.redLine != null && target.blueLine != null;
    }

    public override void DrawLine()
    {
        if (nodeInfo.drawingLine == null)
        {
            nodeInfo.drawingLine = Instantiate(hasDrawRedBlue ? pfLine : pfSwitchLine, transform.position, Quaternion.identity, customSystem.lineParent);
        }

        if (!nodeInfo.drawingLine.hasBeenSetup)
        {
            if (!hasDrawRedBlue)
            {
                SwitchLine switchLine = nodeInfo.drawingLine.GetComponent<SwitchLine>();
                if(redLine == null)
                {
                    switchLine.Setup(this, LineState.SwitchLine, SwitchLineState.RedLine);
                }
                if(blueLine == null)
                {
                    switchLine.Setup(this, LineState.SwitchLine, SwitchLineState.BlueLine);
                }
            }
            else
            {
                nodeInfo.drawingLine.Setup(this, LineState.NormalLine);
            }
        }

        LineInfo lineInfo = nodeInfo.drawingLine.lineInfo;
        Vector2 pos = InputHelper.CalculateEndPosition(InputHelper.MouseWorldPositionIn2D, lineInfo.wayPointNodes.Count == 0 ? (Node)this : lineInfo.wayPointNodes[lineInfo.wayPointNodes.Count - 1]);
        nodeAnimationManager.CheckLineEndPosition(pos);
        nodeInfo.drawingLine.Draw(pos);
    }

    public override void FinishDraw(Node nextNode)
    {
        nodeInfo.drawingLine.FinishLine(nextNode);
        Connect(nextNode);

        CancelSelecting();
        nodeInfo.drawingLine = null;

        CheckSwitchWarning();
    }

    public override void Connect(Node connectNode)
    {
        if(nodeInfo.drawingLine.GetLineState() == LineState.SwitchLine)
        {
            SwitchLine switchLine = nodeInfo.drawingLine.GetComponent<SwitchLine>();
            switch (switchLine.SwitchLineState)
            {
                case SwitchLineState.BlueLine:
                    blueLine = switchLine;
                    break;
                case SwitchLineState.RedLine:
                    redLine = switchLine;
                    break;
            }

            if (connectNode.NodeType == NodeType.Switch)
            {
                SwitchNode switchNode = connectNode.GetComponent<SwitchNode>();

                switch (switchLine.SwitchLineState)
                {
                    case SwitchLineState.BlueLine:
                        if(switchNode.BlueLine == null)
                        {
                            CombineWith(switchNode);
                        }
                        break;
                    case SwitchLineState.RedLine:
                        if(switchNode.RedLine == null)
                        {
                            CombineWith(switchNode);
                        }
                        break;
                }

                switchNode.CheckSwitchWarning();
            }

            CheckSwitchWarning();
        }

        LineInfo lineInfo = nodeInfo.drawingLine.lineInfo;
        lineInfo.endNode = connectNode;

        lineInfos.Add(lineInfo);
        connectNode.lineInfos.Add(lineInfo);
        hasDrawRedBlue = redLine != null && blueLine != null;
    }

    public void Clear()
    {
        if(redLine != null)
        {
            Node node = GetNodeFromConnectingNodesWithLine(redLine);
            GameObject red = redLine.gameObject;
            Disconnect(node, redLine);
            Destroy(red);
        }
        if(blueLine != null)
        {
            Node node = GetNodeFromConnectingNodesWithLine(blueLine);
            GameObject blue = blueLine.gameObject;
            Disconnect(node, blueLine);
            Destroy(blue);
        }
    }

    public override void DeleteLine()
    {
        base.DeleteLine();
        CheckSwitchWarning();
    }

    public override void Disconnect(Node deleteNode, Line line)
    {
        if(line.GetLineState() == LineState.SwitchLine)
        {
            SwitchLine sl = line.GetComponent<SwitchLine>();
            switch (sl.SwitchLineState)
            {
                case SwitchLineState.BlueLine:
                    blueLine = null;
                    break;
                case SwitchLineState.RedLine:
                    redLine = null;
                    break;
            }
            hasDrawRedBlue = false;
        }

        if(deleteNode.NodeType == NodeType.Switch)
        {
            SwitchNode sn = deleteNode.GetComponent<SwitchNode>();
            if (combineNodes.Contains(sn))
            {
                combineNodes.Remove(sn);
                sn.combineNodes.Remove(this);

                if (line.GetLineState() == LineState.SwitchLine)
                {
                    SwitchLine sl = line.GetComponent<SwitchLine>();
                    if(sl.SwitchLineState == SwitchLineState.BlueLine)
                    {
                        sn.blueLine = null;
                    }else if(sl.SwitchLineState == SwitchLineState.RedLine)
                    {
                        sn.redLine = null;
                    }
                }

                sn.hasDrawRedBlue = false;
                sn.CheckSwitchWarning();

                combineLines.Remove(line);
                sn.combineLines.Remove(line);
            }
        }


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

        CheckSwitchWarning();
    }

    public void SwitchByCombineNode(List<Line> changedLines, List<Node> changedNodes)
    {
        if (changedNodes.Contains(this))
        {
            return;
        }

        if (!changedLines.Contains(blueLine))
        {
            blueLine.Switch();
            changedLines.Add(blueLine);
            blueLine.lineInfo.canPass = false;
        }
        if (!changedLines.Contains(redLine))
        {
            redLine.Switch();
            changedLines.Add(redLine);
            redLine.lineInfo.canPass = true;
        }

        var temp = blueLine;
        blueLine = redLine;
        redLine = temp;

        changedNodes.Add(this);

        for (int i = 0; i < combineNodes.Count; i++)
        {
            combineNodes[i].SwitchByCombineNode(changedLines, changedNodes);
        }
    }

    public void Switch()
    {
        List<Line> changedLines = new List<Line>();
        List<Node> changedNodes = new List<Node>();

        SwitchVisual();
        changedLines.Add(blueLine);
        changedLines.Add(redLine);
        changedNodes.Add(this);

        //change all combined node's line

        for (int i = 0; i < combineNodes.Count; i++)
        {
            combineNodes[i].SwitchByCombineNode(changedLines, changedNodes);
        }
    }

    public void SwitchVisual()
    {
        blueLine.Switch();
        redLine.Switch();

        blueLine.lineInfo.canPass = false;
        redLine.lineInfo.canPass = true;

        var temp = blueLine;
        blueLine = redLine;
        redLine = temp;
    }
}
