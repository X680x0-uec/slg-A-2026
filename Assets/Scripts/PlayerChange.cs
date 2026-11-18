using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerChange : MonoBehaviour
{
    [SerializeField] public GameManager gameManager;
    [SerializeField] private Dice diceManager;
    [SerializeField] private DiceUI diceUI;
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

        //表示する
        playerChangePanel.SetActive(true);

        // 誰のターンかを表示
        if (WhoText != null)
        {
            int turn = gameManager.currentTurn + 1;
            WhoText.text = $"プレイヤー {turn} のターンです\nプレイヤー {turn} 以外は画面を見ないでください";
        }
    }

    //準備OKが押されたとき
    private void OnReadyButton()
    {
        Debug.Log("準備OK");
        gameManager.currentPhase = TurnPhase.WaitingForDice;
        playerChangePanel.SetActive(false);
        diceUI.diceImage.sprite = diceUI.diceSprites[0];
        diceManager.diceText.text = "→";

        gameManager.StartTurn();
    }
}
