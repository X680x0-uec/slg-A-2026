using UnityEngine;
using UnityEngine.UI; // ボタン制御のために追加
using TMPro;
using System;
using System.Collections.Generic;

// ターンの進行状態
public enum TurnPhase
{
    AccusationPhase,    // 指摘するかどうか選ぶフェーズ（サイコロ前）
    WaitingForDice,     // サイコロ待ち
    WaitingForCard,     // カード選択待ち
    Moving,             // コマ移動・マス効果処理中
    PlayerChange,       // プレイヤーを入れ替えるフェーズ
}

public class GameManager : MonoBehaviour
{
    [Header("プレイヤー設定")]
    public Player[] players; // ★ P1, P2, P3 を登録

    [Header("プレイヤー専用カメラ（パターンB）")]
    [SerializeField] private Camera[] playerCameras; // ★ Camera_P1, Camera_P2, Camera_P3 を登録

    [SerializeField] public int currentTurn = 0; // 誰のターンか
    [SerializeField] public TurnPhase currentPhase = TurnPhase.WaitingForDice;
    public bool IsWaitingForDice => currentPhase == TurnPhase.WaitingForDice; // Dice.cs 用
    public bool CanUseCard => currentPhase == TurnPhase.WaitingForCard;  //CardScript.cs用

    
    [Header("参照")]
    [SerializeField] private BoardManager boardManager; // BoardManagerから取得
    [SerializeField] private TextMeshProUGUI remainingGridText; // 残りマス数表示用テキスト
    [SerializeField] private TextMeshProUGUI otherRemainingGridText;
    [SerializeField] private PointOut pointOut;         //PointOut.csにアクセス
    [SerializeField] private PlayerChange playerChange; //PlayerChange.csにアクセス

    [SerializeField] private CardManager cardManager;    //カードを生成

    void Start()
    {
        Debug.Log($"ゲーム開始 最初は プレイヤー {currentTurn + 1} の番");
        //pointOut.EnterDicePhase();
        StartTurn();
    }

    // ターン開始時の共通処理
    public void StartTurn()
    {
        if (currentPhase != TurnPhase.WaitingForDice) return;

        // 新しくターンが来たプレイヤーの「1周前のカード使用記録」をリセット
        Player currentPlayer = GetCurrentPlayer();
        if (currentPlayer != null)
        {
            currentPlayer.ResetLastUsedCard();
        }

        UpdateActiveCamera();
        UpdateRemainingGridUI();

        if (cardManager != null)
        {
            cardManager.UpdateCardVisibility();
            cardManager.DrawHandForCurrentPlayer();
        }

        //pointOut.ShowPanel(); //デバッグ用
    }

    public void OnDiceRolled(int diceValue)
    {
        if (currentPhase != TurnPhase.WaitingForDice) return;

        currentPhase = TurnPhase.WaitingForCard; // 次はカード選択待ち
        Debug.Log($"プレイヤー {currentTurn + 1} が {diceValue}を出した");
    }

    public void ExecutePlayerMove()
    {
        if (currentPhase != TurnPhase.WaitingForCard) return;

        currentPhase = TurnPhase.Moving; // 移動中ロック
        Player currentPlayer = GetCurrentPlayer();

        if (currentPlayer.isStayAction)
        {
            currentPlayer.StayAndTriggerEffect();
            return;
        }
        int finalSteps = CardScript.DiceValue + currentPlayer.diceModifier;
        currentPlayer.Move(finalSteps);
    }

    public void OnPlayerMoveComplete()
    {
        UpdateRemainingGridUI();
        
        // プレイヤー数3人に合わせて 0 → 1 → 2 → 0 と自動ループ
        currentTurn = (currentTurn + 1) % players.Length;
        Debug.Log($"次は プレイヤー {currentTurn + 1}のターン。");

        //指摘フェーズ
        if (currentTurn == 0)
        {
            Debug.Log("指摘フェーズ");
            pointOut.ShowPanel();
        }
        //プレイヤー交代フェーズ
        else
        {
            playerChange.ShowPlyaerChange();
        }

        StartTurn();
    }
    
    private void UpdateActiveCamera()
    {
        if (playerCameras == null || playerCameras.Length == 0) return;
        
        for (int i = 0; i < playerCameras.Length; i++)
        {
            if (playerCameras[i] != null)
            {
                bool isActive = (i == currentTurn);
                playerCameras[i].enabled = isActive;

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
        if (boardManager == null || remainingGridText == null) return;
        
        int totalWaypoints = boardManager.GetTotalWaypoints(); // 全体のマス数を取得
        int currentLocation = players[currentTurn].PlayerCurrentLocation; // 現在ターンのプレイヤーの位置を取得
        int remainingGrids = (totalWaypoints - 1) - currentLocation;
        if (remainingGrids < 0) remainingGrids = 0;

        remainingGridText.text = $"P{currentTurn + 1} REMAIN: {remainingGrids}";
        
        if (otherRemainingGridText != null)
        {
            string otherDisplay = "";
            for (int i = 0; i < players.Length; i++)
            {
                if (i == currentTurn) continue; 
                int otherRemain = Mathf.Max(0, (totalWaypoints - 1) - players[i].PlayerCurrentLocation);
                otherDisplay += $"P{i + 1} REMAIN: {otherRemain} ";
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
}