// Assets/Scripts/Systems/ScoreManager.cs
using System;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 필요하면 씬 전환 유지:
        // DontDestroyOnLoad(gameObject);
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetScore(int startValue = 0)
    {
        Score = Mathf.Max(0, startValue);
        OnScoreChanged?.Invoke(Score);
    }

    public void Add(int delta)
    {
        if (delta == 0) return;
        Score = Mathf.Max(0, Score + delta);
        OnScoreChanged?.Invoke(Score);
    }
}
