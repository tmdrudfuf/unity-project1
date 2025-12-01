// Assets/Scripts/DifficultyManager.cs
using UnityEngine;
using System;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Scaling")]
    [Tooltip("몇 초마다 난이도(체력 보너스)를 올릴지")]
    [SerializeField] private float tickSeconds = 10f;

    [Tooltip("틱마다 적 체력에 더해질 양")]
    [SerializeField] private int hpIncrementPerTick = 3;

    [Header("Affects Existing Enemies")]
    [Tooltip("틱 발생 시, 이미 살아있는 적들에게도 체력 증가를 적용할지")]
    [SerializeField] private bool applyToExistingEnemies = true;

    [Tooltip("기존 적에게 적용할 때, 최대체력만 늘릴지(true) / 최대+현재를 함께 늘릴지(false)")]
    [SerializeField] private bool increaseMaxOnlyForExisting = false;

    public int CurrentBonusHP { get; private set; } = 0;

    /// <summary>
    /// 난이도 틱 발생 알림: (증가한 HP량, 기존적에게도 적용하는지, 기존적에겐 Max만 늘릴지)
    /// </summary>
    public event Action<int, bool, bool> OnDifficultyTick;

    private float timer;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // 이 오브젝트는 씬 전환 시 살아있고 싶다면 주석 해제
        // DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime; // 일시정지와 무관하게 진행하려면 unscaled 사용
        if (timer >= tickSeconds)
        {
            timer -= tickSeconds;
            CurrentBonusHP += hpIncrementPerTick;

            // 이벤트 브로드캐스트: 기존 적에게도 적용할지 옵션 포함
            OnDifficultyTick?.Invoke(hpIncrementPerTick, applyToExistingEnemies, increaseMaxOnlyForExisting);
        }
    }

    // 디버그용 수동 트리거 (원하면 사용)
    [ContextMenu("Force Tick")]
    private void ForceTick()
    {
        CurrentBonusHP += hpIncrementPerTick;
        OnDifficultyTick?.Invoke(hpIncrementPerTick, applyToExistingEnemies, increaseMaxOnlyForExisting);
    }
}
