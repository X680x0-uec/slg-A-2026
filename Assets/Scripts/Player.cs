using UnityEngine;

public class Player : MonoBehaviour
{
    //[SerializeField]private int currentSquareIndex = 0;
    private GameManager gameManager;
    private BoardManager boardManager;

    [SerializeField]int playerCurrentLocation = 0; //プレイヤーの現在位置
    public int PlayerCurrentLocation => playerCurrentLocation; // ★外部（GameManager）から現在位置を取得できるようにプロパティを追加

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
        //float moveDistance = spaceWidth * spaces;
        Vector3 currentPosition = transform.position;

        playerCurrentLocation += spaces;

        int totalWaypoints = boardManager.GetTotalWaypoints();
        if (playerCurrentLocation >= totalWaypoints - 1) // ゴール（最後のマス = totalWaypoints - 1）に到達または超えた場合
        {
            playerCurrentLocation = totalWaypoints - 1; // 位置をゴールのマス（49番目）に固定
            transform.position = boardManager.GetWaypointPosition(playerCurrentLocation);
            Debug.Log("ゴール！");
        }
        else
        {
            transform.position = boardManager.GetWaypointPosition(playerCurrentLocation);
            Debug.Log($"{gameObject.name}が{spaces}マス分移動");
        }
        gameManager.OnPlayerMoveComplete();
    }


}
