using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSlot : MonoBehaviour
{
    Node node;
    [SerializeField] Vector2Int globalIndex;
    [SerializeField] NodeSlotPalette palette;

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
        globalIndex = globalIndex_;
    }

    public void SetNode(Node node_) => node = node_;
    public void ClearNode() => node = null;
    public void BeSelecting() => spriteRenderer.color = palette.beSelectingCol;
    public void CancelSelecting() => spriteRenderer.color = palette.defaultCol;
}
