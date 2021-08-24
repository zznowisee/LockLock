using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    List<Vector2Int> tests;

    private void Awake()
    {
        tests = new List<Vector2Int>();

        tests.Add(Vector2Int.down);
        tests.Add(Vector2Int.one);
        tests.Add(Vector2Int.up);
        tests.Add(Vector2Int.left);

        for (int i = 0; i < tests.Count; i++)
        {
            Vector2Int index = tests[i];
            if(index.x == 1)
            {
                tests.Remove(index);
            }
        }

        for (int i = 0; i < tests.Count; i++)
        {
            print(tests[i]);
        }
    }

    private void Update()
    {
        
    }

    public void GenerateMesh()
    {
        
    }
}
