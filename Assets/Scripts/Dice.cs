using UnityEngine;
using TMPro;

public class Dice : MonoBehaviour
{
    private GameManager gameManager;    // GameManagerオブジェクトを記憶
    [SerializeField] private CardScript cardScript;      // 出目をCardScriptに保存
    [SerializeField] private TextMeshProUGUI diceText;
    void Start()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    public void RollDice()
    {
        // GameManagerがダイス待ち状態（isWaitingForDice == true）でない時は一切受け付けない
        if (gameManager != null && !gameManager.IsWaitingForDice)
        {
            Debug.Log("コマが移動中のため、ダイスは振れません");
            return;
        }
        // GameManager から現在のプレイヤー（インスタンス）を取得する
        Player currentPlayer = gameManager.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Debug.LogError("現在のプレイヤーを取得できませんでした！");
            return;
        }

        int totalValue = 0; // 合計値

        if (currentPlayer.NextDiceType == DiceType.Normal)
        {
            int dice1 = Random.Range(1,7); //サイコロの出目を発生
            int dice2 = Random.Range(1,7);
            totalValue = dice1 + dice2;

            Debug.Log($"通常ダイス: {dice1} + {dice2} = {totalValue}");
            if (diceText != null) //出目の表示を変更
            {
                diceText.text = $"DICE: {dice1} + {dice2} = {totalValue}";
            } 
            
            gameManager.OnDiceRolled(totalValue);   //出目を表示
            cardScript.saveDiceValue(totalValue);  //出目をCardScriptに保存
        }
        else if (currentPlayer.NextDiceType == DiceType.Special)
        {
            int dice1 = Random.Range(-1,9);
            int dice2 = Random.Range(-1,9);
            totalValue = dice1 + dice2;

            Debug.Log($"特殊ダイス: {dice1} + {dice2} = {totalValue}");
            if (diceText != null) 
            {
                diceText.text = $"DICE: {dice1} + {dice2} = {totalValue}";
            } 
            gameManager.OnDiceRolled(totalValue);
            cardScript.saveDiceValue(totalValue);
            currentPlayer.NextDiceType = DiceType.Normal; // 通常に戻す
        }
    }
    public void AppendEffectText(int effectValue) // マス効果の演出用にテキストを追記するメソッド
    {
        if (diceText == null) return;

        if (effectValue > 0)
        {
            diceText.text += $" <color=green>+{effectValue}</color>";
        }
        else if (effectValue < 0)
        {
            diceText.text += $" <color=red>{effectValue}</color>";
        }
    }
}