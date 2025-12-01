using System.Collections;
using UnityEngine;

public class SpecialEnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject specialEnemyPrefab;
    public Transform[] spawnPoints;

    [Header("Cycle")]
    [Tooltip("스페셜 적이 등장하는 간격(초)")]
    public float intervalSeconds = 10f;

    Coroutine _loop;

    void OnEnable()
    {
        if (specialEnemyPrefab == null)
        {
            Debug.LogError("[Spawner] specialEnemyPrefab is NULL!");
            return;
        }
        _loop = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    IEnumerator SpawnLoop()
    {
        var w = new WaitForSeconds(intervalSeconds);
        while (true)
        {
            SpawnOne();
            yield return w;
        }
    }

    // 난이도(적 체력) 상승 시 외부에서 호출
    public void EnemyHealthTierUp()
    {
        Debug.Log("[Spawner] EnemyHealthTierUp() called → spawn one extra.");
        SpawnOne();
    }

    void SpawnOne()
    {
        if (specialEnemyPrefab == null) return;

        Vector3 pos; Quaternion rot;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            pos = p.position; rot = p.rotation;
        }
        else
        {
            pos = transform.position; rot = transform.rotation;
        }

        var enemy = Instantiate(specialEnemyPrefab, pos, rot);
        Debug.Log($"[Spawner] Spawned Special Enemy: {enemy.name} at {pos} (t={Time.time:0.00})");

        // ⚠️ 타깃 주입/리플렉션/SendMessage 전부 제거
        // 특수 적 프리팹에 기존 적과 동일한 AI를 붙여 두면 그 로직이 그대로 동작합니다.
    }
}
