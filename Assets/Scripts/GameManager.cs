using UnityEngine;
using TMPro;
using NUnit.Framework;
using System;

public class GameManager : MonoBehaviour
{
    [Header("プレイヤー設定")]
    public Player[] players; // ★ P1, P2, P3 を登録

    [Header("プレイヤー専用カメラ（パターンB）")]
    [SerializeField] private Camera[] playerCameras; // ★ Camera_P1, Camera_P2, Camera_P3 を登録

    [SerializeField] private int currentTurn = 0; // 誰のターンか
    [SerializeField] private bool isWaitingForDice = true;
    public bool IsWaitingForDice => isWaitingForDice; // ダイス側から「今振れる状態か？」を確認できるようにプロパティを追加

    
    [Header("参照")]
    [SerializeField] private BoardManager boardManager; // BoardManagerから取得
    [SerializeField] private TextMeshProUGUI remainingGridText; // 残りマス数表示用テキスト
    [SerializeField] private TextMeshProUGUI otherRemainingGridText;

    [SerializeField] private CardManager cardManager;    //カードを生成

    void Start()
    {
        Debug.Log($"ゲーム開始 最初は プレイヤー {currentTurn + 1} の番");
        UpdateActiveCamera(); // ゲーム開始時のカメラ設定
        UpdateRemainingGridUI();
        cardManager.DrawHandForCurrentPlayer();             //カードを配る
    }

    public void OnDiceRolled(int diceValue)
    {
        if (!isWaitingForDice) return; // サイコロを振ってはいけない状態のときコードを止める

        isWaitingForDice = false; // 二重に振られないようにロック
        Debug.Log($"プレイヤー {currentTurn + 1} が {diceValue}を出した");

        //サイコロ振ったらじゃなくてカードを押したら動いてほしい
        //players[currentTurn].Move(diceValue);

    }
    private int turnStartLocation = 0; // 移動前の位置を記録（ExecutePlayerMoveでセット）
    public void OnPlayerMoveComplete()
    {
        Player movedPlayer = players[currentTurn];

        // 今回の真の移動マス数を記録
        movedPlayer.lastTrueMoveSteps = movedPlayer.PlayerCurrentLocation - turnStartLocation;
    
        // 出目偽装(6番)の次ラウンド・次々ラウンド終了時（残り2または1）に偽装値(2〜12)を決定
        if (movedPlayer.disguiseMoveRounds == 2 || movedPlayer.disguiseMoveRounds == 1)
        {
            movedPlayer.lastDisguisedMoveSteps = UnityEngine.Random.Range(2, 13);
        }
        else
        {
            movedPlayer.lastDisguisedMoveSteps = movedPlayer.lastTrueMoveSteps;
        }
        movedPlayer.TickPenaltyRounds(); // ラウンド数減らす

        UpdateRemainingGridUI();
        
        // プレイヤー数（3人）に合わせて 0 -> 1 -> 2 -> 0 と自動ループ
        currentTurn = (currentTurn + 1) % players.Length;

        UpdateActiveCamera(); // ターン交代時にカメラ切り替え

        if (cardManager != null)
        {
            cardManager.UpdateCardVisibility();
        }

        Debug.Log($"次は プレイヤー {currentTurn + 1}のターン。");
        isWaitingForDice = true;
        UpdateRemainingGridUI();
        cardManager.DrawHandForCurrentPlayer();             //カードを配る
    }

    /// <summary>
    /// 現在ターンのプレイヤー専用カメラだけを有効化（AudioListenerエラー対策含む）
    /// </summary>
    private void UpdateActiveCamera()
    {
        Debug.Log(playerCameras.Length);
        if (playerCameras == null || playerCameras.Length == 0) return;
        
        for (int i = 0; i < playerCameras.Length; i++)
        {
            if (playerCameras[i] != null)
            {
                bool isActive = (i == currentTurn);
                playerCameras[i].enabled = isActive;

                // AudioListenerの重複警告を防止
                AudioListener listener = playerCameras[i].GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = isActive;
                }
            }
        }
    }

    public void UpdateRemainingGridUI()
    {
        Player currentPlayer = players[currentTurn];
        int currentLocation = currentPlayer.PlayerCurrentLocation;
        
        int remainingGrids = Mathf.Max(0, currentPlayer.goalIndex - currentLocation);

        // 強行突破(2番)代償の残りマス文字化け判定
        bool isMyRemainHidden = currentPlayer.IsInfoHidden(currentPlayer.hideRemainGridPermanent);
        string myRemainStr = currentPlayer.MaskTextIfHidden(remainingGrids.ToString(), isMyRemainHidden);
        remainingGridText.text = $"P{currentTurn + 1} REMAIN: {myRemainStr}";

        if (otherRemainingGridText != null)
        {
            // 全体妨害(4番)代償の他プレイヤー残りマス/順位文字化け判定
            bool isOtherHidden = currentPlayer.IsInfoHidden(currentPlayer.hideRankPermanent);
            string otherDisplay = "";
            for (int i = 0; i < players.Length; i++)
            {
                if (i == currentTurn) continue; 
                // 他プレイヤーもそれぞれの goalIndex を使う！
                int otherRemain = Mathf.Max(0, players[i].goalIndex - players[i].PlayerCurrentLocation);
                string otherRemainStr = currentPlayer.MaskTextIfHidden(otherRemain.ToString(), isOtherHidden);
                otherDisplay += $"P{i + 1} REMAIN: {otherRemainStr} ";
            }
            otherRemainingGridText.text = otherDisplay;
        }
    }

    public Player GetCurrentPlayer()
    {
        if (players != null && players.Length > 0 && currentTurn < players.Length)
        {
            return players[currentTurn];
        }
        return null;
    }
    public void ExecutePlayerMove()
    {
        if (isWaitingForDice) return;

        Player currentPlayer = GetCurrentPlayer();

        if (currentPlayer.isStayAction) // もし通常カード③「移動停止」が使われていた場合、止まっていたマス効果を発動
        {
            currentPlayer.StayAndTriggerEffect();
            return;
        }

        int finalSteps = CardScript.DiceValue + currentPlayer.diceModifier;
        currentPlayer.Move(finalSteps);
    }


    
}