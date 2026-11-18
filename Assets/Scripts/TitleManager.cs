using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    private static bool hasBootedFromTitle = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureStartFromTitle()
    {
        if (hasBootedFromTitle) return;
        hasBootedFromTitle = true;

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "TitleScene")
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    // ゲームシーンへ引き継ぐ人間プレイヤー人数（1〜3人）
    public static int HumanPlayerCount = 1;

    // 「1人プレイ（CPU 2人）」ボタン用
    public void OnClick1Player()
    {
        HumanPlayerCount = 1;
        SceneManager.LoadScene(gameSceneName);
    }

    // 「2人プレイ（CPU 1人）」ボタン用
    public void OnClick2Players()
    {
        HumanPlayerCount = 2;
        SceneManager.LoadScene(gameSceneName);
    }

    // 「3人プレイ（CPU 0人）」ボタン用
    public void OnClick3Players()
    {
        HumanPlayerCount = 3;
        SceneManager.LoadScene(gameSceneName);
    }
}