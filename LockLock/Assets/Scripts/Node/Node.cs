using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node : MonoBehaviour
{
    [HideInInspector] public NodeInfo nodeInfo;
    [SerializeField] protected Electron pfElectron;
    [HideInInspector] public List<LineInfo> lineInfos;
    public List<Electron> electrons;
}
