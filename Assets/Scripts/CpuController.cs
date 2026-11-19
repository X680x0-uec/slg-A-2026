using System.Collections;
using UnityEngine;

public class CpuController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private Dice dice;

    private bool isCpuActing = false;
    private bool isInitialized = false;

    void Awake()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (cardManager == null) cardManager = FindAnyObjectByType<CardManager>();
        if (dice == null) dice = FindAnyObjectByType<Dice>();
    }

    public void ApplyHumanPlayerCount()
    {
        if (gameManager == null || gameManager.players == null || gameManager.players.Length == 0) return;

        int humanCount = Mathf.Clamp(TitleManager.HumanPlayerCount, 1, gameManager.players.Length);
        for (int i = 0; i < gameManager.players.Length; i++)
        {
            if (gameManager.players[i] != null)
            {
                gameManager.players[i].isCpu = (i >= humanCount);
            }
        }
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) ApplyHumanPlayerCount();
        if (isCpuActing || gameManager == null) return;

        Player current = gameManager.GetCurrentPlayer();
        if (current == null || !current.isCpu) return;

        if (gameManager.currentPhase == TurnPhase.WaitingForDice && gameManager.IsWaitingForDice)
        {
            StartCoroutine(AutoPlayTurn(current));
        }
    }

    private IEnumerator AutoPlayTurn(Player cpu)
    {
        isCpuActing = true;
        Debug.Log($"[CPU] 1. ターン開始: {cpu.name}");

        yield return new WaitForSeconds(0.5f);
        HideCpuHandCards();

        // 1. ダイス1回目（回転開始）
        if (dice != null && !dice.IsDiceRolling)
        {
            Debug.Log("[CPU] 2. ダイス回転開始");
            dice.RollDice();
        }

        yield return new WaitForSeconds(0.6f);

        // 2. ダイス2回目（出目確定）
        if (dice != null && dice.IsDiceRolling)
        {
            Debug.Log("[CPU] 3. ダイス確定");
            dice.RollDice();
        }

        // CanUseCard が true になるまで最大2秒待機（神経妨害のスキップ等も考慮）
        float timer = 0f;
        while (!gameManager.CanUseCard && timer < 2.0f)
        {
            HideCpuHandCards();
            timer += Time.deltaTime;
            yield return null;
        }

        HideCpuHandCards();
        yield return new WaitForSeconds(0.5f);

        // 3. カード使用
        if (gameManager.CanUseCard && cardManager != null && cardManager.destroyList.Count > 0)
        {
            int idx = Random.Range(0, cardManager.destroyList.Count);
            GameObject cardObj = cardManager.destroyList[idx];
            if (cardObj != null)
            {
                CardScript cs = cardObj.GetComponent<CardScript>();
                if (cs != null)
                {
                    Debug.Log($"[CPU] 4. カード使用実行 (index:{idx})");
                    cs.UseCard();
                }
            }
        }
        else
        {
            Debug.LogWarning($"[CPU] カード使用スキップ: CanUseCard={gameManager.CanUseCard}, 手札数={(cardManager != null ? cardManager.destroyList.Count : -1)}");
        }

        // ターンが変わるかフェーズが変わるまで待機してからロック解除
        while (gameManager.GetCurrentPlayer() == cpu && gameManager.currentPhase == TurnPhase.WaitingForDice)
        {
            yield return null;
        }

        Debug.Log($"[CPU] 5. {cpu.name} の行動完了");
        isCpuActing = false;
    }

    private void HideCpuHandCards()
    {
        if (cardManager == null) return;
        foreach (GameObject c in cardManager.destroyList)
        {
            if (c != null && c.activeSelf) c.SetActive(false);
        }
    }
}