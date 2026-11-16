using UnityEngine;
using TMPro;
using UnityEngine.UI;
using JetBrains.Annotations;

public class Dice : MonoBehaviour
{
    private GameManager gameManager;    // GameManagerオブジェクトを記憶
    [SerializeField] private CardScript cardScript;      // 出目をCardScriptに保存
    [SerializeField] private TextMeshProUGUI diceText;
    [SerializeField] private GameObject diceUiPar;
    [SerializeField] private Sprite[] diceSprites; //サイコロの出目の画像を格納
    private GameObject[] diceUIs; //サイコロのオブジェクト
    private Image[] diceUIImages; //サイコロのImageコンポーネント
    [SerializeField] private bool isDiceRolling = false;
    public bool IsDiceRolling => isDiceRolling;
    
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        int count = diceUiPar.transform.childCount; //サイコロの画像を取得
        diceUIs = new GameObject[count];
        diceUIImages = new Image[count];


        for (int i = 0; i < count; i++)
        {
            diceUIs[i] = diceUiPar.transform.GetChild(i).gameObject; //
            diceUIImages[i] = diceUIs[i].GetComponent<Image>();
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
        // GameManager から現在のプレイヤー（インスタンス）を取得する

        if (!isDiceRolling)
        {
            Debug.Log("もう一度ボタンを押すとサイコロを振ります");
            isDiceRolling = true;
            return;
        }

        isDiceRolling = false;

        Player currentPlayer = gameManager.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Debug.LogError("現在のプレイヤーを取得できませんでした！");
            return;
        }

        int totalValue = 0;

        // 神経妨害(5番)の代償：次のラウンドはダイスを振れず出目0（カード効果だけで移動可能）
        if (currentPlayer.skipDiceNextRound)
        {
            currentPlayer.skipDiceNextRound = false;
            totalValue = 0;
            Debug.Log("【神経妨害の代償】サイコロを振れず、出目合計は 0 です（カードで移動可能）");

            if (diceText != null)
            {
                diceText.text = "→ 0";
            }

            gameManager.OnDiceRolled(totalValue);
            cardScript.saveDiceValue(totalValue);
            return;
        }

        // 強行突破(2番)の代償 または 神経妨害被害中（ただし自分の閲覧バフ中を除く）：サイコロが 1〜2 に制限
        bool isRestricted1To2 = (currentPlayer.restrictDice1To2Rounds > 0) ||
                                (currentPlayer.afflictedByNerveRounds > 0 && currentPlayer.revealAllInfoRounds == 0);

        // 出目改竄(1番)の代償 または 神経妨害被害中：自分の出目が文字化けして見えない
        bool isDiceHidden = currentPlayer.IsInfoHidden(currentPlayer.hideOwnDicePermanent);

        // ★ 文字化け中はサイコロの絵（2個）も隠して数字がバレないようにする
        if (diceUIImages != null && diceUIImages.Length >= 2)
        {
            diceUIImages[0].enabled = !isDiceHidden;
            diceUIImages[1].enabled = !isDiceHidden;
        }

        if (isRestricted1To2)
        {
            int dice1 = Random.Range(1, 3); // 1 または 2
            int dice2 = Random.Range(1, 3); // 1 または 2
            totalValue = dice1 + dice2;

            Debug.Log($"制限ダイス(1〜2): {dice1} + {dice2} = {totalValue}");
            if (diceText != null)
            {
                diceText.text = isDiceHidden ? "→ ■?#@!" : $"→ {totalValue}";
                diceUIImages[0].sprite = diceSprites[dice1 - 1];
                diceUIImages[1].sprite = diceSprites[dice2 - 1];
            }

            gameManager.OnDiceRolled(totalValue);
            cardScript.saveDiceValue(totalValue);
            if (currentPlayer.NextDiceType == DiceType.Special) currentPlayer.NextDiceType = DiceType.Normal;
        }
        else if (currentPlayer.NextDiceType == DiceType.Normal)
        {
            int dice1 = Random.Range(1, 7);
            int dice2 = Random.Range(1, 7);
            totalValue = dice1 + dice2;

            Debug.Log($"通常ダイス: {dice1} + {dice2} = {totalValue}");
            if (diceText != null)
            {
                diceText.text = isDiceHidden ? "→ ■?#@!" : $"→ {totalValue}";
                diceUIImages[0].sprite = diceSprites[dice1 - 1];
                diceUIImages[1].sprite = diceSprites[dice2 - 1];
            }
            
            gameManager.OnDiceRolled(totalValue);
            cardScript.saveDiceValue(totalValue);
        }
        else if (currentPlayer.NextDiceType == DiceType.Special)
        {
            int dice1 = Random.Range(-1, 9);
            int dice2 = Random.Range(-1, 9);
            totalValue = dice1 + dice2;

            Debug.Log($"特殊ダイス: {dice1} + {dice2} = {totalValue}");
            if (diceText != null) 
            {
                diceText.text = isDiceHidden ? "DICE: ■?#@!" : $"DICE: {dice1} + {dice2} = {totalValue}";
            } 
            gameManager.OnDiceRolled(totalValue);
            cardScript.saveDiceValue(totalValue);
            currentPlayer.NextDiceType = DiceType.Normal;
        }     
    }
    public void AppendEffectText(int effectValue) // マス効果の演出用にテキストを追記するメソッド
    {
        if (diceText == null) return;

        // 出目が文字化け中のプレイヤーなら、カード補正値も隠したままにする
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
}