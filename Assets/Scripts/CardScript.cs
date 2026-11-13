using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CardScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CardData cardData;
    private GameManager gameManager;
    private CardManager cardManager;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text effects;

    static int DiceValue;   //Diceから受け取った値

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cardManager = FindAnyObjectByType<CardManager>();
    }
    
    //Diceが振られたときに呼び出され、値が更新される
    public void saveDiceValue(int diceValue)
    {
        DiceValue = diceValue;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager.IsWaitingForDice) return; // サイコロが振られてないとき終了する

        Debug.Log("カードがタップされました");
        Debug.Log(DiceValue);
        gameManager.GetCurrentPlayer().Move(DiceValue);
        //players[gameManager.currentTurn].Move(DiceValue);
        foreach (GameObject card in cardManager.destroyList)
        {
            if (card != null)
            {
                Destroy(card);    //カードを削除
            }
        }
        cardManager.destroyList.Clear();
    }

    //public void OnPointerUp(PointerEventData eventData)
    //{
      //  Debug.Log("カードへのタップを終了しました");
    //}

    public void SetCard(int cardId)
    {
        nameText.text = cardData.CardDataList[cardId].CardName;
    }
}
