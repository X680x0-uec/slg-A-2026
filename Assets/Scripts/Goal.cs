using UnityEngine;
using TMPro;

public class Goal : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] BoardManager boardManager;

    [SerializeField] private GameObject GoalPanel;
    [SerializeField] private TMP_Text message0;
    [SerializeField] private TMP_Text message1;
    [SerializeField] private TMP_Text message2;

    
    private bool[] GoalPlayer;  //ゴールしたかどうか

    void Start()
    {
        GoalPlayer = new bool[gameManager.players.Length];
        for (int i = 0; i < GoalPlayer.Length; i++)
        {
            GoalPlayer[i] = false;
        }
        GoalPanel.SetActive(false);

    }

    public void GoalJudge()
    {
        int totalWaypoints = boardManager.GetTotalWaypoints(); // 全体のマス数を取得

        for (int i = 0; i < GoalPlayer.Length; i++)
        {
            //現在ターンのプレイヤーの位置を取得
            int currentLocation = gameManager.players[i].PlayerCurrentLocation;

            //ゴールしてたらtrue
            if (totalWaypoints - 1 <= currentLocation)
            {
                GoalPlayer[i] = true;
                gameManager.currentPhase = TurnPhase.Goal;
            }
            else
            {
                GoalPlayer[i] = false;
            }
            Debug.Log($"totalWaypoints:{totalWaypoints}\ncurrentLocation:{currentLocation}");
        }

        if (gameManager.currentPhase == TurnPhase.Goal)
        {
            Debug.Log("ゴーーーーーーーーール");
            GoalPanel.SetActive(true);
            if (GoalPlayer[0])
            {
                message0.text = $"プレイヤー 1 の勝利!!!!";
            }
            if (GoalPlayer[1])
            {
                message1.text = $"プレイヤー 2 の勝利!!!!";
            }
            if (GoalPlayer[2])
            {
                message2.text = $"プレイヤー 3 の勝利!!!!";
            }
            
        }
    }
}
