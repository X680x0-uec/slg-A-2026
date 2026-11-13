using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CardData cardData;
    [SerializeField] private GameManager gameManager;

    static int DiceValue;   //Diceから受け取った値

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
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
    }

    //public void OnPointerUp(PointerEventData eventData)
    //{
      //  Debug.Log("カードへのタップを終了しました");
    //}
}
