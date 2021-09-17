using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T>
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;

    public T[,] array;
    private TextMesh[,] debugArray;

    float incXHorizontal;
    float incXVertical;
    float incY;

    public Grid(int _width , int _height , float _cellSize , Vector3 _originPosition , Func<Grid<T> , int ,int ,T> createGridObject)
    {
        width = _width;
        height = _height;
        cellSize = _cellSize;
        originPosition = _originPosition;

        array = new T[width, height];
        debugArray = new TextMesh[width, height];

        incXHorizontal = cellSize;
        incXVertical = cellSize / 2f;
        incY = -Mathf.Sqrt(3f) / 2f * cellSize;

        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                array[x, y] = createGridObject(this, x, y);
            }
        }

        bool showDebug = false;
        if (showDebug)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (y != array.GetLength(1) - 1) Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    if (x != array.GetLength(0) - 1) Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                    if (x != 0 && y != array.GetLength(1) - 1) Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x - 1, y + 1), Color.white, 100f);
                }
            }

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs e) =>
            {
                 //debugArray[e.x, e.y].text = array[e.x, e.y]?.ToString();
            };
        }
    }

    public float GetIncY() => incY;

    //public Vector3 GetWorldPosition(int x, int y) => new Vector3(x, y) * cellSize + originPosition;
    public Vector3 GetWorldPosition(int x, int y) => new Vector3(x * incXHorizontal, y * incY) + originPosition + y * Vector3.right * incXVertical;
    public Vector2 GetMiddlePosition(int x, int y) => GetWorldPosition(x, y) + new Vector3(3f / 4f * cellSize, incY / 2f);

    public void GetXY(Vector3 worldPosition , out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetValue(int x, int y , T value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            array[x, y] = value;
            debugArray[x, y].text = array[x, y].ToString();
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }

    public void SetValue(Vector3 worldPosition , T value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);

        SetValue(x, y, value);
    }

    public T GetValue(int x, int y)
    {
        if(x >= 0 && x < width && y >= 0 && y < height)
        {
            return array[x, y];
        }
        else
        {
            return default(T);
        }
    }

    public T GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public int GetWidth() => width;

    public int GetHeight() => height;

    public void TriggerGridChanged(int x, int y) => OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
}
