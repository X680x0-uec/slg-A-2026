using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CardScript : MonoBehaviour, IPointerClickHandler
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

        if (effects != null)
        {
            effects.gameObject.SetActive(true);
        }
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager.IsWaitingForDice) return; // サイコロが振られてないとき終了する

        Player currentPlayer = gameManager.GetCurrentPlayer();
        Debug.Log($"カード(ID:{cardId})を使用しました！");
        
        List<Player> allPlayers = new List<Player>(gameManager.players); 

        // イカサマカード（ID 5〜10）なら使用済みフラグを立てる
        if (cardId >= 5 && cardId <= 10)
        {
            currentPlayer.usedCheatingCards[cardId - 5] = true;
        }
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
            // 5~10 イカサマカード効果
            case 5: // イカサマ1番：出目改竄
                break;
            
            case 6: // イカサマ2番：強行突破
                currentPlayer.diceModifier += 15;
                currentPlayer.hideRemainGridPermanent = true;
                currentPlayer.restrictDice1To2Rounds = 3;
                if (dice != null) dice.AppendEffectText(+15);
                Debug.Log("【イカサマ2】強行突破：+15マス移動（代償：ゴール残りマス不可視＆2Rダイス1〜2制限）");
                break;

            case 7: // イカサマ3番：現実拒否
                break;
            
            case 8: // イカサマ4番：全体妨害
                foreach (Player p in allPlayers)
                {
                    if (p != currentPlayer)
                    {
                        p.goalIndex = Mathf.Min(90, p.goalIndex + 10);
                    }
                }
                currentPlayer.hideRankPermanent = true;
                Debug.Log("【イカサマ4】全体妨害：他全員のゴール+10（代償：順位不可視）");
                break;

            case 9: // イカサマ5番：神経妨害
                foreach (Player p in allPlayers)
                {
                    if (p != currentPlayer)
                    {
                        p.afflictedByNerveRounds = Mathf.Max(p.afflictedByNerveRounds, 2);
                    }
                }
                currentPlayer.revealAllInfoRounds = Mathf.Max(currentPlayer.revealAllInfoRounds, 2);
                currentPlayer.hideGridInfoPermanent = true;
                currentPlayer.skipDiceNextRound = true;
                Debug.Log("【イカサマ5】神経妨害：他全員に全代償2R＆自分は全情報閲覧2R（代償：マス番号不可視＆次Rダイス不可）");
                break;

            
            case 10: // イカサマ6番：出目偽装
                currentPlayer.disguiseMoveRounds = 3;
                currentPlayer.hideOtherLastMovePermanent = true;
                Debug.Log("【イカサマ6】出目偽装：2R間移動数偽装(2〜12)（代償：他人の前R移動数不可視）");
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
