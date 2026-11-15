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


    // 現在のプレイヤーの山札（通常0〜4 ＋ 未使用のイカサマ5〜10）からランダムに3枚引いて手札にする
    public void DrawHandForCurrentPlayer()
    {
        // 前のターンの手札表示をクリア
        foreach (GameObject card in destroyList)
        {
            if (card != null) Destroy(card);
        }
        destroyList.Clear();

        Player currentPlayer = gameManager.GetCurrentPlayer();
        if (currentPlayer == null) return;
        currentPlayer.MyCards.Clear();

        // 1. 今引けるカードIDの候補リスト（山札）を作る
        List<int> availableCardIds = new List<int>();

        // 通常カード（ID: 0〜4）は毎回山札に入っている
        for (int i = 0; i <= 4; i++)
        {
            if (i < cardData.CardDataList.Count) availableCardIds.Add(i);
        }

        // イカサマカード（ID: 5〜10）は「未使用（usedCheatingCards[i - 5] == false）」のものだけ山札に入れる
        for (int i = 5; i <= 10; i++)
        {
            int cheatIndex = i - 5; // 0〜5
            if (i < cardData.CardDataList.Count && !currentPlayer.usedCheatingCards[cheatIndex])
            {
                availableCardIds.Add(i);
            }
        }

        // 2. 山札から重複なしで最大3枚引く
        int drawCount = Mathf.Min(3, availableCardIds.Count);
        Vector3[] card_pos = {
            new Vector3(-110, -150, 0),
            new Vector3(0, -150, 0),
            new Vector3(110, -150, 0),
        };

        for (int i = 0; i < drawCount; i++)
        {
            int randIdx = Random.Range(0, availableCardIds.Count);
            int pickedCardId = availableCardIds[randIdx];
            availableCardIds.RemoveAt(randIdx); // 重複防止

            GameObject cardObj = Instantiate(cardPrefab, card_pos[i], Quaternion.identity);
            cardObj.transform.SetParent(canvas.transform, false);

            CardScript script = cardObj.GetComponent<CardScript>();
            if (script != null)
            {
                script.SetCard(pickedCardId);
            }

            destroyList.Add(cardObj);
            currentPlayer.MyCards.Add(cardObj);
        }
    }


    public void DrawNormalOrSuspicionCard(Player player) /// カード獲得マス用：指定されたプレイヤーにカードを＋1枚追加する
    {
        if (player == null || cardPrefab == null || canvas == null) return;

        // 1/3で疑惑、2/3で通常の判定
        bool isSuspicion = (Random.Range(0, 3) == 0);
        Debug.Log($"【カード獲得】{player.name} が {(isSuspicion ? "疑惑" : "通常")}カードを獲得！");

        int length = cardData.CardDataList.Count;
        int random = Random.Range(0, length);

        int cardIndex = player.MyCards.Count; // 0,1,2...　カード枚数
        Vector3 spawnPos = new Vector3(-110 + (cardIndex*110),-150,0);

        GameObject addCard = Instantiate(cardPrefab, spawnPos, Quaternion.identity);
        addCard.transform.SetParent(canvas.transform, false);

        CardScript script = addCard.GetComponent<CardScript>();
        if (script != null)
        {
            script.SetCard(random);
        }

        // 踏んだプレイヤーだけのリストに＋1枚追加
        player.MyCards.Add(addCard);

        // 表示と並べ順を更新（そのプレイヤーだけ4枚になる）
        //UpdateCardVisibility();
    }

    public void UpdateCardVisibility()  // 現在のターンのプレイヤーの手札だけを表示し、枚数に合わせて横に並べる
    {
        /*if (gameManager == null) return;
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
        }*/
    }
}