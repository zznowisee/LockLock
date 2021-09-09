using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TrainSetup : ScriptableObject
{
    public TrainIndex[] trainIndices;
}

[System.Serializable]
public class TrainIndex
{
    public Train pfTrain;
    public Vector2Int index;
}
