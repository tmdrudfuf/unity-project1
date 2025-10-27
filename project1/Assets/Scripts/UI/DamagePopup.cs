using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float life = 0.6f;           // 살아있는 시간
    [SerializeField] private float riseDistance = 1.0f;   // 위로 뜨는 거리
    [SerializeField] private AnimationCurve scaleOverLife =
        AnimationCurve.EaseInOut(0f, 1.1f, 1f, 0.9f);

    [Header("Billboard / Visibility")]
    [Tooltip("카메라 쪽으로 당기는 거리(메시 뒤에 가려짐 방지)")]
    [SerializeField] private float pushTowardCamera = 0.35f;   // ▶ 기존보다 강하게
    [Tooltip("생성 시 이 레이어를 강제 적용(카메라 CullingMask에 포함된 레이어)")]
    [SerializeField] private int forceLayer = 0;               // Default = 0

    [Header("Refs")]
    [SerializeField] private TMP_Text tmp;                     // 자식 3D TMP

    private float t;              // 0..1
    private Vector3 startPos;
    private Camera cam;

    private void Awake()
    {
        // 카메라/텍스트 자동 할당(실패해도 Play에서 한 번 더 시도)
        if (!cam) cam = Camera.main;
        if (!tmp) tmp = GetComponentInChildren<TMP_Text>(true);

        // 프리팹 레이어 강제(보이지 않음 방지)
        gameObject.layer = forceLayer;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.gameObject.layer = forceLayer;
    }

    /// <summary>
    /// 팝업 시작(월드좌표, 데미지 수치, 색상)
    /// </summary>
    public void Play(Vector3 worldPos, int amount, Color color)
    {
        if (!cam) cam = Camera.main;
        if (!tmp) tmp = GetComponentInChildren<TMP_Text>(true);

        // (1) 즉시 월드좌표로 이동 + 카메라 쪽으로 당김(부호: -)
        Vector3 pos = worldPos - (cam ? cam.transform.forward * pushTowardCamera : Vector3.zero);
        transform.position = pos;
        startPos = transform.position;

        // (2) 그래픽 초기화
        t = 0f;
        if (tmp)
        {
            tmp.text = "-" + amount.ToString();
            var c = color; c.a = 1f;
            tmp.color = c;
            // 가독성 보정(필요시): 외곽선/쉐도우 머티리얼을 쓰고 있다면 그대로 사용
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            if (tmp.fontSize < 6f) tmp.fontSize = 8f; // 너무 작을 때 대비
        }

        // (3) 혹시 비활성 상태 프리팹이면 보이도록
        gameObject.SetActive(true);

        // (4) 첫 프레임 비주얼 반영
        UpdateVisual(0f);
    }

    private void Update()
    {
        t += Time.deltaTime / Mathf.Max(0.0001f, life);
        UpdateVisual(t);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateVisual(float tt)
    {
        // 위치: 시작점에서 위로 상승
        transform.position = startPos + Vector3.up * (riseDistance * tt);

        // 카메라 바라보기(빌보드)
        if (cam)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
        }

        // 스케일/알파
        float s = scaleOverLife.Evaluate(tt);
        transform.localScale = Vector3.one * s;

        if (tmp)
        {
            Color c = tmp.color;
            c.a = 1f - tt;
            tmp.color = c;
        }
    }
}
