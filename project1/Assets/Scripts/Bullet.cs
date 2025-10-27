using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifeSeconds = 4f;

    [Header("Combat")]
    [SerializeField] private int damage = 1;
    [SerializeField] private bool useLayerFilter = false;
    [SerializeField] private LayerMask hittableLayers = ~0;

    [Header("VFX")]
    [SerializeField] private DamagePopup damagePopupPrefab;

    private Rigidbody rb;
    private Collider col;

    // 선분 레이캐스트용
    private Vector3 prevPos;

    // 중복 히트 방지
    private bool hasHit;

    private void Awake()
    {
        TryGetComponent(out rb);
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        CancelInvoke(nameof(SelfDestruct));
        Invoke(nameof(SelfDestruct), lifeSeconds);

        // Trigger + Kinematic + Translate 강제
        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.None;
        }
        if (col) col.isTrigger = true;

        hasHit = false;
        prevPos = transform.position;
    }

    private void Update()
    {
        Vector3 start = transform.position;
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        prevPos = start;
    }

    private void SelfDestruct()
    {
        if (gameObject.activeInHierarchy) Destroy(gameObject);
    }

    private void Hit(Collider other)
    {
        if (hasHit) return; // 중복 방지

        if (useLayerFilter && (hittableLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        var hp = other.GetComponentInParent<EnemyHealth>();
        if (hp == null)
        {
            Destroy(gameObject);
            return;
        }

        hasHit = true;
        if (col) col.enabled = false; // 추가 트리거 차단

        hp.TakeDamage(damage);

        // 충돌 지점 계산: 같은 콜라이더 Raycast → Physics.Raycast → ClosestPoint
        Vector3 hitPos = other.ClosestPoint(transform.position);
        Vector3 seg = transform.position - prevPos;
        float dist = seg.magnitude;

        if (dist > 0.0001f)
        {
            Vector3 dir = seg / dist;
            if (other.Raycast(new Ray(prevPos, dir), out RaycastHit rh1, dist))
                hitPos = rh1.point;
            else if (Physics.Raycast(prevPos, dir, out RaycastHit rh2, dist, hittableLayers, QueryTriggerInteraction.Ignore))
                hitPos = rh2.point;
        }

        // ▶ 확인용 로그(원하면 주석):
        // Debug.Log($"POP @{hitPos}");

        if (!damagePopupPrefab)
        {
            Debug.LogWarning("Bullet: DamagePopup prefab not assigned.");
        }
        else
        {
            var popup = Instantiate(damagePopupPrefab);
            popup.Play(hitPos, damage, new Color(1f, 0.3f, 0.3f));
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) => Hit(other);
    private void OnCollisionEnter(Collision c)   => Hit(c.collider);
}
