using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Train : MonoBehaviour
{
    [SerializeField] StationTrainPalette palette;

    const float duration = 1f;
    [SerializeField] float easeAmount;
    [SerializeField] float rotateDuration = .5f;

    Node preNode;
    Node currentNode;
    WayPointNode currentWayPointNode;

    Line passingLne;
    Line passedLine;

    TextMeshPro trainTypeText;
    [SerializeField] StationNumber trainNumber;

    MeshRenderer meshRenderer;
    int stationIndex;

    CustomSystem customSystem;

    public int StationIndex
    {
        get
        {
            return stationIndex;
        }
    }
    public StationNumber TrainNumber
    {
        get
        {
            return trainNumber;
        }
    }

    public Node CurrentNode
    {
        get
        {
            return currentNode;
        }
    }

    public void Setup(Node startNode, int index)
    {
        meshRenderer = transform.Find("sprite").GetComponent<MeshRenderer>();
        trainTypeText = transform.Find("trainTypeText").GetComponent<TextMeshPro>();
        preNode = currentNode = startNode;
        stationIndex = index;
        switch (index)
        {
            case 0:
                trainNumber = StationNumber.A;
                meshRenderer.material.color = palette.a;
                break;
            case 1:
                trainNumber = StationNumber.B;
                meshRenderer.material.color = palette.b;
                break;
            case 2:
                trainNumber = StationNumber.C;
                meshRenderer.material.color = palette.c;
                break;
            case 3:
                trainNumber = StationNumber.D;
                meshRenderer.material.color = palette.d;
                break;
        }
        trainTypeText.text = trainNumber.ToString();

        customSystem = FindObjectOfType<CustomSystem>();
    }

    public void MoveToNextNode()
    {
        var nextNode = currentNode.GetNextNode(preNode, currentNode);
        passingLne = currentNode.GetLineFromConnectingNodesWithNode(nextNode);

        if (nextNode != null)
        {
            StartCoroutine(MoveToTarget(nextNode));
        }
        else
        {
            print($"{trainNumber} train : Stop!");
        }
    }

    IEnumerator MoveToTarget(Node nextNode)
    {
        yield return StartCoroutine(RotateToTarget(nextNode));

        Vector2 target = nextNode.transform.position;
        float percent = 0f;
        Vector3 startPosition = transform.position;

        while (percent < 1f)
        {
            percent += Time.deltaTime / duration;
            percent = Mathf.Clamp01(percent);
            float easePercent = Utils.Ease(percent, easeAmount);
            transform.position = Vector3.Lerp(startPosition, target, easePercent);
            yield return null;
        }

        preNode = currentNode;
        currentNode = nextNode;

        if (nextNode.NodeState != NodeState.WayPoint)
        {
            if (passingLne.lineInfo.lineState == LineState.SwitchLine)
            {
                SwitchNode sw = passingLne.lineInfo.startNode.GetComponent<SwitchNode>();
                sw.Switch();
                customSystem = FindObjectOfType<CustomSystem>();
                customSystem.AddSwitchInfo(sw);
            }
        }
            
        yield return new WaitForEndOfFrame();

        currentNode.OnTrainArrived?.Invoke();
        if(!CheckArrive())
            MoveToNextNode();
    }

    bool CheckArrive()
    {
        if(currentNode.Station != null)
        {
            if (currentNode.Station.StationNumber == trainNumber)
            {
                print($"{StationIndex} Train Arrive!");
                currentNode.ClearStation();
                gameObject.SetActive(false);
                return true;
            }
        }

        return false;
    }

    IEnumerator RotateToTarget(Node nextNode)
    {
        Vector2 target = nextNode.transform.position;
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion startRotation = transform.rotation;
        float percent = 0f;
        while(percent < 1f)
        {
            percent += Time.deltaTime / rotateDuration;
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(0f, 0, angle), percent);
            yield return null;
        }
    }
}
