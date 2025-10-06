using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnRate = 1.2f;
    public float fixedY = 1f;
    public float margin = 0.05f;

    float nextTime;

    void Update()
    {
        if (!enemyPrefab || !player) return;

        if (Time.time >= nextTime)
        {
            nextTime = Time.time + 1f / spawnRate;
            SpawnFromTop();
        }
    }

    void SpawnFromTop()
    {
        var cam = Camera.main;
        if (!cam) return;

        // 화면 위쪽(앞쪽) 랜덤 위치
        float vx = Random.value;
        float vy = 1f + margin;

        // 뷰포트 좌표 -> 월드 좌표 변환
        Ray ray = cam.ViewportPointToRay(new Vector3(vx, vy, 0f));
        Plane plane = new Plane(Vector3.up, new Vector3(0f, fixedY, 0f));
        if (plane.Raycast(ray, out float t))
        {
            Vector3 spawnPos = ray.GetPoint(t);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            Enemy e = enemyObj.GetComponent<Enemy>();
            if (e)
            {
                e.fixedY = fixedY;
                e.SetTarget(player.position); // 🎯 스폰 시점의 플레이어 위치로 돌진
            }
        }
    }
}
