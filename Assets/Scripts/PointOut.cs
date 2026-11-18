using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PointOut : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerChange playerChange;
    [SerializeField] private Goal goal;

    [SerializeField] private GameObject accusationPanel;
    [SerializeField] private Button accuseOther1Button;     // 他プレイヤー1を指摘するボタン
    [SerializeField] private TMP_Text accuseOther1Text;     // ボタン1のテキスト
    [SerializeField] private Button accuseOther2Button;     // 他プレイヤー2を指摘するボタン
    [SerializeField] private TMP_Text accuseOther2Text;     // ボタン2のテキスト
    [SerializeField] private Button skipAccuseButton;       // 「指摘しない」ボタン
    [SerializeField] private TMP_Text WhoText;              // 誰のターンか・指摘結果を表示
    [SerializeField] private TMP_Text other1MoveText;       // 他プレイヤー1の進んだマス数表示用
    [SerializeField] private TMP_Text other2MoveText;       // 他プレイヤー2の進んだマス数表示用

    private bool isShowingResult = false; // 結果演出中の連打防止フラグ

    void Start()
    {
        if (skipAccuseButton != null)
        {
            skipAccuseButton.onClick.RemoveAllListeners();
            skipAccuseButton.onClick.AddListener(OnSkipAccusation);
        }

        if (accusationPanel != null) accusationPanel.SetActive(false);
    }

    // パネルを表示する
    public void ShowPanel()
    {
        gameManager.currentPhase = TurnPhase.AccusationPhase;
        isShowingResult = false;

        // 指摘画面に入った瞬間に、前のプレイヤーのサイコロ出目を隠す
        Dice dice = FindAnyObjectByType<Dice>();
        if (dice != null) dice.RefreshDiceResult();

        // 指摘パネルを開くとき、画面にカードが残っていたらすべて消す
        CardManager cm = FindAnyObjectByType<CardManager>();
        if (cm != null)
        {
            foreach (GameObject c in cm.destroyList)
            {
                if (c != null) Destroy(c);
            }
            cm.destroyList.Clear();
        }

        if (accusationPanel != null) accusationPanel.SetActive(true);
        gameManager.UpdateRemainingGridUI();

        Player currentPlayer = GetCurrentPlayer();

        // CPU の指摘番なら自動コルーチンへ
        if (currentPlayer != null && currentPlayer.isCpu)
        {
            StartCoroutine(AutoCpuAccusationRoutine(currentPlayer));
            return;
        }

        // 人間プレイヤーの場合はボタン類を表示状態に戻す
        SetAccusationButtonsActive(true);

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
        if (gameManager.currentPhase != TurnPhase.AccusationPhase || isShowingResult) return;

        Debug.Log($"プレイヤー {gameManager.currentTurn + 1} は指摘を行いませんでした。");
        NextPointOut();
    }

    // どちらか片方を指摘したときの判定処理（人間・CPU共通）
    public void OnAccusePlayer(Player targetPlayer)
    {
        if (gameManager.currentPhase != TurnPhase.AccusationPhase || isShowingResult) return;

        Player currentPlayer = GetCurrentPlayer();
        if (currentPlayer == null || targetPlayer == null) return;

        bool isSuccess = targetPlayer.HasUsedTargetCard();

        if (isSuccess)
        {
            Debug.Log($"【指摘成功！】{currentPlayer.name} が {targetPlayer.name} のイカサマ（ID:{targetPlayer.lastUsedCardId}）を見破った！");
            // ※ ここでは ResetLastUsedCard() を呼ばない！（3人目も同じ相手を指摘できるようにするため）
            currentPlayer.diceModifier += 4;
            targetPlayer.diceModifier -= 10;
        }
        else
        {
            Debug.Log($"【指摘失敗…】{currentPlayer.name} の冤罪！{targetPlayer.name} はイカサマをしていなかった。");
            currentPlayer.diceModifier -= 4;
        }

        // 人間・CPU 共通で結果テキストを 1.5 秒表示してから次へ
        StartCoroutine(ShowAccusationResultRoutine(currentPlayer, targetPlayer, isSuccess));
    }

    private IEnumerator ShowAccusationResultRoutine(Player accuser, Player target, bool isSuccess)
    {
        isShowingResult = true;

        // ボタンと移動数テキストを一時的に隠して結果メッセージを見やすくする
        SetAccusationButtonsActive(false);
        if (other1MoveText != null) other1MoveText.text = "";
        if (other2MoveText != null) other2MoveText.text = "";

        if (WhoText != null)
        {
            WhoText.text = isSuccess
                ? $"{accuser.name} が {target.name} を指摘！\n<color=red>【指摘成功！】</color>"
                : $"{accuser.name} が {target.name} を指摘！\n<color=blue>【指摘失敗…】</color>";
        }

        yield return new WaitForSeconds(1.5f);

        isShowingResult = false;
        NextPointOut();
    }

    private IEnumerator AutoCpuAccusationRoutine(Player cpu)
    {
        SetAccusationButtonsActive(false);
        if (other1MoveText != null) other1MoveText.text = "";
        if (other2MoveText != null) other2MoveText.text = "";
        if (WhoText != null) WhoText.text = $"{cpu.name} が思考中…";

        yield return new WaitForSeconds(0.7f);

        List<Player> others = new List<Player>();
        Player obviousSuspect = null; // 移動量が1以下 or 13以上の明らかな容疑者
        Player actualCheater = null;  // 実際にイカサマを使った相手（勘で気づく用）

        for (int i = 0; i < gameManager.players.Length; i++)
        {
            Player other = gameManager.players[i];
            if (other == null || other == cpu) continue;
            others.Add(other);

            int moved = other.GetDisplayMoveStepsForOthers();
            if (moved <= 1 || moved >= 13)
            {
                obviousSuspect = other;
            }
            if (other.HasUsedTargetCard())
            {
                actualCheater = other;
            }
        }

        Player chosenTarget = null;

        // 1. 移動量が明らかに異常（1以下 or 13以上）なら 90% で指摘
        if (obviousSuspect != null && Random.value < 0.90f)
        {
            chosenTarget = obviousSuspect;
        }
        // 2. 数値が普通でも、実際にイカサマを使っていれば 50% の勘で指摘してくる
        else if (actualCheater != null && Random.value < 0.50f)
        {
            chosenTarget = actualCheater;
        }
        // 3. 誰も使っていなくても 25% で誰かを疑う（冤罪リスク）
        else if (others.Count > 0 && Random.value < 0.25f)
        {
            chosenTarget = others[Random.Range(0, others.Count)];
        }

        if (chosenTarget != null)
        {
            OnAccusePlayer(chosenTarget);
        }
        else
        {
            if (WhoText != null) WhoText.text = $"{cpu.name} は指摘を行いませんでした";
            yield return new WaitForSeconds(1.0f);
            NextPointOut();
        }
    }

    private void SetAccusationButtonsActive(bool active)
    {
        if (accuseOther1Button != null) accuseOther1Button.gameObject.SetActive(active);
        if (accuseOther2Button != null) accuseOther2Button.gameObject.SetActive(active);
        if (skipAccuseButton != null) skipAccuseButton.gameObject.SetActive(active);
    }

    public Player GetCurrentPlayer()
    {
        if (gameManager.players != null && gameManager.players.Length > 0 && gameManager.currentTurn < gameManager.players.Length)
        {
            return gameManager.players[gameManager.currentTurn];
        }
        return null;
    }
    public void NextPointOut()
    {
        if (accusationPanel != null)
        {
            accusationPanel.SetActive(false);
        }

        if (gameManager.currentTurn < gameManager.players.Length - 1)
        {
            // 次の人の指摘へ（まだ全員の指摘が終わっていないのでカード履歴は保持）
            gameManager.currentTurn = (gameManager.currentTurn + 1) % gameManager.players.Length;
            ShowPanel();
        }
        else
        {
            // ★ 全員の指摘フェーズが終わったタイミングで、全員の「直前使用カード記録」をリセット
            for (int i = 0; i < gameManager.players.Length; i++)
            {
                if (gameManager.players[i] != null)
                {
                    gameManager.players[i].ResetLastUsedCard();
                }
            }

            goal.GoalJudge();
            if (gameManager.currentPhase != TurnPhase.Goal)
            {
                gameManager.currentTurn = (gameManager.currentTurn + 1) % gameManager.players.Length;
                playerChange.ShowPlyaerChange();
            }
        }
    }
}