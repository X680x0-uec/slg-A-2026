using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointOut : MonoBehaviour
{
    [SerializeField] public GameManager gameManager;

    [SerializeField] private GameObject accusationPanel;
    [SerializeField] private Button accuseOther1Button;     // 他プレイヤー1を指摘するボタン
    [SerializeField] private TMP_Text accuseOther1Text;     // ボタン1のテキスト
    [SerializeField] private Button accuseOther2Button;     // 他プレイヤー2を指摘するボタン
    [SerializeField] private TMP_Text accuseOther2Text;     // ボタン2のテキスト
    [SerializeField] private Button skipAccuseButton;       // 「指摘しない」ボタン

    //パネルを表示する
    public void ShowPanel()
    {
        gameManager.currentPhase = TurnPhase.AccusationPhase;
        //表示する
        accusationPanel.SetActive(true);

        //3人分繰り返す
        for (int turn = 0; turn < gameManager.players.Length; turn++)
        {
            // 自分以外の2人をリストアップ
            List<Player> otherPlayers = new List<Player>();
            for (int i = 0; i < gameManager.players.Length; i++)
            {
                if (i != turn && gameManager.players[i] != null)
                {
                    otherPlayers.Add(gameManager.players[i]);
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
    }

    // どちらか片方を指摘したときの判定処理
    public void OnAccusePlayer(Player targetPlayer)
    {
        if (gameManager.currentPhase != TurnPhase.AccusationPhase) return;

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
    }

    public Player GetCurrentPlayer()
    {
        if (gameManager.players != null && gameManager.players.Length > 0 && gameManager.currentTurn < gameManager.players.Length)
        {
            return gameManager.players[gameManager.currentTurn];
        }
        return null;
    }
}
