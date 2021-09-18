using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointNode : NormalNode
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
        lineInfos.Add(line.lineInfo);
    }

    public void UnregisterLine(Line line)
    {
        lineInfos.Remove(line.lineInfo);
    }

    public bool CanReceiveLine(Line line)
    {
        for (int i = 0; i < lineInfos.Count; i++)
        {
            if (lineInfos[i].line == line)
            {
                return false;
            }
        }

        return true;
    }

    public override void Reaction()
    {
        if(electrons.Count != 0f)
        {
            for (int i = 0; i < electrons.Count; i++)
            {
                ReactionManager.Instance.RegisterElectron();
                Electron electron = electrons[i];
                NormalNode target = electron.passingLine.lineInfo.GetTargetNode(this, electron.startNode);
                electron.SetupInWayPoint(this ,target);
            }
        }
    }
}
