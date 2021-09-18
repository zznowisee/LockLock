using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeAnimationManager : MonoBehaviour
{
    Animator anim;
    NodeAnimationManager startManager;

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
        NormalNode endNode = InputHelper.GetNodeUnderPosition(position);
        if(endNode != null)
        {
            NodeAnimationManager endManager = endNode.GetComponent<NodeAnimationManager>();
            if(endManager != startManager && endManager != this)
            {
                if(startManager != null)
                {
                    startManager.CancelSelectByLine();
                }
                endManager.BeSelectByLine();
                startManager = endManager;
            }
            else
            {
                if(endManager == this)
                {
                    if (startManager != null)
                    {
                        startManager.CancelSelectByLine();
                        startManager = null;
                    }
                }
            }
        }
        else
        {
            if (startManager != null)
            {
                startManager.CancelSelectByLine();
                startManager = null;
            }
        }
    }
}
