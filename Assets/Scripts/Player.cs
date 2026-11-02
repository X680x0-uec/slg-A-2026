using Unity.VisualScripting;
using UnityEngine;

public enum DiceType // ダイスの種類を定義
{
    Normal,  // 通常: 1~6 の 6面ダイス × 2
    Special  // 特殊: -1~8 の 10面ダイス × 2
}

public class Player : MonoBehaviour
{
    //[SerializeField]private int currentSquareIndex = 0;
    private GameManager gameManager;
    private BoardManager boardManager;
    private Dice dice;

    [SerializeField]int playerCurrentLocation = 0; //プレイヤーの現在位置
    public int PlayerCurrentLocation => playerCurrentLocation; // ★外部（GameManager）から現在位置を取得できるようにプロパティを追加
    public DiceType NextDiceType { get; set; } = DiceType.Normal; // 次に振るダイスの種類(通常はNormal)
       void Start()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
        boardManager = Object.FindAnyObjectByType<BoardManager>();
        dice = Object.FindAnyObjectByType<Dice>();
        // 0番目のマスの位置にプレイヤーを配置
        if (boardManager != null)
        {
            transform.position = boardManager.GetWaypointPosition(0);
        }
    }

    public void Move(int diceValue)
    {
        int totalWaypoints = boardManager.GetTotalWaypoints();
        
        playerCurrentLocation += diceValue; // 合計出目分移動
        if (playerCurrentLocation < 0) playerCurrentLocation = 0;
        if (playerCurrentLocation >= totalWaypoints - 1) // ゴール（最後のマス = totalWaypoints - 1）に到達または超えた場合
        {
            playerCurrentLocation = totalWaypoints - 1; // 位置をゴールのマス（49番目）に固定
            transform.position = boardManager.GetWaypointPosition(playerCurrentLocation);
            Debug.Log("ゴール！");
            gameManager.OnPlayerMoveComplete();
            return;
        }

        transform.position = boardManager.GetWaypointPosition(playerCurrentLocation); // ダイスの出目分の位置へ移動
        Debug.Log($"{gameObject.name}が{diceValue}マス分移動（現在地: {playerCurrentLocation}）");   

        ApplyGridEffect(); //  止まったマスの効果を発動
    }

    private void ApplyGridEffect()
    {
        GridEffect effect = boardManager.GetGridEffect(playerCurrentLocation); // BoardManagerから現在地のマス内容を取得
        int value = boardManager.GetGridEffectValue(playerCurrentLocation); // マス内容の数値を取得

        switch(effect) // if (○○ == ○○) {} を省略した書き方
        {
            case GridEffect.Advance: // +マス
              Debug.Log($"【+マス】{value}マス進む！");
              playerCurrentLocation += value;
              if (dice != null) dice.AppendEffectText(value);
              break;

            case GridEffect.Back: // - マス
              Debug.Log($"【-マス】{value}マス戻る！");
              playerCurrentLocation -= value;
              if (dice != null) dice.AppendEffectText(-value);
              break;
            
            case GridEffect.GetCard:
              // 通常・疑惑カード獲得（未実装）
              break;
            
            case GridEffect.ChangeDiceroll: // 出目変化マス 
              NextDiceType = DiceType.Special;
              Debug.Log("【出目変化マス】次のターンは -1〜8 の10面ダイス×2 に変化！");
              break;

            case GridEffect.RetuenCheatingCard: // イカサマカード復活(未実装)
              break;

            case GridEffect.ForcedCheating: // イカサマ強制使用(未実装)
              break;
        }

        transform.position = boardManager.GetWaypointPosition(playerCurrentLocation);  // マス効果適用後の最終位置にコマを再配置
        gameManager.OnPlayerMoveComplete(); // すべての移動・効果処理が終わってからターン終了
    }

}
