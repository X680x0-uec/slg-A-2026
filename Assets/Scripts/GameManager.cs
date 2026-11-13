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

    [SerializeField] private CardManager cardManager;    //カードを生成

    void Start()
    {
        Debug.Log($"ゲーム開始 最初は プレイヤー {currentTurn + 1} の番");
        UpdateActiveCamera(); // ゲーム開始時のカメラ設定
        UpdateRemainingGridUI();
        cardManager.GenerateCard();             //カードを配る
    }

    public void OnDiceRolled(int diceValue)
    {
        if (!isWaitingForDice) return; // サイコロを振ってはいけない状態のときコードを止める

        isWaitingForDice = false; // 二重に振られないようにロック
        Debug.Log($"プレイヤー {currentTurn + 1} が {diceValue}を出した");

        //サイコロ振ったらじゃなくてカードを押したら動いてほしい
        //players[currentTurn].Move(diceValue);

    }

    public void OnPlayerMoveComplete()
    {
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
        if (boardManager == null || remainingGridText == null) return;
        
        int totalWaypoints = boardManager.GetTotalWaypoints(); // 全体のマス数を取得
        int currentLocation = players[currentTurn].PlayerCurrentLocation; // 現在ターンのプレイヤーの位置を取得
        int remainingGrids = (totalWaypoints - 1) - currentLocation;
        if (remainingGrids < 0) remainingGrids = 0;

        remainingGridText.text = $"P{currentTurn + 1} REMAIN: {remainingGrids}";
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