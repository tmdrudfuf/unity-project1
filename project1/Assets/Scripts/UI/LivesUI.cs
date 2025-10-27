// Assets/Scripts/UI/LivesUI.cs
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class LivesUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerLives playerLives;
    [SerializeField] private TextMeshProUGUI livesText;

    [Header("Style")]
    [Tooltip("하트로 표시하면 true, 숫자로 표시하면 false")]
    [SerializeField] private bool useHearts = true;
    [Tooltip("하트 문자")]
    [SerializeField] private string heartChar = "♥";

    private void Reset()
    {
        // ✅ FindObjectOfType 사용 금지(Deprecated) → 새 API로 대체
        if (!playerLives)
        {
#if UNITY_2023_1_OR_NEWER
            playerLives = Object.FindFirstObjectByType<PlayerLives>();
#else
            // 2023 이전 버전 호환 (경고 피하려면 직접 할당 권장)
            playerLives = Object.FindObjectOfType<PlayerLives>();
#endif
        }

        if (!livesText)
        {
            livesText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        if (playerLives != null)
        {
            playerLives.onLivesChanged.AddListener(HandleLivesChanged);
            HandleLivesChanged(playerLives.CurrentLives, 3);
        }
    }

    private void OnDisable()
    {
        if (playerLives != null)
        {
            playerLives.onLivesChanged.RemoveListener(HandleLivesChanged);
        }
    }

    private void HandleLivesChanged(int current, int max)
    {
        if (!livesText) return;

        if (useHearts)
        {
            livesText.text = new string(heartChar[0], Mathf.Max(0, current));
        }
        else
        {
            livesText.text = $"Lives: {current}";
        }
    }
}
