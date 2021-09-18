using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    NormalNode node;

    // if can control more than one line , use list instead of one single line
    Line line;
    LineRenderer lineRenderer;

    private void Awake()
    {
        node = GetComponentInParent<NormalNode>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Setup(Line line)
    {
        this.line = line;

        Vector3[] points = new Vector3[]
        {
            transform.position,
            line.lineInfo.CenterPos
        };

        lineRenderer.SetPositions(points);
    }

    public void OnTrainArrive()
    {
        line.SetActiveBaseOnController();
        line.gameObject.SetActive(!line.gameObject.activeSelf);
    }
}
