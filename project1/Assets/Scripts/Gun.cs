using System.Collections;
using UnityEngine;

/// <summary>
/// 총 : 자동 발사 + 탄창/장전 + 업그레이드(연사/장전속도) 적용
///  - 외부에서 ApplyUpgrade(UpgradeType, levels)로 업그레이드 반영
///  - 파워업(장전속도↑): UpgradeType.ReloadSpeed
///  - 스피드업(연사속도↑): UpgradeType.FireRate
/// </summary>
public enum UpgradeType { ReloadSpeed, FireRate }

public class Gun : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;          // 총구
    public GameObject bulletPrefab;      // 탄 프리팹(리짓바디 포함 권장)

    [Header("Base Stats")]
    [Tooltip("기본 초당 발사 횟수")] public float baseFireRate = 8f;
    [Tooltip("탄창 용량")] public int magazineSize = 12;
    [Tooltip("기본 장전 시간(초)")] public float baseReloadSeconds = 1.6f;

    [Header("Bullet Motion")]
    [Tooltip("발사 시 초기 속도(없으면 프리팹 자체 로직 사용)")]
    public float initialBulletSpeed = 0f;

    [Header("State (debug)")]
    [SerializeField] int _currentAmmo;
    [SerializeField] bool _isReloading;
    [SerializeField] float _nextFireTime;

    // ── 업그레이드 누적 레벨
    int _reloadLevel = 0;
    int _fireRateLevel = 0;

    // ── 레벨당 보정치
    const float ReloadTimePerLevelMul = 0.85f; // 장전시간 ×0.85
    const float FireRatePerLevelMul   = 1.15f; // 연사속도 ×1.15

    void Awake()
    {
        _currentAmmo = magazineSize;
    }

    void Update()
    {
        if (_isReloading) return;

        // 자동 사격 타이밍
        if (Time.time >= _nextFireTime)
        {
            float rate = GetCurrentFireRate();
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, rate);
            TryShoot();
        }
    }

    // ─────────────────────────────────────────────────────────

    void TryShoot()
    {
        if (!firePoint || !bulletPrefab) return;

        if (_currentAmmo <= 0)
        {
            // 자동 장전
            StartCoroutine(ReloadRoutine());
            return;
        }

        // 탄 1발 발사
        var go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (initialBulletSpeed > 0f && go.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = firePoint.forward * initialBulletSpeed;
        }

        _currentAmmo--;
        if (_currentAmmo <= 0)
            StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        yield return new WaitForSeconds(GetCurrentReloadSeconds());
        _currentAmmo = magazineSize;
        _isReloading = false;
    }

    // ── 업그레이드 API (픽업/드랍 등 외부에서 호출)
    public void ApplyUpgrade(UpgradeType type, int levels = 1)
    {
        if (levels <= 0) return;

        switch (type)
        {
            case UpgradeType.ReloadSpeed:
                _reloadLevel += levels;
                break;
            case UpgradeType.FireRate:
                _fireRateLevel += levels;
                break;
        }
        Debug.Log($"[Gun Upgrade] {type}+{levels}  ⇒ RLD Lv:{_reloadLevel}, FR Lv:{_fireRateLevel}");
    }

    // ── 현재 수치 계산
    float GetCurrentReloadSeconds()
    {
        float t = baseReloadSeconds;
        for (int i = 0; i < _reloadLevel; i++) t *= ReloadTimePerLevelMul;
        return Mathf.Max(0.1f, t);
    }

    float GetCurrentFireRate()
    {
        float r = baseFireRate;
        for (int i = 0; i < _fireRateLevel; i++) r *= FireRatePerLevelMul;
        return r;
    }
}
