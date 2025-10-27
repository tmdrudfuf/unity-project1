using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class PlayerLives : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    public int CurrentLives { get; private set; }

    [Header("Damage Sources (Tags)")]
    [Tooltip("이 태그를 가진 오브젝트와 부딪히면 1 감소")]
    [SerializeField] private string[] damageSourceTags = { "Enemy", "EnemyProjectile" };

    [Header("On Hit Behavior")]
    [Tooltip("맞으면 상대 오브젝트를 파괴할지 여부")]
    [SerializeField] private bool destroyOtherOnHit = true;
    [Tooltip("자식 콜라이더에 부딪혀도 전체 적(루트)을 파괴하려면 체크")]
    [SerializeField] private bool destroyUseRootObject = true;

    [Header("Events")]
    public UnityEvent<int, int> onLivesChanged; // (current, max)
    public UnityEvent onGameOver;

    private bool isDead;

    private void Awake()
    {
        CurrentLives = Mathf.Max(1, maxLives);
        onLivesChanged?.Invoke(CurrentLives, maxLives);
    }

    // ───── 충돌/트리거 모두 지원 (둘 중 하나만 써도 됨) ─────
    private void OnCollisionEnter(Collision other)
    {
        TryHandleHit(other.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHandleHit(other.gameObject);
    }
    // ────────────────────────────────────────────────────────

    private void TryHandleHit(GameObject otherGO)
    {
        if (isDead || otherGO == null) return;
        if (!IsDamageSource(otherGO)) return;

        // 라이프 감소
        TakeHit(1);

        // 상대 파괴
        if (destroyOtherOnHit)
        {
            var target = destroyUseRootObject ? otherGO.transform.root.gameObject : otherGO;
            if (target != null) Destroy(target);
        }
    }

    private bool IsDamageSource(GameObject obj)
    {
        // CompareTag가 가장 안전/빠름
        for (int i = 0; i < damageSourceTags.Length; i++)
        {
            var tagName = damageSourceTags[i];
            if (!string.IsNullOrEmpty(tagName) && obj.CompareTag(tagName))
                return true;
        }
        return false;
    }

    public void TakeHit(int damage)
    {
        if (isDead) return;
        CurrentLives = Mathf.Max(0, CurrentLives - Mathf.Max(1, damage));
        onLivesChanged?.Invoke(CurrentLives, maxLives);

        if (CurrentLives <= 0)
        {
            isDead = true;
            onGameOver?.Invoke();
        }
    }

    // 회복이 필요하면 사용
    public void Heal(int amount)
    {
        if (isDead) return;
        CurrentLives = Mathf.Clamp(CurrentLives + Mathf.Max(1, amount), 0, maxLives);
        onLivesChanged?.Invoke(CurrentLives, maxLives);
    }
}
