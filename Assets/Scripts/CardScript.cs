using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CardScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] CardData cardData;
    [SerializeField] private GameManager gameManager;
    private int cardId; // リストの添え字（0~4:通常カード)

    public static int DiceValue;   //Diceから受け取った値
    private Dice dice;
    private CardManager cardManager;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text effects;


    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cardManager = FindAnyObjectByType<CardManager>();
        dice = FindAnyObjectByType<Dice>();

        if (effects != null) effects.gameObject.SetActive(false);
    }
    
    //Diceが振られたときに呼び出され、値が更新される
    public void saveDiceValue(int diceValue)
    {
        DiceValue = diceValue;
    }
    public void SetCard(int id)
    {
        cardId = id;    //カードIDを受け取る
        if (cardData != null && id < cardData.CardDataList.Count)
        {
            if (nameText != null)
            {
                nameText.text = cardData.CardDataList[id].CardName; // カード表面に名前表示
            }
            if (effects != null)
            {
                effects.text = cardData.CardDataList[id].Explain; // 説明文表示
            }
        }
        nameText.text = cardData.CardDataList[id].CardName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (effects != null)
        {
            effects.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (effects != null)
        {
            effects.gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager.IsWaitingForDice) return; // サイコロが振られてないとき終了する

        Player currentPlayer = gameManager.GetCurrentPlayer();
        Debug.Log($"カード(ID:{cardId})を使用しました！");

        switch (cardId) // ここにカード効果の設定を書く
        {
            // 0~4: 通常カード効果
            case 0: // 1番：出目減少
                currentPlayer.diceModifier -= 1;
                if (dice != null) dice.AppendEffectText(-1);
                Debug.Log($"出目-1をセット（現在の補正: {currentPlayer.diceModifier}）");
                break;

            case 1: // 2番：出目増加
                currentPlayer.diceModifier += 1;
                if (dice != null) dice.AppendEffectText(1);
                Debug.Log($"出目+1をセット（現在の補正: {currentPlayer.diceModifier}）");
                break;

            case 2: // 3番：移動停止
                currentPlayer.isStayAction = true;
                Debug.Log("移動停止をセット（今回は動かずその場のマス効果発動）");
                break;

            case 3: // 4番：周辺確認
                currentPlayer.CheckAheadGrids(10);
                break;

            case 4: // 5番：マス効果無効
                currentPlayer.isGridEffectNullified = true;
                Debug.Log("マス効果無効をセット");
                break;
        }
        // ここで手札からカードを消す
        Debug.Log("カードがタップされました");
        Debug.Log("カードID：" + cardId);
        //players[gameManager.currentTurn].Move(DiceValue);
        foreach (GameObject card in cardManager.destroyList)
        {
            if (card != null)
            {
                Destroy(card);    //カードを削除
            }
        }
        cardManager.destroyList.Clear();
        gameManager.ExecutePlayerMove(); // カード効果適用後、プレイヤーの行動（移動 or 移動停止）を実行
    }

    //public void OnPointerUp(PointerEventData eventData)
    //{
      //  Debug.Log("カードへのタップを終了しました");
    //}

}
