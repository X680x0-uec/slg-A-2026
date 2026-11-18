using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    private GameManager gameManager;    // GameManagerオブジェクトを記憶
    [SerializeField] private CardScript cardScript;      // 出目をCardScriptに保存
    [SerializeField] private TextMeshProUGUI diceText;
    [SerializeField] private GameObject diceUiPar;
    [SerializeField] private Sprite[] diceSprites; // 0:1, 1:2, 2:3, 3:4, 4:5, 5:6, 6:7, 7:8, 8:-1, 9:?
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rollSE;      // 効果音用
    private GameObject[] diceUIs; // サイコロのオブジェクト
    private Image[] diceUIImages; // サイコロのImageコンポーネント
    [SerializeField] private bool isDiceRolling = false;
    public bool IsDiceRolling => isDiceRolling;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (diceUiPar != null)
        {
            int count = diceUiPar.transform.childCount;
            diceUIs = new GameObject[count];
            diceUIImages = new Image[count];

            for (int i = 0; i < count; i++)
            {
                diceUIs[i] = diceUiPar.transform.GetChild(i).gameObject;
                diceUIImages[i] = diceUIs[i].GetComponent<Image>();
            }
        }
    }

    public void RollDice()
    {
        // GameManagerがダイス待ち状態（isWaitingForDice == true）でない時は一切受け付けない
        if (gameManager != null && !gameManager.IsWaitingForDice)
        {
            Debug.Log("コマが移動中またはカードが未選択のため、ダイスは振れません");
            return;
        }

        Player currentPlayer = gameManager.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Debug.LogError("現在のプレイヤーを取得できませんでした！");
            return;
        }

        if (diceUIs != null && diceUIs.Length >= 2)
        {
            DiceUI ui0 = diceUIs[0].GetComponent<DiceUI>();
            DiceUI ui1 = diceUIs[1].GetComponent<DiceUI>();
            if (ui0 != null) ui0.ChangeDiceType(currentPlayer.NextDiceType);
            if (ui1 != null) ui1.ChangeDiceType(currentPlayer.NextDiceType);
        }

        if (!isDiceRolling)
        {
            Debug.Log("もう一度ボタンを押すとサイコロを振ります");
            isDiceRolling = true;
            return;
        }

        isDiceRolling = false;

        if (audioSource != null && rollSE != null)
        {
            audioSource.PlayOneShot(rollSE);
        }

        // 神経妨害(5番)の代償：次のラウンドはダイスを振れず出目0（カード効果だけで移動可能）
        if (currentPlayer.skipDiceNextRound)
        {
            currentPlayer.skipDiceNextRound = false;
            Debug.Log("【神経妨害の代償】サイコロを振れず、出目合計は 0 です");
            if (diceText != null) diceText.text = "→ 0";
            gameManager.OnDiceRolled(0);
            if (cardScript != null) cardScript.saveDiceValue(0);
            return;
        }

        // 強行突破(2番)の代償 または 神経妨害被害中：サイコロが 1〜2 に制限
        bool isRestricted1To2 = (currentPlayer.restrictDice1To2Rounds > 0) ||
                                (currentPlayer.afflictedByNerveRounds > 0 && currentPlayer.revealAllInfoRounds == 0);

        // 出目改竄(1番)の代償 または 神経妨害被害中：自分の出目が「?」になって見えない
        bool isDiceHidden = currentPlayer.IsInfoHidden(currentPlayer.hideOwnDicePermanent);

        int dice1 = 1;
        int dice2 = 1;
        int totalValue = 0;

        if (isRestricted1To2)
        {
            dice1 = Random.Range(1, 3); // 1 または 2
            dice2 = Random.Range(1, 3); // 1 または 2
            totalValue = dice1 + dice2;
            Debug.Log($"制限ダイス(1〜2): {dice1} + {dice2} = {totalValue}");
            if (currentPlayer.NextDiceType == DiceType.Special) currentPlayer.NextDiceType = DiceType.Normal;
        }
        else if (currentPlayer.NextDiceType == DiceType.Normal)
        {
            dice1 = Random.Range(1, 7); // 1〜6
            dice2 = Random.Range(1, 7); // 1〜6
            totalValue = dice1 + dice2;
            Debug.Log($"通常ダイス: {dice1} + {dice2} = {totalValue}");
        }
        else if (currentPlayer.NextDiceType == DiceType.Special)
        {
            dice1 = Random.Range(-1, 9); // -1〜8
            dice2 = Random.Range(-1, 9); // -1〜8
            totalValue = dice1 + dice2;
            Debug.Log($"特殊ダイス: {dice1} + {dice2} = {totalValue}");
            currentPlayer.NextDiceType = DiceType.Normal; // 通常に戻す
        }

        // テキストとサイコロ画像（2個）の更新
        if (diceText != null)
        {
            if (isDiceHidden)
            {
                diceText.text = "→ ■?#@!";
            }
            else if (dice1 < 0 || dice1 > 6 || dice2 < 0 || dice2 > 6)
            {
                diceText.text = $"DICE: {dice1} + {dice2} = {totalValue}";
            }
            else
            {
                diceText.text = $"→ {totalValue}";
            }
        }

        if (diceUIImages != null && diceUIImages.Length >= 2 && diceSprites != null && diceSprites.Length >= 10)
        {
            diceUIImages[0].enabled = true;
            diceUIImages[1].enabled = true;

            if (isDiceHidden)
            {
                diceUIImages[0].sprite = diceSprites[9]; // 「?」
                diceUIImages[1].sprite = diceSprites[9]; // 「?」
            }
            else
            {
                diceUIImages[0].sprite = diceSprites[GetSpriteIndexForRoll(dice1)];
                diceUIImages[1].sprite = diceSprites[GetSpriteIndexForRoll(dice2)];
            }
        }

        gameManager.OnDiceRolled(totalValue);
        if (cardScript != null) cardScript.saveDiceValue(totalValue);

        // 指摘失敗の−４などをテキストに反映（隠蔽中はAppendEffectText内で自動ブロック）
        if (currentPlayer.diceModifier != 0)
        {
            AppendEffectText(currentPlayer.diceModifier);
        }
    }

    public void AppendEffectText(int effectValue) // マス効果・カード補正の演出用にテキストを追記
    {
        if (diceText == null) return;

        if (gameManager != null)
        {
            Player p = gameManager.GetCurrentPlayer();
            if (p != null && p.IsInfoHidden(p.hideOwnDicePermanent))
            {
                diceText.text = "→ ■?#@!";
                return;
            }
        }

        if (effectValue > 0)
        {
            diceText.text += $" <color=green>+{effectValue}</color>";
        }
        else if (effectValue < 0)
        {
            diceText.text += $" <color=red>{effectValue}</color>";
        }
    }

    // ターン開始・交代時に前のプレイヤーの出目を隠す
    public void RefreshDiceResult()
    {
        isDiceRolling = false;

        if (diceText != null)
        {
            diceText.text = "→";
        }

        if (diceUIImages != null && diceUIImages.Length >= 2 && diceSprites != null && diceSprites.Length > 0)
        {
            int qIndex = Mathf.Min(9, diceSprites.Length - 1); // 「?」のスプライト
            diceUIImages[0].enabled = true;
            diceUIImages[1].enabled = true;
            diceUIImages[0].sprite = diceSprites[qIndex];
            diceUIImages[1].sprite = diceSprites[qIndex];
        }
    }

    // 出目（-1〜8）に対応するスプライトインデックスを返す
    // 0:1, 1:2, 2:3, 3:4, 4:5, 5:6, 6:7, 7:8, 8:-1, 9:?
    private int GetSpriteIndexForRoll(int rollValue)
    {
        if (rollValue >= 1 && rollValue <= 8) return rollValue - 1; // 1→0, ..., 8→7
        if (rollValue == -1) return 8;                             // -1→8
        return 9;                                                  // 0は「?」（またはシートに0画像を追加したらその番号）
    }

    
}