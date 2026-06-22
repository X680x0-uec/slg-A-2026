using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Player[] players;
    [SerializeField] private int currentTurn = 0; //誰のターンか
    [SerializeField] private bool isWaitingForDice = true;
    void Start()
    {
        Debug.Log($"ゲーム開始　最初は プレイヤー {currentTurn + 1} の番");
    }

    public void OnDiceRolled(int diceValue)
    {
        if (!isWaitingForDice) return; // サイコロを振ってはいけない状態のときコードを止める

        isWaitingForDice = false; // 二重に振られないようにロック
        Debug.Log($"プレイヤー {currentTurn + 1} が {diceValue}を出した");

        players[currentTurn].Move(diceValue);
    }

    public void OnPlayerMoveComplete()
    {
        currentTurn = (currentTurn + 1) % players.Length;

        Debug.Log($"次は プレイヤー {currentTurn + 1}のターン。");
        isWaitingForDice = true;
    }
}