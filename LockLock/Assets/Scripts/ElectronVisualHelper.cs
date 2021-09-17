using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectronVisualHelper : MonoBehaviour
{
    Electron electron;
    Transform visualMark;

    void Awake()
    {
        electron = GetComponent<Electron>();
        visualMark = transform.Find("originVisual").Find("sprite");
    }

    public void UpdateOriginVisualMark()
    {
        visualMark.position = electron.startNode.transform.position;
    }

    private void Update()
    {
        if (electron.startNode != null)
        {
            UpdateOriginVisualMark();
        }
    }
}
