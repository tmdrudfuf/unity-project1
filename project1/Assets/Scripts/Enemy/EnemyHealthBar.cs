// Assets/Scripts/UI/EnemyHealthBar.cs
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Assign")]
    [SerializeField] private Image fill;          // Bar_FG (Type=Filled / Horizontal / Left)
    [SerializeField] private bool hideWhenFull = false;

    [Header("Follow")]
    [SerializeField] private Transform target;    // 비우면 Enemy 루트
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.9f, 0f);
    [SerializeField] private Camera cam;          // 비우면 Camera.main

    private EnemyHealth health;
    [SerializeField] private float pushTowardCamera = 0.05f; // 5cm 정도 앞으로

    

    private void Start()
    {
        if (!cam) cam = Camera.main;
        health = GetComponentInParent<EnemyHealth>();
        if (!health) { enabled = false; return; }
        if (!target) target = health.transform;
        if (!fill)   { enabled = false; return; }

        // 초기화 + 구독
        UpdateFill(health.CurrentHP, health.MaxHP);
        health.OnHealthChanged += UpdateFill;
        health.OnDied += HideNow;
    }

    private void LateUpdate()
    {
         if (target) transform.position = target.position + worldOffset;

        if (cam)
        {
            // 바가 항상 카메라를 보게
            Vector3 fwd = cam.transform.forward;
            transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);

            // 카메라 쪽으로 살짝 밀어 메쉬 뒤에 숨지 않도록
            transform.position += fwd * pushTowardCamera;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateFill;
            health.OnDied -= HideNow;
        }
    }

    private void UpdateFill(int cur, int max)
    {
        float t = (max <= 0) ? 0f : Mathf.Clamp01((float)cur / max);
        fill.fillAmount = t;

        if (hideWhenFull && fill.transform.parent)
            fill.transform.parent.gameObject.SetActive(t < 0.999f);

        // EnemyHealthBar.cs - UpdateFill 끝
        fill.color = (t > 0.5f) ? Color.green : (t > 0.25f ? Color.yellow : Color.red);

    }

    private void HideNow()
    {
        if (fill && fill.transform.parent)
            fill.transform.parent.gameObject.SetActive(false);
    }
}
