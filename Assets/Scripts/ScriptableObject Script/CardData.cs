using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public List<CardParam> CardDataList = new List<CardParam>();
}

[System.Serializable]
public class CardParam
{
    //リストの添え字をカード番号とする
    [SerializeField] public string Name;       //カードの名前
    [SerializeField] public bool isUsed;        //使われたかどうか
    [SerializeField] public int PulsTurn;       //残り何ターン持続するか（効果）
    [SerializeField] public int MinusTurn;      //残り何ターン持続するか（代償）
    [SerializeField] public string Explain;    //説明文
}