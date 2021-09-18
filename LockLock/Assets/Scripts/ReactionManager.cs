using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionManager : MonoBehaviour
{
    CustomSystem customSystem;

    public static ReactionManager Instance;
    public List<Node> activeNodes;
    public int desiredElectronNumbers = 0;

    bool isStopping = true;

    private void Awake()
    {
        Instance = this;
        customSystem = FindObjectOfType<CustomSystem>();
    }

    public void RunMachine()
    {
        if (isStopping)
        {
            isStopping = false;
            StartCoroutine(NodesReactions());
        }
    }

    public void Stop()
    {
        isStopping = true;
        StopAllCoroutines();
    }

    public void RegisterElectron()
    {
        desiredElectronNumbers++;
    }

    public void Check()
    {
        desiredElectronNumbers--;
        if (desiredElectronNumbers == 0)
        {
            if(CustomSystem.activeElectrons.Count > 0)
            {
                if (!isStopping)
                {
                    StartCoroutine(NodesReactions());
                    customSystem.CheckAfterOnceReaction(); 
                }
            }
        }
    }

    public void ResetReaction()
    {
        desiredElectronNumbers = 0;
        isStopping = true;
    }

    IEnumerator NodesReactions()
    {
        for (int i = 0; i < activeNodes.Count; i++)
        {
            activeNodes[i].Reaction();
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < activeNodes.Count; i++)
        {
            for (int j = 0; j < activeNodes[i].electrons.Count; j++)
            {
                activeNodes[i].electrons[j].MoveTo();
            }
            activeNodes[i].electrons.Clear();
        }
    }
}
