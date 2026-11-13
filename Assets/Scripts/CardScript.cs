using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CardData cardData;
    [SerializeField] private GameManager gameManager;
    private int cardId; // リストの添え字（0~4:通常カード)

    public static int DiceValue;   //Diceから受け取った値

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }
    
    //Diceが振られたときに呼び出され、値が更新される
    public void saveDiceValue(int diceValue)
    {
        DiceValue = diceValue;
    }
    public void SetCardId(int id)
    {
        cardId = id;
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
                Debug.Log($"出目-1をセット（現在の補正: {currentPlayer.diceModifier}）");
                break;

            case 1: // 2番：出目増加
                currentPlayer.diceModifier += 1;
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
    }

    //public void OnPointerUp(PointerEventData eventData)
    //{
      //  Debug.Log("カードへのタップを終了しました");
    //}
}
