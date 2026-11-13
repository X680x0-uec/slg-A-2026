using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TTestData", menuName = "Scriptable Objects/TTestData")]
public class EnemyParamAsset : ScriptableObject
{
    public List<EnemyParam> EnemyParamList = new List<EnemyParam>();
}

[System.Serializable]
public class EnemyParam
{
    public string EnemyName = "スライム";

    [SerializeField]
    private int maxHP = 100;

    public int MaxHP => maxHP;
}