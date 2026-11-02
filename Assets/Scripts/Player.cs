using System.Collections; // ★ コルーチンを使うために追加
using UnityEngine;

public class Player : MonoBehaviour
{
    //[SerializeField]private int currentSquareIndex = 0;
    private GameManager gameManager;
    private BoardManager boardManager;

    [SerializeField]int playerCurrentLocation = 0; //プレイヤーの現在位置
    public int PlayerCurrentLocation => playerCurrentLocation; // ★外部（GameManager）から現在位置を取得できるようにプロパティを追加

    [SerializeField] private float moveSpeed = 8f; // ★ なめらかな移動速度（お好みで調整）

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
        boardManager = Object.FindAnyObjectByType<BoardManager>();
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
        }
        else
        {
            Debug.Log($"{gameObject.name}が{spaces}マス分移動");
        }

        // 移動が完全に終わってからGameManagerへ通知
        if (gameManager != null)
        {
            gameManager.OnPlayerMoveComplete();
        }
    }
}
