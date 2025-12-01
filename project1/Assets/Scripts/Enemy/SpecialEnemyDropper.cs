using UnityEngine;

public class SpecialEnemyDropper : MonoBehaviour
{
    [Header("Pickup Prefab (must have UpgradePickup)")]
    public GameObject upgradePickupPrefab;

    [Header("Drop")]
    [Range(0f,1f)] public float dropChance = 1f;

    private bool _hasDropped;

    public void OnDead()
    {
        if (_hasDropped) { Debug.Log("[Dropper] Already dropped. Skipping."); return; }

        if (upgradePickupPrefab == null)
        {
            Debug.LogError($"[Dropper] {name} upgradePickupPrefab is NULL. Cannot drop!");
            return;
        }

        _hasDropped = true;

        if (Random.value > dropChance)
        {
            Debug.Log($"[Dropper] {name} rolled no-drop (chance={dropChance}).");
            return;
        }

        var go = Instantiate(upgradePickupPrefab, transform.position, Quaternion.identity);
        var up = go.GetComponent<UpgradePickup>();
        if (up == null)
        {
            Debug.LogError("[Dropper] The assigned prefab has NO UpgradePickup component!");
        }
        else
        {
            // 50:50 랜덤
            var type = (Random.value < 0.5f) ? UpgradeType.ReloadSpeed : UpgradeType.FireRate;
            up.upgradeType = type;
            up.levels = 1;
            Debug.Log($"[Dropper] Dropped {type} at {transform.position} (t={Time.time:0.00})");
        }
    }
}
