using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointNode : MonoBehaviour
{
    List<Line> lines;

    Transform beSelectGraphic;

    private void Awake()
    {
        lines = new List<Line>();
        beSelectGraphic = transform.Find("sprite").Find("beSelectGraphic");
    }

    public void Connect(Line line)
    {
        lines.Add(line);

        beSelectGraphic.gameObject.SetActive(true);
    }

    public void Separate(Line line)
    {
        lines.Remove(line);

        beSelectGraphic.gameObject.SetActive(false);
    }

    public bool CanReceiveLine(Line line) => !lines.Contains(line);
}
