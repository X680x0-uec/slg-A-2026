using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerChange : MonoBehaviour
{
    [SerializeField] public GameManager gameManager;

    [SerializeField] private GameObject playerChangePanel;
    [SerializeField] private TMP_Text WhoText;      // 誰のターンかを表示
    [SerializeField] private Button ReadyButton;    // 準備OKのボタン

    void Start()
    {
        playerChangePanel.SetActive(false);

        //ReadyButtonの設定
        ReadyButton.onClick.AddListener(() => OnReadyButton());
    }
    
    //パネルを表示する
    public void ShowPlyaerChange()
    {
        gameManager.currentPhase = TurnPhase.PlayerChange;
        Dice dice = FindAnyObjectByType<Dice>();
        if (dice != null) dice.RefreshDiceResult();

        // 次のプレイヤーが CPU なら「準備OK」画面を出さずにそのままターン開始！
        Player nextPlayer = gameManager.GetCurrentPlayer();
        if (nextPlayer != null && nextPlayer.isCpu)
        {
            Debug.Log($"[CPU] {nextPlayer.name} のため交代画面を自動スキップします");
            OnReadyButton();
            return;
        }

        // 人間プレイヤーの場合のみ表示する
        if (playerChangePanel != null) playerChangePanel.SetActive(true);


        // 誰のターンかを表示
        if (WhoText != null)
        {
            int turn = gameManager.currentTurn + 1;
            WhoText.text = $"プレイヤー {turn} のターンです\nプレイヤー {turn} 以外は画面を見ないでください";
        }
    }

    //準備OKが押されたとき
    public void OnReadyButton()
    {
        Debug.Log("準備OK");
        gameManager.currentPhase = TurnPhase.WaitingForDice;
        if (playerChangePanel != null) playerChangePanel.SetActive(false);

        gameManager.StartTurn();
    }
}
