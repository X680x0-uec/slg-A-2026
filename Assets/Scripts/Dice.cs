using UnityEngine;
using TMPro;

public class Dice : MonoBehaviour
{
    private GameManager gameManager; // GameManagerオブジェクトを記憶
    [SerializeField] private TextMeshProUGUI diceText;
    void Start()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    public void RollDice()
    {
        int diceValue = Random.Range(1,7); //サイコロの出目を発生

        Debug.Log($"Dice Rolled: : {diceValue}");
        diceText.text = "DICE: " + diceValue.ToString(); //出目の表示を変更
        gameManager.OnDiceRolled(diceValue);
    }
}
