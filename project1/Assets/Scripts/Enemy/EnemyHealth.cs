// Assets/Scripts/Enemies/EnemyHealth.cs
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP & Score")]
    [SerializeField] private int maxHP = 5;     // ← 여러 번 맞아야 죽도록
    [SerializeField] private int scoreOnKill = 100;

    [Header("Hit Feedback")]
    [SerializeField] private bool briefInvincible = true;
    [SerializeField] private float invincibleSeconds = 0.05f; // 다중 히트 보호
    private float invincibleUntil;

    private int currentHP;
    private bool isDead;

    public int CurrentHP => currentHP;
    public int MaxHP     => maxHP;

    // 체력바가 구독할 수 있게 이벤트 제공
    public System.Action<int,int> OnHealthChanged; // (current, max)
    public System.Action OnDied;

    private void Awake()
    {
        currentHP = Mathf.Max(1, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int dmg)
    {
        Debug.Log($"HIT {name}: {currentHP}/{maxHP} -> {currentHP - dmg}");
        if (isDead) return;
        if (briefInvincible && Time.time < invincibleUntil) return;

        currentHP -= Mathf.Max(1, dmg);
        invincibleUntil = Time.time + invincibleSeconds;

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(maxHP, currentHP + Mathf.Max(1, amount));
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 점수 추가
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.Add(scoreOnKill);

        OnDied?.Invoke();
        Destroy(gameObject);
    }

    // 편하게 에디터에서 값 바꾸면 즉시 반영되게
#if UNITY_EDITOR
    private void OnValidate()
    {
        maxHP = Mathf.Max(1, maxHP);
    }
#endif
}
