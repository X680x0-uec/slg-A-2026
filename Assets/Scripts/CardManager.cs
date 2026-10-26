using UnityEngine;

public enum CardProp //カードの属性
{
    Normal,     //通常カード　後で作る
    Suspicion,  //疑惑カード　後で作る
    Cheeting,   //イカサマカード
}

[System.Serializable]
public struct CardData //Inspectorタブでマスの座標と種類を指定する。
{
    public CardProp prop;
    public string CardName;
}

public class CardManager : MonoBehaviour
{
    [Header("カードの情報")]
    public CardData[] CardDataList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
