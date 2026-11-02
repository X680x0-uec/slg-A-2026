using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Player[] players;
    [SerializeField] private int currentTurn = 0; //誰のターンか
    [SerializeField] private bool isWaitingForDice = true;
    
    [Header("参照")]
    [SerializeField] private BoardManager boardManager; // BoardManagerから取得
    [SerializeField] private TextMeshProUGUI remainingGridText; // 残りマス数表示用テキスト
    void Start()
    {
        Debug.Log($"ゲーム開始　最初は プレイヤー {currentTurn + 1} の番");
        UpdateRemainingGridUI();
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
        UpdateRemainingGridUI();
        currentTurn = (currentTurn + 1) % players.Length;

        Debug.Log($"次は プレイヤー {currentTurn + 1}のターン。");
        isWaitingForDice = true;
        UpdateRemainingGridUI();
    }

    public void UpdateRemainingGridUI()
    {
        if (boardManager == null || remainingGridText == null) return;
        
        int totalWaypoints = boardManager.GetTotalWaypoints(); // 全体のマス数を取得
        int currentLocation = players[currentTurn].PlayerCurrentLocation; // 現在ターンのプレイヤーの位置を取得
        int remainingGrids = (totalWaypoints -1) - currentLocation;
        if (remainingGrids < 0) remainingGrids = 0;

        remainingGridText.text = $"REMAIN: {remainingGrids} ";
    }

    public Player GetCurrentPlayer()
{
    if (players != null && players.Length > 0 && currentTurn < players.Length)
    {
        return players[currentTurn];
    }
    return null;
}
}