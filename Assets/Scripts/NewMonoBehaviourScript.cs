using UnityEngine;
using TMPro;

// 【ここを Dice から NewMonoBehaviourScript に変更】
public class NewMonoBehaviourScript : MonoBehaviour
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

        Debug.Log($"サイコロを振った　出目: {diceValue}");
        diceText.text = "DICE: " + diceValue.ToString();
        gameManager.OnDiceRolled(diceValue);
    }
}
