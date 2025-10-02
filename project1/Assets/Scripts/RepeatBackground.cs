using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    private Vector3 startPos;
    private float repeatLength;

    void Start()
    {
        startPos = transform.position;
        // 월드 기준 크기 사용 (스케일/회전 영향 포함)
        var rend = GetComponent<Renderer>();
        if (rend) repeatLength = rend.bounds.size.z;
        else      repeatLength = GetComponent<Collider>().bounds.size.z;
    }

    void Update()
    {
        // 월드 -Z 로 이동 (카메라 위→아래가 -Z 라는 가정)
        transform.Translate(Vector3.back * 2f * Time.deltaTime, Space.World);

        // Z 기준으로 한 장 길이만큼 내려가면 원위치
        if (transform.position.z < startPos.z - repeatLength*2/3)
        {
            transform.position = startPos;
        }
    }
}
