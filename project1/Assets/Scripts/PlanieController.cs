using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 10f;  // 이동 속도 (키 반응)
    public float accel = 12f;      // 가감속 보간 (크면 즉각적)

    [Header("Play Area (auto from camera)")]
    public float fixedY = 1f;      // 비행기 높이 고정값
    [Range(0f, 0.2f)]
    public float screenMargin = 0.02f; // 화면 안쪽 여유(퍼센트)

    // 내부 상태
    private Vector3 _vel;          // 현재 속도 (보간용)

    void Start()
    {
        // Rigidbody 쓰면 중력 끄고 회전 고정 추천
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        }

        // 시작 시 높이를 고정값으로 맞춤
        var p = transform.position;
        p.y = fixedY;
        transform.position = p;
    }

    void Update()
    {
        // 1) 입력 (← → = x, ↑ ↓ = z)
        float ix = Input.GetAxisRaw("Horizontal"); // -1,0,1
        float iz = Input.GetAxisRaw("Vertical");

        // 2) 목표 속도 & 가감속 보간
        Vector3 targetVel = new Vector3(ix, 0f, iz) * moveSpeed;
        _vel = Vector3.Lerp(_vel, targetVel, Time.deltaTime * accel);

        // 3) 이동 (월드 기준, y는 고정)
        transform.position += _vel * Time.deltaTime;

        // 4) 화면 경계로 클램프 (비행기 크기 고려)
        ClampToCameraView();

        // 5) 높이 고정
        var p = transform.position;
        p.y = fixedY;
        transform.position = p;
    }

    void ClampToCameraView()
    {
        var cam = Camera.main;
        if (!cam) return;

        // 화면 내부 모서리 두 점(왼아래, 오른위)을 y=fixedY 평면으로 투영
        Vector3 worldMin = ViewportToWorldOnY(cam, new Vector2(0f + screenMargin, 0f + screenMargin), fixedY);
        Vector3 worldMax = ViewportToWorldOnY(cam, new Vector2(1f - screenMargin, 1f - screenMargin), fixedY);

        // 비행기 실제 반경(가시 영역)을 계산 (Renderer 우선, 없으면 Collider)
        float extX = 0f, extZ = 0f;
        if (TryGetComponent<Renderer>(out var rend))
        {
            extX = rend.bounds.extents.x;
            extZ = rend.bounds.extents.z;
        }
        else if (TryGetComponent<Collider>(out var col))
        {
            extX = col.bounds.extents.x;
            extZ = col.bounds.extents.z;
        }

        // 위치 클램프
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, worldMin.x + extX, worldMax.x - extX);
        pos.z = Mathf.Clamp(pos.z, worldMin.z + extZ, worldMax.z - extZ);
        transform.position = pos;
    }

    /// <summary>
    /// 카메라의 뷰포트 좌표(0~1)를 y=fixedY 평면에 맞춰 월드 좌표로 변환.
    /// Orthographic/Perspective 모두 동작.
    /// </summary>
    static Vector3 ViewportToWorldOnY(Camera cam, Vector2 vp, float y)
    {
        Ray r = cam.ViewportPointToRay(new Vector3(vp.x, vp.y, 0f));
        Plane plane = new Plane(Vector3.up, new Vector3(0f, y, 0f));
        if (plane.Raycast(r, out float t))
            return r.GetPoint(t);

        // 폴백: 근접 평면 기준
        Vector3 p = cam.ViewportToWorldPoint(new Vector3(vp.x, vp.y, cam.nearClipPlane + 1f));
        p.y = y;
        return p;
    }
}
