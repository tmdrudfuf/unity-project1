using UnityEngine;

/// <summary>
/// EnemyHealth의 OnDied 이벤트에 붙어서, "진짜 사망"할 때만 파티클 1번 재생.
/// - 적 오브젝트(EnemyHealth가 있는 곳)에 이 컴포넌트를 붙이고,
///   particlePrefab 슬롯에 파티클 프리팹을 할당하세요.
/// - 외부 Destroy/Despawn로 제거될 땐 재생하지 않습니다(EnemyHealth.Die() 경유만 재생).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeathParticles : MonoBehaviour
{
    [Tooltip("사망 시 재생할 파티클 프리팹 (ParticleSystem 포함 GameObject)")]
    [SerializeField] private GameObject particlePrefab;

    [Tooltip("파티클 오브젝트 자동 파괴 시간(초). 0 이하면 프리팹 설정을 그대로 사용")]
    [SerializeField] private float autoDestroyAfter = 2f;

    private EnemyHealth _health;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        if (_health == null)
            Debug.LogError("[EnemyDeathParticles] EnemyHealth가 없습니다.");
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (particlePrefab == null) return;

        var vfx = Instantiate(particlePrefab, transform.position, transform.rotation);

        if (autoDestroyAfter > 0f)
            Destroy(vfx, autoDestroyAfter);
        // 파티클 프리팹의 ParticleSystem → Stop Action=Destroy 로 두면 autoDestroyAfter 없이도 자동 정리됩니다.
    }
}
