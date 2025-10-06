using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 8f;
    public float fixedY = 1f;
    private Vector3 targetDir; // 목표 방향(스폰 시 플레이어 위치 기준으로 고정)

    // 👉 스폰 직후 한 번만 호출해서 방향 지정
    public void SetTarget(Vector3 playerPos)
    {
        playerPos.y = fixedY;
        Vector3 dir = (playerPos - transform.position).normalized;
        dir.y = 0;
        targetDir = dir;
    }

    void Update()
    {
        // 높이 고정
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        // 정해진 방향으로 돌진
        transform.position += targetDir * speed * Time.deltaTime;

        // 화면 밖으로 나가면 삭제
        if (Camera.main)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.z < 0f || vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
