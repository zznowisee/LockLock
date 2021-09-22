using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ElectronType { MinusOne, PlusOne, Zero }

public class Electron : MonoBehaviour
{
    [SerializeField] float duration = 1f;
    [SerializeField] float easeAmount;

    public ElectronType Type { get; private set; }

    Node target;
    public Node startNode;
    public Node lastNode;
    public Line passingLine;

    public int level = 0;
    public bool canMeet = false;

    public Material minusOneMat;
    public Material plusOneMat;

    MeshRenderer meshRenderer;

    public void SetupInfo(Node startNode_, int level_, Line passingLine_)
    {
        startNode = startNode_;
        level = level_;
        passingLine = passingLine_;
        meshRenderer = transform.Find("sprite").Find("insideSprite").GetComponent<MeshRenderer>();
        Type = ElectronType.PlusOne;
        switch (Type)
        {
            case ElectronType.MinusOne:
                meshRenderer.material = minusOneMat;
                break;
            case ElectronType.PlusOne:
                meshRenderer.material = plusOneMat;
                break;
        }

        target = passingLine.GetTargetFromStartNode(startNode);
    }

    public void SetupInWayPoint(Node lastNode_, Node target_)
    {
        lastNode = lastNode_;
        target = target_;
    }

    public void MoveTo()
    {
        StartCoroutine(MoveToTarget());
        canMeet = passingLine.CanMeet;
    }

    IEnumerator MoveToTarget()
    {
        float percent = 0f;
        Vector3 startPosition = lastNode == null ? startNode.transform.position : lastNode.transform.position;
        Vector2 endPosition = passingLine.CanMeet ? startPosition : target.transform.position;
        Vector3 middlePosition = (target.transform.position + startPosition) / 2f;

        while (percent < 1f)
        {
            percent += Time.deltaTime / duration * 2f;
            percent = Mathf.Clamp01(percent);
            float easePercent = Utils.Ease(percent, easeAmount);
            transform.position = Vector3.Lerp(startPosition, middlePosition, easePercent);
            yield return null;
        }

        percent = 0f;

        while (percent < 1f)
        {
            percent += Time.deltaTime / duration * 2f;
            percent = Mathf.Clamp01(percent);
            float easePercent = Utils.Ease(percent, easeAmount);
            transform.position = Vector3.Lerp(middlePosition, endPosition, easePercent);
            yield return null;
        }

        if (passingLine.CanMeet) print($"Change target by { target.gameObject.name } to {startNode.gameObject.name}");

        if (target.NodeType != NodeType.WayPoint)
        {
            if(passingLine.lineInfo.lineState == LineState.SwitchLine)
            {
                SwitchNode sw = passingLine.lineInfo.startNode.GetComponent<SwitchNode>();
                FindObjectOfType<CustomSystem>().AddSwitchInfo(sw);
                sw.Switch();
            }
        }

        target.RegisterNewElectron(this);
        ReactionManager.Instance.Check();
    }

    void OnEnable() => ReactionManager.Instance.activeElectrons.Add(this);
    void OnDisable() => ReactionManager.Instance.activeElectrons.Remove(this);
}

