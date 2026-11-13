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
    [Header("ゲーム管理")]
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (gameManager == null) gameManager = Object.FindAnyObjectByType<GameManager>();
    }

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
        for (int p = 0; p < gameManager.players.Length; p++)
        {
            Player targetPlayer = gameManager.players[p];
            GameObject[] newCard = new GameObject[3];
            
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
                destroyList.Add(newCard[i]);    //一括で消去できるように全てリストに入れておく
                CardScript cardScript = newCard[i].GetComponent<CardScript>();
                cardScript.SetCard(random);     //カードの見た目を設定
                
                // 作ったカードをそのプレイヤーのリストに覚えさせる
                targetPlayer.MyCards.Add(newCard[i]);
            }
        }
        UpdateCardVisibility();
        Debug.Log("プレファブを生成");
    }


    public void DrawNormalOrSuspicionCard(Player player) /// カード獲得マス用：指定されたプレイヤーにカードを＋1枚追加する
    {
        if (player == null || cardPrefab == null || canvas == null) return;

        // 1/3で疑惑、2/3で通常の判定
        bool isSuspicion = (Random.Range(0, 3) == 0);
        Debug.Log($"【カード獲得】{player.name} が {(isSuspicion ? "疑惑" : "通常")}カードを獲得！");

        int length = cardData.CardDataList.Count;
        int random = Random.Range(0, length);

        GameObject addCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        addCard.transform.SetParent(canvas.transform, false);

        // 踏んだプレイヤーだけのリストに＋1枚追加
        player.MyCards.Add(addCard);

        // 表示と並べ順を更新（そのプレイヤーだけ4枚になる）
        UpdateCardVisibility();
    }

    public void UpdateCardVisibility()  // 現在のターンのプレイヤーの手札だけを表示し、枚数に合わせて横に並べる
    {
        if (gameManager == null) return;
        Player currentPlayer = gameManager.GetCurrentPlayer();

        for (int p = 0; p < gameManager.players.Length; p++)
        {
            Player player = gameManager.players[p];
            bool isCurrent = (player == currentPlayer);

            for (int i = 0; i < player.MyCards.Count; i++)
            {
                GameObject cardObj = player.MyCards[i];
                if (cardObj == null) continue;

                cardObj.SetActive(isCurrent);

                if (isCurrent)
                {
                    // 元の座標 (-100, 0, 100...) と同じ規則で枚数分並べる
                    cardObj.transform.localPosition = new Vector3(-100 + (i * 100), -150, 0);
                }
            }
        }
    }
}
