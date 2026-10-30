using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [SerializeField] private GameManager gameManager; // GameManagerを参照
    [SerializeField] private float smoothSpeed = 10f; // 追従のなめらかさ（大きいほどピッタリ追う）

    void LateUpdate()
    {
        if (gameManager == null) return;

        // 現在のターンのプレイヤーを取得
        Player targetPlayer = gameManager.GetCurrentPlayer();

        if (targetPlayer != null)
        {
            // プレイヤーの位置を取得
            Vector3 targetPosition = targetPlayer.transform.position;

            // 2Dなので Z 軸（カメラの奥行き位置）は固定したまま、X軸・Y軸だけ追従させる
            targetPosition.z = transform.position.z;

            // プレイヤーの位置へ滑らかに移動
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }
}
