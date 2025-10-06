using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 10f; // 이동 속도
    public float xLimit = 10f;    // 좌우 한계 (원하면 조정)
    public float zLimit = 20f;    // 앞뒤 한계 (원하면 크게/제거)

    [Header("Height")]
    public bool lockY = true;     // y(높이) 고정할지
    public float fixedY = 0f;     // 고정 높이 값

    void Start()
    {
        if (lockY)
        {
            // 시작할 때 높이를 고정 값으로 맞춤
            Vector3 p = transform.position;
            p.y = fixedY;
            transform.position = p;
        }
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); // ← →
        float moveZ = Input.GetAxis("Vertical");   // ↑ ↓

        Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // 화면/맵 범위 제한 (z도 이제 자유롭게, 범위만 클램프)
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -xLimit, xLimit);
        pos.z = Mathf.Clamp(pos.z, -zLimit, zLimit);

        if (lockY) pos.y = fixedY; // y 고정
        transform.position = pos;
    }
}
