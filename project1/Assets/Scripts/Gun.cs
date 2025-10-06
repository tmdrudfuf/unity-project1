using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform firePoint;        // 비행기 앞부분(총구) 위치
    public GameObject bulletPrefab;    // Bullet 프리팹
    public float fireRate = 8f;        // 초당 발사 횟수

    float _nextTime;

    void Update()
    {
        if (Time.time >= _nextTime)
        {
            _nextTime = Time.time + (1f / fireRate);
            Shoot();
        }
    }

    void Shoot()
    {
        if (!firePoint || !bulletPrefab) return;

        // 총알 생성: 총구 위치/방향을 그대로 사용
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
