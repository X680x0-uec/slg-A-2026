using UnityEngine;

public class Player : MonoBehaviour
{
    //[SerializeField]private int currentSquareIndex = 0;
    private GameManager gameManager;
    float spaceWidth = 2.0f; //1マス分の移動距離

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    public void Move(int spaces)
    {
        float moveDistance = spaceWidth * spaces;

        Vector3 currentPosition = transform.position;

        currentPosition.x += moveDistance;

        transform.position = currentPosition;

        Debug.Log($"{gameObject.name}が{spaces}マス分移動");

        gameManager.OnPlayerMoveComplete();
    }


}