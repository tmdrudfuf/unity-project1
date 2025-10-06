using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;        // 이동 속도
    public float lifeTime = 5f;      // 최대 생존 시간(안전망)
    public float margin = 0.05f;     // 화면 바깥 여유(뷰포트 기준)

    float _alive;

    void Update()
    {
        // 비행기 바라보는 방향(로컬 전방)으로 전진
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

        // 안전망: 일정 시간 지나면 제거
        _alive += Time.deltaTime;
        if (_alive > lifeTime) Destroy(gameObject);

        // 화면 밖 검사(메인 카메라 기준)
        if (Camera.main)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            bool off = (vp.z < 0f) || (vp.x < -margin) || (vp.x > 1f + margin) || (vp.y < -margin) || (vp.y > 1f + margin);
            if (off) Destroy(gameObject);
        }
    }
}
