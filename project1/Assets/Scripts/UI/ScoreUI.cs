// Assets/Scripts/UI/ScoreUI.cs
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ScoreUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Format")]
    [SerializeField] private bool useThousandsSeparator = true;

    private void Reset()
    {
        if (!scoreText) scoreText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (!scoreText) scoreText = GetComponent<TextMeshProUGUI>();
        if (scoreText) scoreText.textWrappingMode = TextWrappingModes.NoWrap;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            // 초기 값 표시
            HandleScoreChanged(ScoreManager.Instance.Score);
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int score)
    {
        if (!scoreText) return;
        scoreText.text = useThousandsSeparator ? $"Score: {score:N0}" : $"Score: {score}";
    }
}
