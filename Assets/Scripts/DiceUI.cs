using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour
{
    [SerializeField] public Dice diceManager;
    [SerializeField] private GameManager gameManager;
    // 0:1, 1:2, 2:3, 3:4, 4:5, 5:6, 6:7, 7:8, 8:-1, 9:?
    [SerializeField] private Sprite[] diceSprites;
    private UnityEngine.UI.Image diceImage;

    [SerializeField]private DiceType currentDiceType;

    private int diceAnimeFlame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diceImage = GetComponent<Image>();
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();

        // 初期表示で真っ白にならないように「1」の目をセットしておく
        if (diceImage != null && diceSprites != null && diceSprites.Length > 0)
        {
            diceImage.sprite = diceSprites[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (diceManager == null || diceImage == null || diceSprites == null || diceSprites.Length < 10) return;

        // 代償（出目改竄 or 神経妨害）で出目が隠されているか判定
        bool isHidden = false;
        if (gameManager != null)
        {
            Player p = gameManager.GetCurrentPlayer();
            if (p != null)
            {
                isHidden = p.IsInfoHidden(p.hideOwnDicePermanent) || p.isCpu;
                currentDiceType = p.NextDiceType; // 現在のプレイヤーのダイス種類に自動同期
            }
        }
        // 隠されている間は常に「?」（インデックス9）を表示
        if (isHidden)
        {
            diceImage.enabled = true;
            diceImage.sprite = diceSprites[9];
            return;
        }

        if(diceManager.IsDiceRolling)
        {
            diceAnimeFlame = (diceAnimeFlame + 1) % 2;
            if(diceAnimeFlame != 0)
            {
                if(currentDiceType == DiceType.Normal)
                {
                    diceImage.sprite = diceSprites[Random.Range(0,6)];
                }
                else if(currentDiceType == DiceType.Special)
                {
                    diceImage.sprite = diceSprites[Random.Range(0,9)];
                }
                
            }
                
        }
        
    }
    public void ChangeDiceType(DiceType changeTo)
    {
        currentDiceType = changeTo;
    }
}
