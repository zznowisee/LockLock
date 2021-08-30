using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointNode : Node
{
    List<Line> lines;

    private void Awake()
    {
        lines = new List<Line>();
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");

        nodeInfo.nodeState = NodeState.WayPoint;
    }

    public override void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        base.Setup(nodeIndex, nodeSlot);

        gameObject.name = "WayPointNode_" + nodeIndex.x + "_" + nodeIndex.y;
    }

    public void RegisterNewLine(Line line)
    {
        lines.Add(line);
        print("RegisterNewLine");
        beSelectGraphic.gameObject.SetActive(true);
    }

    public void UnregisterLine(Line line)
    {
        print("UnregisterLine");
        lines.Remove(line);

        beSelectGraphic.gameObject.SetActive(false);
    }

    public bool CanReceiveLine(Line line) => !lines.Contains(line);

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
