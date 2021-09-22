using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionManager : MonoBehaviour
{
    CustomSystem customSystem;

    public static ReactionManager Instance;
    public List<Node> activeNodes;
    public List<Line> activeLines;
    [HideInInspector] public List<Electron> activeElectrons;
    public int desiredElectronNumbers = 0;

    bool isRunning = true;

    private void Awake()
    {
        Instance = this;
        customSystem = FindObjectOfType<CustomSystem>();
        activeLines = new List<Line>();
        activeLines = new List<Line>();
    }

    public void SwitchGameModeToRun()
    {
        isRunning = true;
        RunMachine();
    }

    public void SwitchGameModeToPause()
    {
        isRunning = false;
        StopMachine();
    }

    public void SwitchGameModeToEdit()
    {
        isRunning = false;
        StopMachine();
        ResetReaction();
    }

    public void RunMachine()
    {
        StartCoroutine(NodesReactions());
    }

    public void StopMachine()
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
        if (customSystem.HasElectronsLeft)
        {
            if (desiredElectronNumbers == 0)
            {
                if (isRunning)
                {
                    StartCoroutine(NodesReactions());
                }
                else
                {
                    ResetReaction();
                }
            }
        }
    }

    void ResetReaction()
    {
        desiredElectronNumbers = 0;
    }

    IEnumerator NodesReactions()
    {
        for (int i = 0; i < activeLines.Count; i++)
        {
            activeLines[i].ResetToInit();
        }

        for (int i = 0; i < activeNodes.Count; i++)
        {
            activeNodes[i].Reaction();
        }

        yield return new WaitForEndOfFrame();

        if (!customSystem.HasElectronsLeft)
        {
            customSystem.NoElectronsLeft();
            ResetReaction();
        }
        else
        {
            for (int i = 0; i < activeNodes.Count; i++)
            {
                for (int j = 0; j < activeNodes[i].waitingElectrons.Count; j++)
                {
                    activeNodes[i].waitingElectrons[j].MoveTo();
                }
                activeNodes[i].waitingElectrons.Clear();
            }
        }
    }
}
