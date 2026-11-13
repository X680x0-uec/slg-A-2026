using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*public enum CardProp //カードの属性
{
    Normal,     //通常カード　後で作る
    Suspicion,  //疑惑カード　後で作る
    Cheeting,   //イカサマカード
}*/

/*[System.Serializable]
public struct CardData
{
    public CardProp prop;
    public string CardName;
}*/

public class CardManager : MonoBehaviour
{
    [Header("カードの情報")]
    //public CardData[] CardDataList;
    [SerializeField] CardData cardData; //CardData.cs

    [Header("カードの見た目")]
    public GameObject cardPrefab; //AssetsのCardを雛型とする

    public Canvas canvas;

    public List<GameObject> destroyList = new List<GameObject>();  //全消去するために生成した瞬間にこのリストに入れる

    public void GenerateCard()
    {
        int length = cardData.CardDataList.Count;   //カードの種類の数
        int[] already = new int[length];            //すでに出た番号（ダブらないように）
        int random;                                 //乱数を格納

        GameObject[] newCard = new GameObject[3];
        Vector3[] card_pos = {
            new Vector3(-100,-150,0),
            new Vector3(0,-150,0),
            new Vector3(100,-150,0),
        };
        
        for (int i = 0; i < 3; i++)
        {
            do
            {
                random = Random.Range(0, length);   //ランダムなカード番号を生成
                bool flg = false;                   //trueになったらやり直し
                for (int j = 0; j < already.Length; j++)
                {
                    if (random == already[j])       //もし被ったらやり直し
                    {
                        flg = true;
                        break;
                    }
                }
                if (!flg)   //被んなかったとき
                {
                    already[i] = random;
                    break;  //ループ抜け出す
                }
            } while (true);
            newCard[i] = Instantiate(cardPrefab, card_pos[i], Quaternion.identity);
            newCard[i].transform.SetParent(canvas.transform, false);
            destroyList.Add(newCard[i]);
        }
        Debug.Log("プレファブを生成");
    }
}
