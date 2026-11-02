using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardProp //カードの属性
{
    Normal,     //通常カード　後で作る
    Suspicion,  //疑惑カード　後で作る
    Cheeting,   //イカサマカード
}

[System.Serializable]
public struct CardData
{
    public CardProp prop;
    public string CardName;
}

public class CardManager : MonoBehaviour
{
    [Header("カードの情報")]
    public CardData[] CardDataList;

    [Header("カードの見た目")]
    public GameObject cardPrefab; //AssetsのCardを雛型とする

    public Canvas canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Debug.Log("スタート");
        GenerateCard();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateCard()
    {
        Vector3 pos = new Vector3(0,0,0);
        GameObject newCard = Instantiate(cardPrefab, pos, Quaternion.identity);
        newCard.transform.SetParent(canvas.transform, false);
        Debug.Log("プレファブを生成");
    }
}
