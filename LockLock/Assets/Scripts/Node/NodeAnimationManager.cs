using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeAnimationManager : MonoBehaviour
{
    Animator anim;
    NodeAnimationManager currentLineSelectingNode;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void BeSelectByLine()
    {
        anim.SetBool("BeSelectByLine", true);
    }

    public void CancelSelectByLine()
    {
        anim.SetBool("BeSelectByLine", false);
    }

    public void CheckLineEndPosition(Vector2 position)
    {
        Node endNode = InputHelper.GetNodeUnderPosition(position);
        if(endNode != null)
        {
            NodeAnimationManager nodeAnimationManager = endNode.GetComponent<NodeAnimationManager>();
            if(nodeAnimationManager != currentLineSelectingNode && nodeAnimationManager != this)
            {
                if(currentLineSelectingNode != null)
                {
                    currentLineSelectingNode.CancelSelectByLine();
                }
                nodeAnimationManager.BeSelectByLine();
                currentLineSelectingNode = nodeAnimationManager;
            }
            else
            {
                if(nodeAnimationManager == this)
                {
                    if (currentLineSelectingNode != null)
                    {
                        currentLineSelectingNode.CancelSelectByLine();
                        currentLineSelectingNode = null;
                    }
                }
            }
        }
        else
        {
            if (currentLineSelectingNode != null)
            {
                currentLineSelectingNode.CancelSelectByLine();
                currentLineSelectingNode = null;
            }
        }
    }
}
