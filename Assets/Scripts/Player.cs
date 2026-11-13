using System.Collections; // ★ コルーチンを使うために追加
using System.Collections.Generic; 
using UnityEngine;

public enum DiceType // ダイスの種類を定義
{
    Normal,  // 通常: 1~6 の 6面ダイス × 2
    Special  // 特殊: -1~8 の 10面ダイス × 2
}

public class Player : MonoBehaviour
{
    private GameManager gameManager;
    private BoardManager boardManager;
    private Dice dice;

    [SerializeField]int playerCurrentLocation = 0; //プレイヤーの現在位置
    public int PlayerCurrentLocation => playerCurrentLocation; // ★外部（GameManager）から現在位置を取得できるようにプロパティを追加
    public DiceType NextDiceType { get; set; } = DiceType.Normal; // 次に振るダイスの種類(通常はNormal)
    private List<GameObject> myCards = new List<GameObject>();  // プレイヤー個別の手札リスト
    public List<GameObject> MyCards => myCards;

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
        StartCoroutine(MoveRoutine(spaces, triggerEffectOnComplete: true));
    }

    // ★ なめらかに移動する処理（コルーチン）
    private IEnumerator MoveRoutine(int spaces, bool triggerEffectOnComplete = false)
    {
        int totalWaypoints = boardManager.GetTotalWaypoints();
        int stepDir = spaces > 0 ? 1 : -1; // 進む (+1) か 戻る (-1) かの方向
        int absSpaces = Mathf.Abs(spaces); // 移動マス数の絶対値

        // 出目の数だけ1マスずつ進む
        for (int i = 0; i < Mathf.Abs(spaces); i++)
        {
          // ゴール到達またはスタート（0番目）未満にならない安全対策
            if (stepDir > 0 && playerCurrentLocation >= totalWaypoints - 1) break;
            if (stepDir < 0 && playerCurrentLocation <= 0) break;

            if (spaces > 0) playerCurrentLocation++;
            else playerCurrentLocation--;
            
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

        Debug.Log($"{gameObject.name}が移動完了（現在地: {playerCurrentLocation}）");

        // ダイスによる通常移動の場合のみ、止まったマスの効果を発動
        if (triggerEffectOnComplete)
        {
            yield return StartCoroutine(ApplyGridEffectRoutine());
        }
    }

    /// <summary>
    //  マス効果一覧
    /// </summary>
    private IEnumerator ApplyGridEffectRoutine()
    {
        // 通常カード5番「マス効果無効」が使われていたら、マス効果を発動せずにターン終了
        if (isGridEffectNullified)
        {
            Debug.Log("【マス効果無効】カードの効果により、このマスの効果は無効化されました！");
            ResetTurnCardEffects(); // 使い終わったのでリセット
            if (gameManager != null) gameManager.OnPlayerMoveComplete();
            yield break; // ここで終了
        }

        GridEffect effect = boardManager.GetGridEffect(playerCurrentLocation); // BoardManagerから現在地のマス内容を取得
        int value = boardManager.GetGridEffectValue(playerCurrentLocation); // マス内容の数値を取得

        switch(effect) // if (○○ == ○○) {} を省略した書き方
        {
            case GridEffect.Advance: // +マス
              Debug.Log($"【+マス】{value}マス進む！");
              if (dice != null) dice.AppendEffectText(value);
              yield return StartCoroutine(MoveRoutine(value,triggerEffectOnComplete: false));
              break;

            case GridEffect.Back: // - マス
              Debug.Log($"【-マス】{value}マス戻る！");
              if (dice != null) dice.AppendEffectText(-value);
              yield return StartCoroutine(MoveRoutine(-value,triggerEffectOnComplete: false));
              break;
            
            case GridEffect.GetCard:   // 通常・疑惑カード獲得
              Debug.Log("【カード獲得マス】手札が1枚増える！");
              CardManager cardManager = Object.FindAnyObjectByType<CardManager>();
              if (cardManager != null)
              {
                cardManager.DrawNormalOrSuspicionCard(this); // ★ 「this（自分自身）」を渡す！
              }
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
    
    /// <summary>
    /// カード効果実行
    /// </summary>

    // --- 今回のターンだけのカード効果状態 ---
    public int diceModifier = 0;               // 1番・2番用：出目補正（-1 や +1）
    public bool isStayAction = false;          // 3番用：移動停止フラグ
    public bool isGridEffectNullified = false; // 5番用：マス効果無効フラグ

    // 【通常カード3：移動停止】移動せず現在地のマス効果だけ発動する
    public void StayAndTriggerEffect()
    {
        Debug.Log("【移動停止】現在地に留まった場合、その現在地のマス効果を発動する");
        StartCoroutine(ApplyGridEffectRoutine());
    }

    // 【通常カード4：周辺確認】現在地の次のマスから10マス先まで調べる
    public void CheckAheadGrids(int range)
    {
        if (boardManager == null) return;

        int totalWaypoints = boardManager.GetTotalWaypoints();
        string resultText = $"【周辺確認】現在地({playerCurrentLocation}番)から先 {range}マスの情報：\n";

        for (int i = 1; i <= range; i++)
        {
            int targetIndex = playerCurrentLocation + i;
            if (targetIndex >= totalWaypoints) break;

            GridEffect effect = boardManager.GetGridEffect(targetIndex);
            int value = boardManager.GetGridEffectValue(targetIndex);
            resultText += $"{targetIndex}番マス：{effect}（数値：{value}）\n";
        }

        Debug.Log(resultText);
    }

    // ターン終了時にカード効果をリセットする
    public void ResetTurnCardEffects()
    {
        diceModifier = 0;
        isStayAction = false;
        isGridEffectNullified = false;
    }

}