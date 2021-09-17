using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electron : MonoBehaviour
{
    [SerializeField] float duration = 1f;
    [SerializeField] float easeAmount;

    Node target;
    public Node startNode;
    public Node lastNode;
    public Line passingLine;

    public int level = 0;
    public bool canMeet = false;

    IEnumerator MoveToTarget()
    {
        float percent = 0f;
        Vector3 startPosition = lastNode == null ? startNode.transform.position : lastNode.transform.position;
        Vector2 endPosition = canMeet ? startPosition : target.transform.position;
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

        if (canMeet) print($"Change target by { target.gameObject.name } to {startNode.gameObject.name}");

        if (target.NodeType != NodeType.WayPoint)
        {
            if(passingLine.lineInfo.lineState == LineState.SwitchLine)
            {
                SwitchNode sw = passingLine.lineInfo.startNode.GetComponent<SwitchNode>();
                sw.Switch();
            }
        }

        target.RegisterNewElectron(this);
        ReactionManager.Instance.Check();
    }

    public void SetupInfo(Node startNode_, int level_, Line passingLine_)
    {
        startNode = startNode_;
        level = level_;
        passingLine = passingLine_;

        target = passingLine.GetTargetFromStartNode(startNode);
        print(target);
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

    void OnEnable() => CustomSystem.activeElectrons.Add(this);
    void OnDisable() => CustomSystem.activeElectrons.Remove(this);
}

