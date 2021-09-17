using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionManager : MonoBehaviour
{
    public static ReactionManager Instance;
    public List<Node> activeNodes;
    public int desiredElectronNumbers = 0;
    private void Awake()
    {
        Instance = this;
    }

    public void RunMachine()
    {
        StartCoroutine(NodesReactions());
    }

    public void Stop()
    {
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
            StartCoroutine(NodesReactions());
        }
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
