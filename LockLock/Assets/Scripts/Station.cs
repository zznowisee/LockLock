using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Station : MonoBehaviour
{
    [SerializeField] StationTrainPalette palette;

    public StationNumber stationNumber;
    Node stayNode;
    TextMeshPro stationTypeText;

    SpriteRenderer spriteRenderer;

    public void Setup(Node stayNode, int index)
    {
        spriteRenderer = transform.Find("sprite").GetComponent<SpriteRenderer>();
        this.stayNode = stayNode;
        stationTypeText = transform.Find("stationTypeText").GetComponent<TextMeshPro>();

        switch (index)
        {
            case 0:
                stationNumber = StationNumber.A;
                spriteRenderer.color = palette.a;
                break;
            case 1:
                stationNumber = StationNumber.B;
                spriteRenderer.color = palette.b;
                break;
            case 2:
                stationNumber = StationNumber.C;
                spriteRenderer.color = palette.c;
                break;
            case 3:
                stationNumber = StationNumber.D;
                spriteRenderer.color = palette.d;
                break;
        }

        stationTypeText.text = stationNumber.ToString();
    }

    public bool CanPass(Train enterTrain)
    {
        return stationNumber == enterTrain.TrainNumber;
    }
}
