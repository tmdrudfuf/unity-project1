using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class PlayerLivesSfx : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerLives playerLives; // 같은 오브젝트에 PlayerLives가 있으면 자동 할당 시도

    [Header("Hit SFX (플레이어 라이프 감소 시)")]
    [SerializeField] private AudioClip[] hitClips;  // 여러 개면 랜덤 재생
    [Range(0f,1f)] [SerializeField] private float hitVolMin = 0.9f;
    [Range(0f,1f)] [SerializeField] private float hitVolMax = 1.0f;
    [Range(0.5f,2f)] [SerializeField] private float hitPitchMin = 0.95f;
    [Range(0.5f,2f)] [SerializeField] private float hitPitchMax = 1.05f;
    [SerializeField] private float hitCooldown = 0.03f; // 같은 프레임 중복 방지
    private float lastHitTime = -999f;

    [Header("Game Over SFX")]
    [SerializeField] private AudioClip[] gameOverClips;
    [Range(0f,1f)] [SerializeField] private float goVolMin = 0.9f;
    [Range(0f,1f)] [SerializeField] private float goVolMax = 1.0f;
    [Range(0.5f,2f)] [SerializeField] private float goPitchMin = 1.0f;
    [Range(0.5f,2f)] [SerializeField] private float goPitchMax = 1.0f;

    [Header("3D Settings (공통)")]
    [Tooltip("0=2D(전역 재생), 1=3D(거리감 적용)")]
    [Range(0f,1f)] [SerializeField] private float spatialBlend = 0f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    private int prevLives = -1;

    private void Reset()
    {
        if (!playerLives) playerLives = GetComponent<PlayerLives>();
    }

    private void OnEnable()
    {
        if (!playerLives) playerLives = GetComponent<PlayerLives>();
        if (!playerLives) { enabled = false; return; }

        prevLives = playerLives.CurrentLives;
        playerLives.onLivesChanged.AddListener(OnLivesChanged);
        playerLives.onGameOver.AddListener(OnGameOver);
    }

    private void OnDisable()
    {
        if (!playerLives) return;
        playerLives.onLivesChanged.RemoveListener(OnLivesChanged);
        playerLives.onGameOver.RemoveListener(OnGameOver);
    }

    private void OnLivesChanged(int current, int max)
    {
        // 이전값보다 줄었을 때만 피격음
        if (prevLives >= 0 && current < prevLives && Time.unscaledTime - lastHitTime >= hitCooldown)
        {
            lastHitTime = Time.unscaledTime;
            var clip = Pick(hitClips);
            PlayOneShot(clip, playerLives.transform.position,
                        Random.Range(hitVolMin, hitVolMax),
                        Random.Range(hitPitchMin, hitPitchMax));
        }
        prevLives = current;
    }

    private void OnGameOver()
    {
        var clip = Pick(gameOverClips);
        PlayOneShot(clip, playerLives.transform.position,
                    Random.Range(goVolMin, goVolMax),
                    Random.Range(goPitchMin, goPitchMax));
    }

    // ─── 내부 헬퍼 ─────────────────────────────────────────────
    private AudioClip Pick(AudioClip[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }

    /// 오브젝트가 즉시 Destroy되어도 끝까지 들리도록 임시 AudioSource 생성 후 자동 파괴
    private void PlayOneShot(AudioClip clip, Vector3 position, float volume, float pitch)
    {
        if (!clip) return;

        var go = new GameObject("[SFX] Player OneShot");
        go.transform.position = position;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch  = Mathf.Clamp(pitch, 0.5f, 2f);
        src.spatialBlend = spatialBlend;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.rolloffMode = AudioRolloffMode.Linear;
        if (outputMixerGroup) src.outputAudioMixerGroup = outputMixerGroup;

        src.Play();
        Destroy(go, clip.length / Mathf.Max(0.01f, src.pitch));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minDistance = Mathf.Max(0.01f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
    }
#endif
}
