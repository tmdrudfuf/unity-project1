using UnityEngine;

public class BackgroundTilerScreenDown : MonoBehaviour
{
    public float speed = 2f;                 // 스크롤 속도 (화면 아래 방향)
    public Transform[] tiles = new Transform[2]; // 배경 타일 2장 (권장: 인스펙터에 직접 할당)

    private Camera cam;
    private Vector3 moveDir;     // 화면 아래 방향 = -cam.transform.up
    private float tileLength;    // 두 타일 사이 초기 간격(이동 방향 성분)
    private float depthLock;     // 카메라 forward 축에 대한 고정 깊이

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[BackgroundTiler] Main Camera가 없습니다.");
            enabled = false; return;
        }

        // 타일이 비어 있으면 자식 두 개 자동 할당
        if (tiles == null || tiles.Length < 2 || tiles[0] == null || tiles[1] == null)
        {
            if (transform.childCount >= 2)
            {
                tiles = new Transform[2] { transform.GetChild(0), transform.GetChild(1) };
            }
            else
            {
                Debug.LogError("[BackgroundTiler] 자식으로 배경 타일 2개가 필요합니다.");
                enabled = false; return;
            }
        }

        // 화면 아래쪽으로 이동 (카메라 기준)
        moveDir = -cam.transform.up.normalized;

        // 초기 간격을 이동 방향으로 투영하여 '한 장 길이'로 사용
        tileLength = Mathf.Abs(Vector3.Dot(tiles[1].position - tiles[0].position, moveDir));
        if (tileLength <= 0.0001f)
        {
            Debug.LogError("[BackgroundTiler] 두 타일을 화면 아래 방향으로 정확히 한 장 길이만큼 떨어뜨려 배치해 주세요.");
            enabled = false; return;
        }

        // 깊이(카메라 forward 성분) 고정 — 퍼스펙티브라도 크기 변하지 않게
        depthLock = Vector3.Dot(tiles[0].position, cam.transform.forward);
        LockDepth(tiles[0]);
        LockDepth(tiles[1]);
    }

    void Update()
    {
        Vector3 delta = moveDir * speed * Time.deltaTime;

        // 이동 (화면 아래로만)
        tiles[0].position += delta;
        tiles[1].position += delta;

        // 깊이 고정(혹시라도 떠밀려난 경우 보정)
        LockDepth(tiles[0]);
        LockDepth(tiles[1]);

        // 순환 재배치
        RecycleIfPassed(0, 1);
        RecycleIfPassed(1, 0);
    }

    // i 타일이 other 타일보다 이동 반대쪽으로 'tileLength 이상' 뒤로 갔으면 앞으로 보냄
    private void RecycleIfPassed(int i, int other)
    {
        float signedGap = Vector3.Dot(tiles[i].position - tiles[other].position, moveDir);
        if (signedGap <= -tileLength)
        {
            tiles[i].position = tiles[other].position + moveDir * tileLength;
            LockDepth(tiles[i]);
        }
    }

    // 카메라 forward 방향 성분(깊이) 고정 -> 가까워지거나 멀어지는 느낌 방지
    private void LockDepth(Transform t)
    {
        Vector3 p = t.position;
        // 현재 깊이
        float cur = Vector3.Dot(p, cam.transform.forward);
        // 깊이 차이를 forward 방향으로 보정해 동일한 깊이 유지
        float diff = depthLock - cur;
        t.position = p + cam.transform.forward * diff;
    }
}
