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
    Goal,               // ゴール演出
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
        Dice dice = FindAnyObjectByType<Dice>();
        if (dice != null) dice.RefreshDiceResult();
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
        Player currentPlayer = players[currentTurn];
        int currentLocation = currentPlayer.PlayerCurrentLocation;
        
        int remainingGrids = Mathf.Max(0, currentPlayer.goalIndex - currentLocation);

        // 強行突破(2番)代償の残りマス文字化け判定
        bool isMyRemainHidden = currentPlayer.IsInfoHidden(currentPlayer.hideRemainGridPermanent);
        string myRemainStr = currentPlayer.MaskTextIfHidden(remainingGrids.ToString(), isMyRemainHidden);
        remainingGridText.text = $"P{currentTurn + 1} REMAIN: {myRemainStr}";

        if (otherRemainingGridText != null)
        {
     
            // 指摘フェーズのときだけ、他プレイヤー全員の残りマス数を表示する
            if (currentPhase == TurnPhase.AccusationPhase)
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
        else
            {
                // サイコロのターンなど、指摘フェーズ以外は非表示（空）にする
                otherRemainingGridText.text = "";
            }
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

    [Header("イカサマ選択UIパネル（1番：出目改竄 / 3番：現実拒否）")]
    [SerializeField] private GameObject diceFalsifyPanel;      // 出目改竄用パネル
    [SerializeField] private TextMeshProUGUI falsifyValueText; // 現在の変更値(-10〜+10)と移動先プレビュー表示
    [SerializeField] private GameObject realityDenialPanel;    // 現実拒否用パネル

    private int currentFalsifyDelta = 0; // 出目改竄で選択中の増減値（-10 〜 +10）

    // =========================================================
    // 【イカサマ1】出目改竄（±10範囲で変更・位置0〜90）
    // =========================================================
    public void OpenDiceFalsifyUI()
    {
        currentFalsifyDelta = 0;
        if (diceFalsifyPanel != null) diceFalsifyPanel.SetActive(true);
        UpdateFalsifyPreviewText();
    }

    // UIの「+1」「-1」ボタン等から呼ぶ（例：amount に 1 や -1 を渡す）
    public void ChangeFalsifyDelta(int amount)
    {
        currentFalsifyDelta = Mathf.Clamp(currentFalsifyDelta + amount, -10, 10);
        UpdateFalsifyPreviewText();
    }

    private void UpdateFalsifyPreviewText()
    {
        if (falsifyValueText == null) return;
        Player p = GetCurrentPlayer();
        int modifiedDice = CardScript.DiceValue + currentFalsifyDelta;
        int targetGrid = Mathf.Clamp(p.PlayerCurrentLocation + modifiedDice + p.diceModifier, 0, 90);

        // 代償でマス番号が隠されているか判定
        bool isGridHidden = p.IsInfoHidden(p.hideGridInfoPermanent);
        string gridStr = p.MaskTextIfHidden($"{targetGrid}番マス", isGridHidden);

        falsifyValueText.text = $"出目補正: {currentFalsifyDelta:+#;-#;0} (合計: {modifiedDice})\n停止予定: {gridStr}";
    }

    // UIの「決定」ボタンから呼ぶ
    public void ConfirmDiceFalsify()
    {
        Player currentPlayer = GetCurrentPlayer();
        if (diceFalsifyPanel != null) diceFalsifyPanel.SetActive(false);

        // 本来の出目合計値から ±10 変更（位置の下限0、上限90に収まるよう補正）
        int modifiedDice = CardScript.DiceValue + currentFalsifyDelta;
        int targetPos = Mathf.Clamp(currentPlayer.PlayerCurrentLocation + modifiedDice + currentPlayer.diceModifier, 0, 90);
        CardScript.DiceValue = targetPos - currentPlayer.PlayerCurrentLocation - currentPlayer.diceModifier;

        // 代償：次のラウンド以降、自分の出目がずっと見えなくなる
        currentPlayer.hideOwnDicePermanent = true;

        Debug.Log($"【イカサマ1】出目改竄確定：補正 {currentFalsifyDelta:+#;-#;0}（代償：出目永続不可視）");
        ExecutePlayerMove();
    }

    // =========================================================
    // 【イカサマ3】現実拒否（7種類のマス効果に強制変更）
    // =========================================================
    public void OpenRealityDenialUI()
    {
        if (realityDenialPanel != null) realityDenialPanel.SetActive(true);
    }

    // UIの7個のボタンから呼ぶ（0〜6の番号を渡す）
    // 0:イカサマ誘導, 1:カード枚数変動(獲得), 2:±マス(Advance), 3:±マス(Back), 4:出目変化, 5:イカサマ復活, 6:空白
    public void ConfirmRealityDenial(int choiceIndex)
    {
        Player currentPlayer = GetCurrentPlayer();
        if (realityDenialPanel != null) realityDenialPanel.SetActive(false);

        GridEffect chosenEffect = GridEffect.Normal;
        int chosenValue = 0;

        switch (choiceIndex)
        {
            case 0: chosenEffect = GridEffect.ForcedCheating; break;     // 1. イカサマ誘導マス
            case 1: chosenEffect = GridEffect.GetCard; break;            // 2. カード獲得マス
            case 2: chosenEffect = GridEffect.Advance; chosenValue = 3; break; // 3-a. +3マス
            case 3: chosenEffect = GridEffect.Back; chosenValue = 3; break;    // 3-b. -3マス
            case 4: chosenEffect = GridEffect.ChangeDiceroll; break;     // 5. 出目変化マス
            case 5: chosenEffect = GridEffect.RetuenCheatingCard; break; // 6. イカサマ復活マス
            case 6: chosenEffect = GridEffect.Normal; break;             // 7. 空白マス
        }

        // 選択した直後に、止まる予定のマスの見た目（色）を変更する！
        int landingIndex = Mathf.Clamp(currentPlayer.PlayerCurrentLocation + CardScript.DiceValue + currentPlayer.diceModifier, 0, currentPlayer.goalIndex);
        if (boardManager != null)
        {
            boardManager.OverrideGridEffect(landingIndex, chosenEffect, chosenValue);
        }

        // 止まったマスの種類に関係なく、移動完了後に選んだ効果を無条件で発動させる
        currentPlayer.hasForcedGridEffect = true;
        currentPlayer.forcedGridEffect = chosenEffect;
        currentPlayer.forcedGridValue = chosenValue;

        // 代償：マス効果の詳細が永続不可視
        currentPlayer.hideGridDetailPermanent = true;

        Debug.Log($"【イカサマ3】現実拒否確定：{chosenEffect} の効果を発動予約（代償：マス効果詳細不可視）");
        ExecutePlayerMove();
    }
    
}