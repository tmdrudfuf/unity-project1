using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class UpgradePickup : MonoBehaviour
{
    public UpgradeType upgradeType = UpgradeType.ReloadSpeed;
    public int levels = 1;
    public float autoDestroyAfter = 20f;

    [Header("Filter")]
    [Tooltip("플레이어 식별에 사용할 태그")]
    public string playerTag = "Player";   // ← 플레이어 오브젝트에 Player 태그 달아두세요

    void Start()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        if (autoDestroyAfter > 0) Destroy(gameObject, autoDestroyAfter);
        Debug.Log($"[Pickup] Spawned {upgradeType}+{levels} (will autodestruct={autoDestroyAfter}s)");
    }

    void OnTriggerEnter(Collider other)
    {
        // ✅ 플레이어만 통과
        if (!other.CompareTag(playerTag))
        {
            // 디버그 용도로만 남김: 총알 등 불필요 충돌 필터링 확인
            Debug.Log($"[Pickup] Triggered by {other.name} (tag={other.tag}) — ignored.");
            return;
        }

        var gun = other.GetComponentInParent<Gun>() ?? other.GetComponent<Gun>();
        if (gun != null)
        {
            gun.ApplyUpgrade(upgradeType, levels);
            Debug.Log($"[Pickup] Collected by {gun.name} → {upgradeType}+{levels}. Destroy self.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[Pickup] Player hit but no Gun found in hierarchy on {other.name}.");
        }
    }
}
