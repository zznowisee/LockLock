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

    public SwitchLine BlueLine { get { return blueLine; } }
    public SwitchLine RedLine { get { return redLine; } }

    bool hasDrawSwitchLine = false;

    private void Awake()
    {
        switchWarning = transform.Find("sprite").Find("switchWarning");
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");

        customSystem = FindObjectOfType<CustomSystem>();
    }

    public override void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        base.Setup(nodeIndex, nodeSlot);
        nodeInfo.nodeState = NodeState.Switch;

        combineNodes = new List<SwitchNode>();
        combineLines = new List<Line>();

        gameObject.name = "SwitchNode_" + nodeIndex.x + "_" + nodeIndex.y;
    }

    void CheckSwitchWarning()
    {
        switchWarning.gameObject.SetActive(redLine == null || blueLine == null);
    }

    public bool CombineNodesContain(SwitchNode switchNode) => combineNodes.Contains(switchNode);

    public void CombineWith(ref SwitchNode target)
    {
        SwitchLine switchLine = nodeInfo.currentLine.GetComponent<SwitchLine>();

        combineNodes.Add(target);
        target.combineNodes.Add(this);

        combineLines.Add(nodeInfo.currentLine);
        target.combineLines.Add(nodeInfo.currentLine);

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

        LineRenderer markLineRenderer = nodeInfo.currentLine.SwitchVisualMark.GetComponent<LineRenderer>();
        markLineRenderer.SetPosition(1, target.transform.position);
    }

    public override void DrawLine(Vector3 endPosition)
    {
        if (nodeInfo.currentLine == null)
        {
            nodeInfo.currentLine = Instantiate(pfLine, transform.position, Quaternion.identity, customSystem.lineParent);
        }
        if (!nodeInfo.currentLine.hasBeenSetup)
        {
            if (!hasDrawSwitchLine)
            {
                SwitchLine switchLine = nodeInfo.currentLine.GetComponent<SwitchLine>();
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
                nodeInfo.currentLine.Setup(this, LineState.NormalLine);
            }
        }

        nodeInfo.currentLine.Draw(endPosition);
    }

    public override void FinishDraw(ref Node nextNode)
    {
        nodeInfo.currentLine.FinishLine(nextNode);
        Connect(ref nextNode);

        CancelSelect();
        nodeInfo.currentLine = null;

        CheckSwitchWarning();
    }

    public override void Connect(ref Node connectNode)
    {
        if(nodeInfo.currentLine.GetLineState() == LineState.SwitchLine)
        {
            SwitchLine switchLine = nodeInfo.currentLine.GetComponent<SwitchLine>();
            switch (switchLine.SwitchLineState)
            {
                case SwitchLineState.BlueLine:
                    blueLine = switchLine;
                    break;
                case SwitchLineState.RedLine:
                    redLine = switchLine;
                    break;
            }

            if (connectNode.NodeState == NodeState.Switch)
            {
                SwitchNode switchNode = connectNode.GetComponent<SwitchNode>();

                switch (switchLine.SwitchLineState)
                {
                    case SwitchLineState.BlueLine:
                        if(switchNode.BlueLine == null)
                        {
                            CombineWith(ref switchNode);
                        }
                        break;
                    case SwitchLineState.RedLine:
                        if(switchNode.RedLine == null)
                        {
                            CombineWith(ref switchNode);
                        }
                        break;
                }

                switchNode.CheckSwitchWarning();
            }

            CheckSwitchWarning();
        }

        LineInfo lineInfo = nodeInfo.currentLine.lineInfo;
        lineInfo.endNode = connectNode;

        lineInfos.Add(lineInfo);
        connectNode.LineInfos.Add(lineInfo);

        hasDrawSwitchLine = redLine != null && blueLine != null;
    }

    public override void DeleteLine()
    {
        Destroy(nodeInfo.currentLine.gameObject);
        nodeInfo.currentLine = null;

        CheckSwitchWarning();
    }

    public override void Disconnect(ref Node deleteNode, Line line)
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
        }

        hasDrawSwitchLine = false;
        if(deleteNode.NodeState == NodeState.Switch)
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

                sn.hasDrawSwitchLine = false;
                sn.CheckSwitchWarning();

                combineLines.Remove(line);
                sn.combineLines.Remove(line);
            }
        }
        

        for (int i = 0; i < lineInfos.Count; i++)
        {
            LineInfo lineInfo = lineInfos[i];
            Node anotherNode = lineInfo.GetAnotherNode(this);
            if(anotherNode != null)
            {
                if (anotherNode == deleteNode)
                {
                    lineInfos.Remove(lineInfo);
                    break;
                }
            }
        }

        for (int i = 0; i < deleteNode.LineInfos.Count; i++)
        {
            LineInfo lineInfo = deleteNode.LineInfos[i];
            Node anotherNode = lineInfo.GetAnotherNode(deleteNode);
            if(anotherNode != null)
            {
                if (anotherNode == this)
                {
                    deleteNode.LineInfos.Remove(lineInfo);
                    break;
                }
            }
        }

        CheckSwitchWarning();
    }

    public void SwitchByCombineNode(ref List<Line> changedLines, ref List<Node> changedNodes)
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
            combineNodes[i].SwitchByCombineNode(ref changedLines, ref changedNodes);
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
            combineNodes[i].SwitchByCombineNode(ref changedLines, ref changedNodes);
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
