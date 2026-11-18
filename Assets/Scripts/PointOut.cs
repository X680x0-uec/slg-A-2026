using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

//指摘ボタン押したら「あなたはプレイヤー〇ですか？」のボタン押してから画面切り替えした方が分かりやすそう
//ただ、必須の機能ではないので後回し

public class PointOut : MonoBehaviour
{
    [SerializeField] public GameManager gameManager;

    [SerializeField] private GameObject accusationPanel;
    [SerializeField] private Button accuseOther1Button;     // 他プレイヤー1を指摘するボタン
    [SerializeField] private TMP_Text accuseOther1Text;     // ボタン1のテキスト
    [SerializeField] private Button accuseOther2Button;     // 他プレイヤー2を指摘するボタン
    [SerializeField] private TMP_Text accuseOther2Text;     // ボタン2のテキスト
    [SerializeField] private Button skipAccuseButton;       // 「指摘しない」ボタン
    [SerializeField] private TMP_Text WhoText;              // 誰のターンかを表示
    [SerializeField] private TMP_Text other1MoveText;       // 他プレイヤー1の進んだマス数表示用
    [SerializeField] private TMP_Text other2MoveText;       // 他プレイヤー2の進んだマス数表示用


    private int PointOutTurn;   //誰の指摘の番か

    void Start()
    {
        if (skipAccuseButton != null)
        {
            skipAccuseButton.onClick.RemoveAllListeners();
            skipAccuseButton.onClick.AddListener(OnSkipAccusation);
        }

        accusationPanel.SetActive(false);
    }
    
    //パネルを表示する
    public void ShowPanel()
    {
        gameManager.currentPhase = TurnPhase.AccusationPhase;

        //表示する
        accusationPanel.SetActive(true);

Player currentPlayer = GetCurrentPlayer();
        bool isMoveHidden = currentPlayer != null && currentPlayer.IsInfoHidden(currentPlayer.hideOtherLastMovePermanent);

        // 自分以外の2人をリストアップ
        List<Player> otherPlayers = new List<Player>();
        for (int i = 0; i < gameManager.players.Length; i++)
        {
            if (i != gameManager.currentTurn && gameManager.players[i] != null)
            {
                otherPlayers.Add(gameManager.players[i]);
            }
        }

        // 誰のターンかを表示
        if (WhoText != null)
        {
            WhoText.text = $"プレイヤー {gameManager.currentTurn + 1} のターン\nイカサマを指摘する？";
        }

        // 他プレイヤー1のボタン設定
        if (otherPlayers.Count >= 1 && accuseOther1Button != null)
        {
            Player target1 = otherPlayers[0];
            if (accuseOther1Text != null) accuseOther1Text.text = $"{target1.name} を指摘";
            if (other1MoveText != null)
            {
                string stepStr = FormatMoveStepsText(target1, currentPlayer, isMoveHidden);
                other1MoveText.text = $"{target1.name} が進んだマス: {stepStr}";
            }
            accuseOther1Button.onClick.RemoveAllListeners();
            accuseOther1Button.onClick.AddListener(() => OnAccusePlayer(target1));
        }

        // 他プレイヤー2のボタン設定
        if (otherPlayers.Count >= 2 && accuseOther2Button != null)
        {
            Player target2 = otherPlayers[1];
            if (accuseOther2Text != null) accuseOther2Text.text = $"{target2.name} を指摘";
            if (other2MoveText != null)
            {
                string stepStr = FormatMoveStepsText(target2, currentPlayer, isMoveHidden);
                other2MoveText.text = $"{target2.name} が進んだマス: {stepStr}";
            }
            accuseOther2Button.onClick.RemoveAllListeners();
            accuseOther2Button.onClick.AddListener(() => OnAccusePlayer(target2));
        }
    }

    private string FormatMoveStepsText(Player target, Player viewer, bool isHidden)
    {
        int steps = target.GetDisplayMoveStepsForOthers();
        string rawText = steps >= 0 ? $"+{steps}マス" : $"{steps}マス";

        if (viewer != null)
        {
            return viewer.MaskTextIfHidden(rawText, isHidden);
        }
        return rawText;
    }

    // 「指摘しない」を選んだときの処理
    public void OnSkipAccusation()
    {
        if (gameManager.currentPhase != TurnPhase.AccusationPhase)
        {
            Debug.Log("まだそのときではない");
            return;
        }

        Debug.Log($"プレイヤー {gameManager.currentTurn + 1} は指摘を行いませんでした。");
        //次の人の指摘へ
        NextPointOut();
    }

    // どちらか片方を指摘したときの判定処理
    public void OnAccusePlayer(Player targetPlayer)
    {
        if (gameManager.currentPhase != TurnPhase.AccusationPhase)
        {
            Debug.Log("まだそのときではない");
            return;
        }

        Player currentPlayer = GetCurrentPlayer();
        Debug.Log($"プレイヤー {gameManager.currentTurn + 1} が {targetPlayer.name} を指摘しました！");

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

        //次の人の指摘へ
        NextPointOut();
    }

    public Player GetCurrentPlayer()
    {
        if (gameManager.players != null && gameManager.players.Length > 0 && gameManager.currentTurn < gameManager.players.Length)
        {
            return gameManager.players[gameManager.currentTurn];
        }
        return null;
    }

    public void EnterDicePhase()
    {
        gameManager.currentPhase = TurnPhase.WaitingForDice;

        gameManager.StartTurn();
    }

    public void NextPointOut()
    {
        if (accusationPanel != null)
        {
            accusationPanel.SetActive(false);
        }
        
        if (gameManager.currentTurn < gameManager.players.Length - 1)
        {
            //次の人の指摘
            gameManager.currentTurn = (gameManager.currentTurn + 1) % gameManager.players.Length;
            ShowPanel();
        }
        else
        {
            gameManager.currentTurn = (gameManager.currentTurn + 1) % gameManager.players.Length;
            //最後の人の指摘が終わったら盤面に戻る
            EnterDicePhase();
        }
    }
}
