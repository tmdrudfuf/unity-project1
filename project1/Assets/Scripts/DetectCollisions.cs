using UnityEngine;

public class DetectCollisions : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Projectile(총알)에만 반응하도록 태그 검사
        if (other.CompareTag("Projectile"))
        {
            Destroy(other.gameObject); // 총알 삭제
            Destroy(gameObject);       // 적 삭제
        }
    }
}
