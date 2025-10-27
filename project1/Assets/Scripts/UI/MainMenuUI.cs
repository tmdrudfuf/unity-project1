// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Options")]
    [Tooltip("비워두면 BuildIndex+1 로드")]
    [SerializeField] private string gameSceneName = "";

    [Header("Press Any Key 모드")]
    [SerializeField] private bool enablePressAnyKey = false;
    [SerializeField] private TextMeshProUGUI pressAnyKeyText;

    private bool waitingAnyKey;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (startButton) startButton.onClick.AddListener(StartGame);
        if (quitButton)  quitButton.onClick.AddListener(SceneLoader.Quit);

        if (enablePressAnyKey)
        {
            waitingAnyKey = true;
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(true);
            if (startButton) startButton.gameObject.SetActive(false);
            if (quitButton)  quitButton.gameObject.SetActive(false);
        }
        else
        {
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!enablePressAnyKey || !waitingAnyKey) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            waitingAnyKey = false;
            StartGame();
        }

        if (pressAnyKeyText)
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
            pressAnyKeyText.alpha = Mathf.Lerp(0.3f, 1f, a);
        }
    }

    private void StartGame()
    {
        if (!string.IsNullOrWhiteSpace(gameSceneName))
            SceneLoader.LoadByName(gameSceneName);
        else
            SceneLoader.LoadNext();
    }
}
