using Unity.VisualScripting;
using System.Collections; // ★ コルーチンを使うために追加
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

    [SerializeField] private float moveSpeed = 8f; // ★ なめらかな移動速度（お好みで調整）

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    public void Move(int spaces)
    {
        // 一瞬で移動していた処理をコルーチン呼び出しに変更
        StartCoroutine(MoveRoutine(spaces));
    }

    // ★ なめらかに移動する処理（コルーチン）
    private IEnumerator MoveRoutine(int spaces)
    {
        int totalWaypoints = boardManager.GetTotalWaypoints();

        // 出目の数だけ1マスずつ進む
        for (int i = 0; i < spaces; i++)
        {
            // すでにゴール（最後のマス）に到達している場合は途中で抜ける
            if (playerCurrentLocation >= totalWaypoints - 1) break;

            playerCurrentLocation++;
            Vector3 targetPosition = boardManager.GetWaypointPosition(playerCurrentLocation);

            // 目標マスに着くまで毎フレーム少しずつ移動
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null; // 1フレーム待つ
            }

            transform.position = targetPosition; // 位置をきっちり固定
            yield return new WaitForSeconds(0.1f); // 1マスごとにほんの少し立ち止まる（すごろく感）
        }

        // 既存のゴール判定・ログ出力
        if (playerCurrentLocation >= totalWaypoints - 1)
        {
            playerCurrentLocation = totalWaypoints - 1; // 位置をゴールのマスに固定
            Debug.Log("ゴール！");
            gameManager.OnPlayerMoveComplete();
            yield break;
        }

        transform.position = boardManager.GetWaypointPosition(playerCurrentLocation); // ダイスの出目分の位置へ移動
        Debug.Log($"{gameObject.name}が{spaces}マス分移動（現在地: {playerCurrentLocation}）");   

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
