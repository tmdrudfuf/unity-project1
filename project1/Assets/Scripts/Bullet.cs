using UnityEngine;

/// <summary>
/// 총알: 적에게 피해만 주고, 절대 적을 직접 Destroy하지 않는다.
/// - EnemyHealth.TakeDamage() 호출 후 자신(Bullet)만 파괴
/// - lifeSeconds가 지나면 자동 파괴
/// - 트리거/콜리전 둘 다 지원(프로젝트 셋업에 따라 한 쪽만 호출됨)
/// </summary>
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 3;

    [Header("Lifetime")]
    [SerializeField] private float lifeSeconds = 4f;

    [Header("Hit Filter")]
    [Tooltip("맞을 수 있는 레이어(비워두면 전부 허용)")]
    [SerializeField] private LayerMask hittableLayers = ~0;

    private void Start()
    {
        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    private void OnTriggerEnter(Collider other)  { TryHit(other.gameObject); }
    private void OnCollisionEnter(Collision col) { TryHit(col.collider.gameObject); }

    private void TryHit(GameObject other)
    {
        // 레이어 필터
        if (((1 << other.layer) & hittableLayers) == 0) return;

        // 적 체력 찾기(자식/부모 구조 모두 커버)
        var eh = other.GetComponentInParent<EnemyHealth>() ?? other.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage(Mathf.Max(1, damage));  // ✅ 처치는 EnemyHealth가 담당
            Destroy(gameObject);                  // ✅ 총알만 제거
            return;
        }

        // 적이 아니면(바닥/벽 등) 옵션에 따라 총알 제거
        Destroy(gameObject);
    }
}
