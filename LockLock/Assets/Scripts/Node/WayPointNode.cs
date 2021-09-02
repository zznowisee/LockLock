using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointNode : Node
{
    private void Awake()
    {
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");

        nodeInfo.nodeType = NodeType.WayPoint;
    }

    public override void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        base.Setup(nodeIndex, nodeSlot);

        gameObject.name = "WayPointNode_" + nodeIndex.x + "_" + nodeIndex.y;
    }

    public void RegisterNewLine(Line line)
    {
        print("RegisterNewLine");

        lineInfos.Add(line.lineInfo);

        beSelectGraphic.gameObject.SetActive(true);
    }

    public void UnregisterLine(Line line)
    {
        print("UnregisterLine");

        lineInfos.Remove(line.lineInfo);

        beSelectGraphic.gameObject.SetActive(false);
    }

    public bool CanReceiveLine(Line line)
    {
        for (int i = 0; i < lineInfos.Count; i++)
            if (lineInfos[i].line == line) return false;
        return true;
    }

    public override Line GetLineFromConnectingNodesWithNode(Node node)
    {
        for (int i = 0; i < lineInfos.Count; i++)
        {
            if(lineInfos[i].IsTwoNodeInSameLine(this, node))
            {
                return lineInfos[i].line;
            }
        }
        return null;
    }

    public override Node GetNextNode(Node preNode, Node lineEnterNode)
    {
        Line line = GetLineFromConnectingNodesWithNode(preNode);
        if (line != null)
        {
            LineInfo lineInfo = line.lineInfo;
            return lineInfo.GetNextNodeFromWayPoint(this, preNode);
        }
        else
        {
            print("Way point line is null");
            return null;
        }
    }
}
