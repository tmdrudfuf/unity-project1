using UnityEngine;

/// <summary>
/// 경계/청소 구역: 들어온 적은 점수/드랍 없이 정리한다.
/// - EnemyHealth가 있으면 Despawn() 호출 (드랍/점수 X)
/// - 일반 물체는 기존처럼 Destroy
/// 사용법: 바닥/하단 경계 등에 Trigger 콜라이더로 배치.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillZoneDespawn : MonoBehaviour
{
    private void Reset()
    {
        // 에디터에서 자동으로 트리거로 전환
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var eh = other.GetComponentInParent<EnemyHealth>() ?? other.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.Despawn();           // ✅ 외부 Destroy 금지, 정식 정리 루트
            return;
        }

        // 적이 아니면 그대로 정리
        Destroy(other.gameObject);
    }
}
