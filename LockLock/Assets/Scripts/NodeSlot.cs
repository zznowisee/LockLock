using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSlot : MonoBehaviour
{
    Node node;
    [SerializeField] Vector2Int globalIndex;
    [SerializeField] Color beSelectColor;
    [SerializeField] Color defaultColor;
    SpriteRenderer spriteRenderer;

    public Vector2Int GlobalIndex
    {
        get
        {
            return globalIndex;
        }
    }
    public Node Node { get { return node; } }
    public bool IsEmpty { get { return node == null; } }
    public void Setup(Vector2Int globalIndex_)
    {
        spriteRenderer = transform.Find("sprite").GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
        beSelectColor = new Color(1, 1, 1, 1);
        globalIndex = globalIndex_;
    }

    public void SetNode(Node node_) => node = node_;
    public void ClearNode() => node = null;
    public void BeSelect() => spriteRenderer.color = beSelectColor;
    public void CancelSelect() => spriteRenderer.color = defaultColor;
}
