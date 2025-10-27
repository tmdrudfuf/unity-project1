// Assets/Scripts/Camera/FixedAspectCamera.cs
using UnityEngine;

/// <summary>
/// 카메라의 ViewportRect를 조정해서 지정한 종횡비를 유지.
/// 화면과 비율이 다르면 위/아래(레터박스) 또는 좌/우(필러박스) 여백을 만든다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    [Tooltip("예: 16:9 는 16/9 = 1.777...")]
    [SerializeField] private float targetAspect = 16f / 9f;

    private Camera cam;
    private int lastW, lastH;

    public Rect CurrentViewportRect { get; private set; }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyViewport();
    }

    private void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
        {
            ApplyViewport();
        }
    }

    private void ApplyViewport()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1f)
        {
            // 화면이 더 세로로 길다 → 위/아래 레터박스
            float viewportHeight = scaleHeight;
            rect.height = viewportHeight;
            rect.y = (1f - viewportHeight) * 0.5f;
        }
        else
        {
            // 화면이 더 가로로 길다 → 좌/우 필러박스
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) * 0.5f;
        }

        cam.rect = rect;
        CurrentViewportRect = rect;
    }
}
