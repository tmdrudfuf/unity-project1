using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 4f;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 안전망
    }

    void Update()
    {
        // 전방(Z+) 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    // Bullet = Trigger / Enemy = Non-Trigger 이 조합에서 호출됨
    void OnTriggerEnter(Collider other)
    {
        // Enemy만 반응
        if (!other.CompareTag("Enemy"))
        {

            Debug.Log($"Hit: {other.name}");
            Destroy(other.gameObject); // 적 제거
            Destroy(gameObject);       // 총알 제거
        }
    }
}
