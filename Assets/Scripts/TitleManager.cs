using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // インスペクターから切り替え先のゲームシーン名を指定できるようにする
    [SerializeField] private string gameSceneName = "Game";

    public void OnClickPlayButton()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}