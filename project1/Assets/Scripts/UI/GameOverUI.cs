// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameOverUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerLives playerLives;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private bool shown;

    private void Reset()
    {
        if (!playerLives)
        {
#if UNITY_2023_1_OR_NEWER
            playerLives = Object.FindFirstObjectByType<PlayerLives>();
#else
            playerLives = Object.FindObjectOfType<PlayerLives>();
#endif
        }
    }

    private void Awake()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (retryButton) retryButton.onClick.AddListener(ReloadScene);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);
    }

    private void OnEnable()
    {
        if (playerLives) playerLives.onGameOver.AddListener(ShowGameOver);
    }

    private void OnDisable()
    {
        if (playerLives) playerLives.onGameOver.RemoveListener(ShowGameOver);
    }

    private void ShowGameOver()
    {
        if (shown) return;
        shown = true;

        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
