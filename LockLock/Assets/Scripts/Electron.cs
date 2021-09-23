using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ElectronType { MinusOne, PlusOne, Zero }

public class Electron : MonoBehaviour
{
    [SerializeField] float duration = 1f;
    [SerializeField] float easeAmount;

    public event Action OnElectronArriveNode; 

    public ElectronType Type { get; private set; }

    Node target;
    public Node startNode;
    public Node lastNode;
    public Line passingLine;

    public bool willMeetOtherElectron = false;
    public bool needToDestroy = false;

    public Material minusOneMat;
    public Material plusOneMat;
    public Material zeroMat;

    MeshRenderer meshRenderer;

    public void SetupElectronType(ElectronType type_)
    {
        Type = type_;

        switch (Type)
        {
            case ElectronType.MinusOne:
                meshRenderer.material = minusOneMat;
                break;
            case ElectronType.PlusOne:
                meshRenderer.material = plusOneMat;
                break;
            case ElectronType.Zero:
                meshRenderer.material = zeroMat;
                break;
        }
    }

    void Awake()
    {
        meshRenderer = transform.Find("sprite").Find("insideSprite").GetComponent<MeshRenderer>();
    }

    public void Setup(Node startNode_, ElectronType type_)
    {
        startNode = startNode_;
        Type = type_;
        SetupElectronType(type_);
    }

    public void SetupInNode(Node startNode_, Line passingLine_, ElectronType type_)
    {
        startNode = startNode_;
        passingLine = passingLine_;
        SetupElectronType(type_);

        target = passingLine.GetTargetFromStartNode(startNode);
    }

    public void SetupInWayPoint(Node lastNode_, Node target_)
    {
        lastNode = lastNode_;
        target = target_;
    }

    public void MoveTo()
    {
        willMeetOtherElectron = passingLine.CanMeet;
        needToDestroy = passingLine.NeedToDestroy();
        StartCoroutine(OnceMoving());
    }

    IEnumerator OnceMoving()
    {
        Vector3 startPosition = transform.position;
        Vector3 middlePosition = (target.transform.position + transform.position) / 2f;
        Vector3 endPosition = target.transform.position;
        startNode.RemoveElectron(this);
        yield return StartCoroutine(MoveToTarget(middlePosition));

        if (willMeetOtherElectron)
        {
            if (needToDestroy)
            {
                passingLine.passingElectrons.Clear();
                OnElectronArriveNode?.Invoke();
                Destroy(gameObject);
            }
            else
            {
                yield return StartCoroutine(MoveToTarget(startPosition));
                ArriveNode();
            }
        }
        else
        {
            yield return StartCoroutine(MoveToTarget(endPosition));
            ArriveNode();
        }
    }

    void ArriveNode()
    {
        if (target.NodeType != NodeType.WayPoint)
        {
            if (passingLine.lineInfo.lineState == LineState.SwitchLine)
            {
                SwitchNode sw = passingLine.lineInfo.startNode.GetComponent<SwitchNode>();
                FindObjectOfType<CustomSystem>().AddSwitchInfo(sw);
                sw.Switch();
            }
        }

        target.RegisterNewElectron(this);
        OnElectronArriveNode?.Invoke();
    }

    IEnumerator MoveToTarget(Vector3 position)
    {
        float percent = 0f;
        Vector3 startPosition = transform.position;

        while (percent < 1f)
        {
            percent += Time.deltaTime / duration * 2f;
            percent = Mathf.Clamp01(percent);
            float easePercent = Utils.Ease(percent, easeAmount);
            transform.position = Vector3.Lerp(startPosition, position, easePercent);
            yield return null;
        }
    }

    void OnEnable() => ReactionManager.Instance.RegisterElectrons(this);
    void OnDisable() => ReactionManager.Instance.UnregisterElectron(this);
}

