using UnityEngine;

public static class InputHelper
{
    static Camera mainCamera;
    static Camera MainCamera
    {
        get
        {
            if(mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            return mainCamera;
        }
    }

    public static float Tan60 { get { return Mathf.Tan(Mathf.Deg2Rad * 60f); } }

    public static Vector2 MouseWorldPositionIn2D
    {
        get
        {
            return MainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public static bool IsMouseOverUIObject()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public static GameObject GetObjectUnderMousePosition(LayerMask layer)
    {
        Vector2 mousePos = MouseWorldPositionIn2D;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector3.forward, float.MaxValue, layer);
        if (hit.collider)
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    public static GameObject GetObjectUnderMousePosition()
    {
        Vector2 mousePos = MouseWorldPositionIn2D;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector3.forward, float.MaxValue);
        if (hit.collider)
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    public static GameObject GetObjectUnderPosition(LayerMask layer, Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector3.forward, float.MaxValue, layer);
        if (hit.collider)
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    public static bool IsTheseKeysDown(params KeyCode[] keyCodes)
    {
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsTheseKeysHeld(params KeyCode[] keyCodes)
    {
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKey(keyCodes[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static Node GetNodeUnderPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector3.forward, float.MaxValue);
        if (hit.collider)
        {
            Node node = hit.collider.gameObject.GetComponent<Node>();
            return node;
        }

        return null;
    }

    public static Vector3 CalculateEndPosition(Vector2 mousePos, Node startNode)
    {
        Vector2 dir = (mousePos - (Vector2)startNode.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle < 0f) angle += 360f;

        float slope = 0f;

        if ((angle > 30f && angle <= 90f) || (angle > 210f && angle <= 270f)) slope = Tan60;
        else if ((angle > 90f && angle < 150f) || (angle > 270f && angle <= 330f)) slope = -Tan60;
        else if ((angle < 60f || angle > 330f) || (angle >= 150f && angle < 210f)) slope = 0f;

        Vector3 unclampedEndPosition;
        if (slope == 0)
        {
            unclampedEndPosition = new Vector3(mousePos.x, startNode.transform.position.y);
        }
        else
        {
            float a = -1 / slope;
            Vector2 norMousePos = mousePos - (Vector2)startNode.transform.position;
            float b = norMousePos.y - norMousePos.x * a;

            float targetX = b / (slope - a);
            float targetY = targetX * slope;
            unclampedEndPosition = new Vector3(targetX, targetY) + startNode.transform.position;
        }

        float dist = Vector3.Distance(unclampedEndPosition, startNode.transform.position);
        Vector2Int offset = Vector2Int.zero;

        if (angle < 30f || angle > 330f) offset = Vector2Int.right;
        if (angle > 30f && angle < 90f) offset = new Vector2Int(1, -1);
        if (angle >= 90f && angle < 150f) offset = new Vector2Int(0, -1);
        if (angle >= 150f && angle < 210f) offset = -Vector2Int.right;
        if (angle >= 210f && angle < 270f) offset = new Vector2Int(-1, 1);
        if (angle >= 270f && angle < 330f) offset = new Vector2Int(0, 1);

        Node node = GridSystem.Instance.GetNodeFromGlobalIndex(startNode.GlobalIndex + offset);
        return node == null ? startNode.transform.position : dist > 6f ? node.transform.position : unclampedEndPosition;
    }
}
