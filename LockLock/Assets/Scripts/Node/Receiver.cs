using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receiver : Node
{
    public int ReceiverIndex { get; set; }
    string code;
    UIManager uiManager;

    bool codeIsEmpty;
    public override void Setup(Vector2Int nodeIndex, NodeSlot nodeSlot)
    {
        base.Setup(nodeIndex, nodeSlot);

        uiManager = FindObjectOfType<UIManager>();
        nodeInfo.nodeType = NodeType.Receiver;
        gameObject.name = "ReceiverNode_" + nodeIndex.x + "_" + nodeIndex.y;
        code = "Empty";
        codeIsEmpty = true;
    }

    public void UpdateCode()
    {
        uiManager.UpdateReceiverCode(code, ReceiverIndex);
    }

    public void ResetReceiver()
    {
        code = "Empty";
        codeIsEmpty = true;

        UpdateCode();
    }

    string Combine()
    {
        int minusOneNum = 0;
        int plusOneNum = 0;
        int zeroNum = 0;
        for (int i = 0; i < waitingElectrons.Count; i++)
        {
            switch (waitingElectrons[i].Type)
            {
                case ElectronType.MinusOne:
                    minusOneNum++;
                    break;
                case ElectronType.PlusOne:
                    plusOneNum++;
                    break;
                case ElectronType.Zero:
                    zeroNum++;
                    break;
            }

            Destroy(waitingElectrons[i].gameObject);
        }
        waitingElectrons.Clear();

        int result = minusOneNum * -1 + plusOneNum * 1 + zeroNum * 0;
        return result.ToString();
    }

    public override void Reaction()
    {
        if (codeIsEmpty)
        {
            if(waitingElectrons.Count > 0)
            {
                codeIsEmpty = false;
                code = Combine();
                UpdateCode();
                return;
            }
        }

        if (!codeIsEmpty)
        {
            code += Combine();
            UpdateCode();
        }
    }
}
