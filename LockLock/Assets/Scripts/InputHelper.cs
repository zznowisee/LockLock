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

    public static Vector3 CalculateEndPosition(Vector2 mousePos, Vector3 nodePos)
    {
        Vector2 dir = (mousePos - (Vector2)nodePos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle < 0f) angle += 360f;

        float slope = 0f;

        if ((angle > 30f && angle <= 90f) || (angle > 210f && angle <= 270f)) slope = InputHelper.Tan60;
        else if ((angle > 90f && angle < 150f) || (angle > 270f && angle <= 330f)) slope = -InputHelper.Tan60;
        else if ((angle < 60f || angle > 330f) || (angle >= 150f && angle < 210f)) slope = 0f;

        if (slope == 0)
        {
            return new Vector2(mousePos.x, nodePos.y);
        }
        else
        {
            float a = -1 / slope;
            Vector2 norMousePos = mousePos - (Vector2)nodePos;
            float b = norMousePos.y - norMousePos.x * a;

            float targetX = b / (slope - a);
            float targetY = targetX * slope;
            Vector3 target = new Vector3(targetX, targetY) + nodePos;
            return target;
        }
    }
}
