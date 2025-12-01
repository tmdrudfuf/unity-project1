// Assets/Scripts/Enemies/EnemyHealth.cs
using UnityEngine;
using UnityEngine.Audio;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP & Score")]
    [SerializeField] private int maxHP = 5;
    [SerializeField] private int scoreOnKill = 100;

    [Header("Difficulty Scaling (Spawn Only)")]
    [SerializeField] private bool applySpawnBonus = true;

    [Header("Hit Feedback")]
    [SerializeField] private bool briefInvincible = true;
    [SerializeField] private float invincibleSeconds = 0.05f;
    private float invincibleUntil;

    [Header("Death VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float destroyDelay = 0f;

    [Header("SFX: On Hit")]
    [SerializeField] private AudioClip[] hitClips;
    [Range(0f,1f)] [SerializeField] private float hitVolMin = 0.9f;
    [Range(0f,1f)] [SerializeField] private float hitVolMax = 1.0f;
    [Range(0.5f,2f)] [SerializeField] private float hitPitchMin = 0.95f;
    [Range(0.5f,2f)] [SerializeField] private float hitPitchMax = 1.05f;
    [SerializeField] private float hitSfxCooldown = 0.03f;
    private float lastHitSfxTime = -999f;

    [Header("SFX: On Death")]
    [SerializeField] private AudioClip[] deathClips;
    [Range(0f,1f)] [SerializeField] private float deathVolMin = 0.9f;
    [Range(0f,1f)] [SerializeField] private float deathVolMax = 1.0f;
    [Range(0.5f,2f)] [SerializeField] private float deathPitchMin = 0.95f;
    [Range(0.5f,2f)] [SerializeField] private float deathPitchMax = 1.05f;

    [Header("SFX 3D Settings")]
    [Range(0f,1f)] [SerializeField] private float spatialBlend = 0f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    [Header("Debug Logging")]
    [SerializeField] private bool verboseLogs = true;

    private int currentHP;
    private int effectiveMaxHP;
    private bool isDead;
    private bool dropTried;
    private bool isDespawning;
    private static bool isQuitting;

    public int CurrentHP => currentHP;
    public int MaxHP => effectiveMaxHP;

    public System.Action<int,int> OnHealthChanged;
    public System.Action OnDied;

    private void Awake()
    {
        int bonus = 0;
        if (applySpawnBonus && DifficultyManager.Instance != null)
            bonus = DifficultyManager.Instance.CurrentBonusHP;

        effectiveMaxHP = Mathf.Max(1, maxHP + bonus);
        currentHP = effectiveMaxHP;

        if (verboseLogs)
            Debug.Log($"[EnemyHealth] Awake on '{name}' | baseMaxHP={maxHP}, bonus={bonus}, effectiveMaxHP={effectiveMaxHP}, applySpawnBonus={applySpawnBonus}");

        OnHealthChanged?.Invoke(currentHP, effectiveMaxHP);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || isDespawning) { if (verboseLogs) Debug.Log($"[EnemyHealth] '{name}' dmg ignored (dead/despawning)."); return; }

        if (briefInvincible && Time.time < invincibleUntil)
        {
            if (verboseLogs) Debug.Log($"[EnemyHealth] '{name}' TakeDamage({dmg}) ignored: invincible until {invincibleUntil:0.000} (now {Time.time:0.000})");
            return;
        }

        int prev = currentHP;
        currentHP -= Mathf.Max(1, dmg);
        invincibleUntil = Time.time + invincibleSeconds;

        if (verboseLogs)
            Debug.Log($"[EnemyHealth] '{name}' took {dmg} → HP {prev}->{currentHP}/{effectiveMaxHP}");

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (currentHP < prev && Time.unscaledTime - lastHitSfxTime >= hitSfxCooldown)
        {
            lastHitSfxTime = Time.unscaledTime;
            PlayOneShotAt(Pick(hitClips), transform.position,
                Random.Range(hitVolMin, hitVolMax),
                Random.Range(hitPitchMin, hitPitchMax));
        }

        OnHealthChanged?.Invoke(currentHP, effectiveMaxHP);
    }

    /// <summary>정상 처치(드랍/점수 발생)</summary>
    public void Die()
    {
        if (isDead || isDespawning) return;
        isDead = true;

        if (verboseLogs) Debug.Log($"[EnemyHealth] '{name}' DIE()");
        TryDrop(); // 드랍은 여기서 1회

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Add(scoreOnKill);
            if (verboseLogs) Debug.Log($"[EnemyHealth] '{name}' +{scoreOnKill} score");
        }
        else if (verboseLogs)
        {
            Debug.Log($"[EnemyHealth] '{name}' ScoreManager not found.");
        }

        PlayOneShotAt(Pick(deathClips), transform.position,
            Random.Range(deathVolMin, deathVolMax),
            Random.Range(deathPitchMin, deathPitchMax));

        if (deathVfxPrefab)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        OnDied?.Invoke();

        if (destroyDelay <= 0f) Destroy(gameObject);
        else Destroy(gameObject, destroyDelay);
    }

    /// <summary>드랍/점수 없이 정리(화면 밖 등)</summary>
    public void Despawn()
    {
        if (isDead || isDespawning) return;
        isDespawning = true;
        if (verboseLogs) Debug.Log($"[EnemyHealth] '{name}' DESPAWN()");
        Destroy(gameObject);
    }

    private void TryDrop()
    {
        if (dropTried) return;
        dropTried = true;

        var dropper = GetComponent<SpecialEnemyDropper>();
        if (dropper != null)
        {
            if (verboseLogs) Debug.Log($"[EnemyHealth] '{name}' -> SpecialEnemyDropper.OnDead()");
            dropper.OnDead();
        }
        else
        {
            if (verboseLogs) Debug.LogWarning($"[EnemyHealth] '{name}' has NO SpecialEnemyDropper. No drop.");
        }
    }

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxHP = Mathf.Max(1, maxHP);
        minDistance = Mathf.Max(0.01f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
    }
#endif

    private void OnDestroy()
    {
        if (isQuitting) return;

        // ⚠ 외부 Destroy로 제거될 때도 "한 번은" 드랍만 보장
        if (!isDead && !isDespawning)
        {
            if (verboseLogs) Debug.LogWarning($"[EnemyHealth] '{name}' OnDestroy WITHOUT Die()/Despawn() — destroyed externally? Forcing drop.");
            TryDrop(); // 점수 없이 드랍만
        }
    }

    private void OnApplicationQuit() { isQuitting = true; }
}
