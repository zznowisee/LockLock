using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static TextMesh CreateWorldText(Transform parent , string text , Vector3 localPosition , int fontSize , Color color , TextAnchor textAnchor , TextAlignment textAlignment = TextAlignment.Center ,  int sortingOlder = 10)
    {
        GameObject g = new GameObject("world_text", typeof(TextMesh));
        Transform transform = g.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = g.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.text = text;
        textMesh.alignment = textAlignment;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOlder;
        return textMesh;
    }

    public static float Ease(float x, float easeAmount)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }
}
