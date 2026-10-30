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
        int diceValue = Random.Range(1,7);

        Debug.Log($"Dice Rolled: : {diceValue}");
        diceText.text = "DICE: " + diceValue.ToString();
        gameManager.OnDiceRolled(diceValue);
    }
}
