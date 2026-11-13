using System.Runtime.CompilerServices;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour
{
    [SerializeField] public Dice diceManager;
    
    [SerializeField] private Sprite[] diceSprites;
    private UnityEngine.UI.Image diceImage;

    private int diceAnimeFlame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diceImage = GetComponent<UnityEngine.UI.Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(diceManager.IsDiceRolling)
        {
            diceAnimeFlame = (diceAnimeFlame + 1) % 2;
            if(diceAnimeFlame != 0)
            {
                diceImage.sprite = diceSprites[Random.Range(0,6)];
            }
                
        }
    }
}
