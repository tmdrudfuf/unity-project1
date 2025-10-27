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
    [Tooltip("시작 시 로드할 게임 씬 이름 (비워두면 BuildIndex+1 로드)")]
    [SerializeField] private string gameSceneName = "";

    [Header("Press Any Key 모드")]
    [SerializeField] private bool enablePressAnyKey = false;
    [SerializeField] private TextMeshProUGUI pressAnyKeyText;

    private bool canStart;

    private void Awake()
    {
        Time.timeScale = 1f; // 메뉴 진입 시 항상 정상 속도
        if (startButton) startButton.onClick.AddListener(StartGame);
        if (quitButton)  quitButton.onClick.AddListener(SceneLoader.Quit);

        if (enablePressAnyKey)
        {
            if (pressAnyKeyText)
            {
                pressAnyKeyText.gameObject.SetActive(true);
                // 깜빡임 효과(간단)
                pressAnyKeyText.alpha = 1f;
            }
            if (startButton) startButton.gameObject.SetActive(false);
            if (quitButton)  quitButton.gameObject.SetActive(false);
            canStart = true;
        }
        else
        {
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!enablePressAnyKey || !canStart) return;

        // 키/클릭/터치 아무 거나
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            canStart = false;
            StartGame();
        }

        // 텍스트 깜빡임(선택)
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
