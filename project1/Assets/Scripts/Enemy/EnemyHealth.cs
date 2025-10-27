// Assets/Scripts/Enemies/EnemyHealth.cs
using UnityEngine;
using UnityEngine.Audio;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP & Score")]
    [SerializeField] private int maxHP = 5;          // 여러 번 맞아야 죽도록
    [SerializeField] private int scoreOnKill = 100;

    [Header("Hit Feedback")]
    [SerializeField] private bool briefInvincible = true;
    [SerializeField] private float invincibleSeconds = 0.05f; // 다중 히트 보호
    private float invincibleUntil;

    [Header("Death VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float destroyDelay = 0f;

    [Header("SFX: On Hit (피격음)")]
    [Tooltip("맞을 때 재생할 사운드(죽지 않을 때만). 여러 개면 랜덤 선택")]
    [SerializeField] private AudioClip[] hitClips;
    [Range(0f,1f)][SerializeField] private float hitVolMin = 0.9f;
    [Range(0f,1f)][SerializeField] private float hitVolMax = 1.0f;
    [Range(0.5f,2f)][SerializeField] private float hitPitchMin = 0.95f;
    [Range(0.5f,2f)][SerializeField] private float hitPitchMax = 1.05f;
    [SerializeField] private float hitSfxCooldown = 0.03f; // 같은 프레임 중복 방지
    private float lastHitSfxTime = -999f;

    [Header("SFX: On Death (파괴음)")]
    [Tooltip("죽을 때 재생할 사운드. 여러 개면 랜덤 선택")]
    [SerializeField] private AudioClip[] deathClips;
    [Range(0f,1f)][SerializeField] private float deathVolMin = 0.9f;
    [Range(0f,1f)][SerializeField] private float deathVolMax = 1.0f;
    [Range(0.5f,2f)][SerializeField] private float deathPitchMin = 0.95f;
    [Range(0.5f,2f)][SerializeField] private float deathPitchMax = 1.05f;

    [Header("SFX 3D Settings (둘 다 공통)")]
    [Tooltip("0=2D(전역), 1=완전 3D(거리감 적용)")]
    [Range(0f,1f)][SerializeField] private float spatialBlend = 0f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 50f;
    [Tooltip("오디오 믹서 그룹(선택). SFX 그룹에 연결하면 전체 볼륨 제어 쉬움")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    private int currentHP;
    private bool isDead;

    public int CurrentHP => currentHP;
    public int MaxHP     => maxHP;

    // 체력바/외부가 구독
    public System.Action<int,int> OnHealthChanged; // (current, max)
    public System.Action OnDied;

    private void Awake()
    {
        currentHP = Mathf.Max(1, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        if (briefInvincible && Time.time < invincibleUntil) return;

        int prev = currentHP;
        currentHP -= Mathf.Max(1, dmg);
        invincibleUntil = Time.time + invincibleSeconds;

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        // 피격음: 죽지 않았고, 쿨다운 이후에만 재생
        if (currentHP < prev && Time.unscaledTime - lastHitSfxTime >= hitSfxCooldown)
        {
            lastHitSfxTime = Time.unscaledTime;
            PlayOneShotAt(Pick(hitClips), transform.position,
                Random.Range(hitVolMin, hitVolMax),
                Random.Range(hitPitchMin, hitPitchMax));
        }

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(maxHP, currentHP + Mathf.Max(1, amount));
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 점수 추가
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.Add(scoreOnKill);

        // 파괴음
        PlayOneShotAt(Pick(deathClips), transform.position,
            Random.Range(deathVolMin, deathVolMax),
            Random.Range(deathPitchMin, deathPitchMax));

        // VFX
        if (deathVfxPrefab)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        OnDied?.Invoke();

        if (destroyDelay <= 0f) Destroy(gameObject);
        else                    Destroy(gameObject, destroyDelay);
    }

    // ─────────────────────────────
    // 내부 헬퍼: 클립 랜덤 선택 & 원샷 재생
    private AudioClip Pick(AudioClip[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }

    private void PlayOneShotAt(AudioClip clip, Vector3 pos, float volume, float pitch)
    {
        if (!clip) return;

        var go = new GameObject("[SFX] EnemyHealth OneShot");
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch  = Mathf.Clamp(pitch, 0.5f, 2f);
        src.spatialBlend = spatialBlend;
        src.minDistance  = minDistance;
        src.maxDistance  = maxDistance;
        src.rolloffMode  = AudioRolloffMode.Linear;
        src.playOnAwake  = false;
        src.loop = false;
        if (outputMixerGroup) src.outputAudioMixerGroup = outputMixerGroup;

        src.Play();
        Destroy(go, clip.length / Mathf.Max(0.01f, src.pitch));
    }
    // ─────────────────────────────

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxHP = Mathf.Max(1, maxHP);
        minDistance = Mathf.Max(0.01f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
    }
#endif
}
