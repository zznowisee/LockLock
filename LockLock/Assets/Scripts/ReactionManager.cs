using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public void AddDesiredElectron()
    {
        desiredElectronNumbers++;
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

        if (activeElectrons.Count == 0)
        {
            customSystem.NoElectronsLeft();
            ResetReaction();
        }
        else
        {
            for (int i = 0; i < activeElectrons.Count; i++)
            {
                activeElectrons[i].MoveTo();
            }
        }
    }

    public void RegisterElectrons(Electron e)
    {
        activeElectrons.Add(e);
        e.OnElectronArriveNode += ElectronArriveNode;
    }

    void ElectronArriveNode()
    {
        desiredElectronNumbers--;
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

    public void UnregisterElectron(Electron e)
    {
        activeElectrons.Remove(e);
    }
}
