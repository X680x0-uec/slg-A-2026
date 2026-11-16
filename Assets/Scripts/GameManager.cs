using UnityEngine;
using UnityEngine.UI; // ボタン制御のために追加
using TMPro;
using System;
using System.Collections.Generic;

// ターンの進行状態
public enum TurnPhase
{
    AccusationPhase, // 指摘するかどうか選ぶフェーズ（サイコロ前）
    WaitingForDice,  // サイコロ待ち
    WaitingForCard,  // カード選択待ち
    Moving           // コマ移動・マス効果処理中
}

public class GameManager : MonoBehaviour
{
    [Header("プレイヤー設定")]
    public Player[] players; // ★ P1, P2, P3 を登録

    [Header("プレイヤー専用カメラ（パターンB）")]
    [SerializeField] private Camera[] playerCameras; // ★ Camera_P1, Camera_P2, Camera_P3 を登録

    [SerializeField] private int currentTurn = 0; // 誰のターンか
    [SerializeField] private TurnPhase currentPhase = TurnPhase.WaitingForDice;
    public bool IsWaitingForDice => currentPhase == TurnPhase.WaitingForDice; // Dice.cs 用
    public bool CanUseCard => currentPhase == TurnPhase.WaitingForCard;  //CardScript.cs用

    
    [Header("参照")]
    [SerializeField] private BoardManager boardManager; // BoardManagerから取得
    [SerializeField] private TextMeshProUGUI remainingGridText; // 残りマス数表示用テキスト
    [SerializeField] private TextMeshProUGUI otherRemainingGridText;

    [SerializeField] private CardManager cardManager;    //カードを生成

[Header("指摘選択UI")]
    [SerializeField] private GameObject accusationPanel;          // 選択肢全体を囲むパネル
    [SerializeField] private Button accuseOther1Button;           // 他プレイヤー1を指摘するボタン
    [SerializeField] private TextMeshProUGUI accuseOther1Text;    // ボタン1のテキスト
    [SerializeField] private Button accuseOther2Button;           // 他プレイヤー2を指摘するボタン
    [SerializeField] private TextMeshProUGUI accuseOther2Text;    // ボタン2のテキスト
    [SerializeField] private Button skipAccuseButton;             // 「指摘しない」ボタン

private bool isFirstTurnOfGame = true; // ゲーム開始直後の1ターン目判定用


    void Start()
    {
        if (skipAccuseButton != null)
        {
            skipAccuseButton.onClick.RemoveAllListeners();
            skipAccuseButton.onClick.AddListener(OnSkipAccusation);
        }

        Debug.Log($"ゲーム開始 最初は プレイヤー {currentTurn + 1} の番");
        StartTurn();
    }

    // ターン開始時の共通処理
    private void StartTurn()
    {
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

　　　　　// ゲーム最初の1ターン目は誰もカードを使っていないため、指摘フェーズをスキップ
        if (isFirstTurnOfGame)
        {
            isFirstTurnOfGame = false;
            EnterDicePhase();
        }
        else
        {
            StartAccusationPhase();
        }
    }

// 指摘フェーズを開始し、他2人のボタンをセットアップする
    private void StartAccusationPhase()
    {
        currentPhase = TurnPhase.AccusationPhase;

        if (accusationPanel == null)
        {
            EnterDicePhase();
            return;
        }

        accusationPanel.SetActive(true);

        // 自分以外の2人をリストアップ
        List<Player> otherPlayers = new List<Player>();
        for (int i = 0; i < players.Length; i++)
        {
            if (i != currentTurn && players[i] != null)
            {
                otherPlayers.Add(players[i]);
            }
        }

        // 他プレイヤー1のボタン設定
        if (otherPlayers.Count >= 1 && accuseOther1Button != null)
        {
            Player target1 = otherPlayers[0];
            if (accuseOther1Text != null) accuseOther1Text.text = $"{target1.name} を指摘";
            accuseOther1Button.onClick.RemoveAllListeners();
            accuseOther1Button.onClick.AddListener(() => OnAccusePlayer(target1));
        }

        // 他プレイヤー2のボタン設定
        if (otherPlayers.Count >= 2 && accuseOther2Button != null)
        {
            Player target2 = otherPlayers[1];
            if (accuseOther2Text != null) accuseOther2Text.text = $"{target2.name} を指摘";
            accuseOther2Button.onClick.RemoveAllListeners();
            accuseOther2Button.onClick.AddListener(() => OnAccusePlayer(target2));
        }
    }

// 「指摘しない」を選んだときの処理
    public void OnSkipAccusation()
    {
        if (currentPhase != TurnPhase.AccusationPhase) return;

        Debug.Log($"プレイヤー {currentTurn + 1} は指摘を行いませんでした。");
        EnterDicePhase();
    }

    // どちらか片方を指摘したときの判定処理
    public void OnAccusePlayer(Player targetPlayer)
    {
        if (currentPhase != TurnPhase.AccusationPhase) return;

        Player currentPlayer = GetCurrentPlayer();
        Debug.Log($"プレイヤー {currentTurn + 1} が {targetPlayer.name} を指摘しました！");

        if (targetPlayer.HasUsedTargetCard())
        {
            // 【指摘成功】対象者がイカサマカード（ID 5〜10）を使っていた場合
            Debug.Log($"【指摘成功！】{targetPlayer.name} はイカサマカード（ID:{targetPlayer.lastUsedCardId}）を使用していました！");

            // 二重指摘を防ぐため記録をリセット
            targetPlayer.ResetLastUsedCard();

            // ここに指摘成功時のボーナス・ペナルティを追加できる
        }
        else
        {
            // 【指摘失敗】対象者はイカサマカードを使っていなかった場合
            Debug.Log($"【指摘失敗…】{targetPlayer.name} はイカサマカードを使っていませんでした。ペナルティで出目−４！");

            // 失敗（冤罪）時のペナルティ出目-4
            currentPlayer.diceModifier -=4;
        }
        EnterDicePhase();
    }

private void EnterDicePhase()
    {
        if (accusationPanel != null)
        {
            accusationPanel.SetActive(false);
        }
        currentPhase = TurnPhase.WaitingForDice;
        Debug.Log($"プレイヤー {currentTurn + 1} はサイコロを振れます。");
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
        }int finalSteps = CardScript.DiceValue + currentPlayer.diceModifier;
        currentPlayer.Move(finalSteps);
    }

public void OnPlayerMoveComplete()
    {
        UpdateRemainingGridUI();
        
        // プレイヤー数3人に合わせて 0 → 1 → 2 → 0 と自動ループ
        currentTurn = (currentTurn + 1) % players.Length;
        Debug.Log($"次は プレイヤー {currentTurn + 1}のターン。");

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