// Assets/Scripts/UI/PillarboxMarginsUI.cs
using UnityEngine;

[ExecuteAlways] // 에디트 모드에서도 동작
[RequireComponent(typeof(RectTransform))]
public class PillarboxMarginsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Canvas rootCanvas;

    [Header("Optional Margin Panels (RectTransform)")]
    public RectTransform leftMargin;
    public RectTransform rightMargin;
    public RectTransform topMargin;
    public RectTransform bottomMargin;

    private RectTransform rt;
    private int lastW, lastH;
    private Rect lastCamRect;

    private void OnEnable()
    {
        CacheRefs();
        Apply(true);
    }

    private void Reset()
    {
        CacheRefs();
    }

    private void OnValidate()
    {
        // 인스펙터에서 슬롯 바꿀 때 즉시 반영
        Apply(true);
    }

    private void Update()
    {
        // 화면 크기나 카메라 rect 바뀌면 갱신
        if (!rootCanvas || !targetCamera) return;

        bool screenChanged = (Screen.width != lastW || Screen.height != lastH);
        bool camRectChanged = targetCamera.rect != lastCamRect;

        if (screenChanged || camRectChanged || !Application.isPlaying)
        {
            Apply();
        }
    }

    [ContextMenu("Refresh Now")]
    private void RefreshNow()
    {
        Apply(true);
    }

    private void CacheRefs()
    {
        if (!rt) rt = GetComponent<RectTransform>();
        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
        if (!targetCamera && Camera.main) targetCamera = Camera.main;
    }

    private void Apply(bool logWarn = false)
    {
        lastW = Screen.width;
        lastH = Screen.height;

        if (!rootCanvas || !targetCamera)
        {
            if (logWarn)
                Debug.LogWarning("[PillarboxMarginsUI] Root Canvas 또는 Target Camera가 비어 있습니다.", this);
            return;
        }

        Rect normalized = targetCamera.rect;   // 0..1
        lastCamRect = normalized;

        // Canvas의 픽셀 영역
        Rect pixel = rootCanvas.pixelRect;

        float viewX = normalized.x * pixel.width;
        float viewY = normalized.y * pixel.height;
        float viewW = normalized.width * pixel.width;
        float viewH = normalized.height * pixel.height;

        float leftW   = Mathf.Max(0f, viewX);
        float rightW  = Mathf.Max(0f, pixel.width  - (viewX + viewW));
        float bottomH = Mathf.Max(0f, viewY);
        float topH    = Mathf.Max(0f, pixel.height - (viewY + viewH));

        if (leftMargin)   SetVerticalStrip(leftMargin, 0f, leftW,  0f, 1f);
        if (rightMargin)  SetVerticalStrip(rightMargin,1f, rightW, 0f, 1f);
        if (bottomMargin) SetHorizontalStrip(bottomMargin,0f, bottomH,0f, 1f);
        if (topMargin)    SetHorizontalStrip(topMargin,  1f, topH,   0f, 1f);

#if UNITY_EDITOR
        // 에디터에서 눈으로 확인하기 쉽도록, 패널이 있다면 Raycast Target은 꺼 두는 걸 권장
        // (선택 사항) 여기서 색도 임시로 바꿀 수 있지만, 프로젝트 스타일을 존중해 생략.
#endif
    }

    private void SetVerticalStrip(RectTransform strip, float side01, float width, float bottom01, float top01)
    {
        // side01: 0 = 왼쪽, 1 = 오른쪽
        float x = (side01 <= 0f) ? 0f : 1f;

        strip.anchorMin = new Vector2(x, bottom01);
        strip.anchorMax = new Vector2(x, top01);
        strip.pivot     = new Vector2(side01, 0.5f);
        strip.sizeDelta = new Vector2(width, 0f);
        strip.anchoredPosition = Vector2.zero;
    }

    private void SetHorizontalStrip(RectTransform strip, float side01, float height, float left01, float right01)
    {
        // side01: 0 = 아래, 1 = 위
        float y = (side01 <= 0f) ? 0f : 1f;

        strip.anchorMin = new Vector2(left01, y);
        strip.anchorMax = new Vector2(right01, y);
        strip.pivot     = new Vector2(0.5f, side01);
        strip.sizeDelta = new Vector2(0f, height);
        strip.anchoredPosition = Vector2.zero;
    }
}
