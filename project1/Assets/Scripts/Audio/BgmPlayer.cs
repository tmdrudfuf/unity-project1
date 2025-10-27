using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BgmPlayer : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.8f;
    [SerializeField] private bool loop = true;
    [Tooltip("AudioMixer의 SFX와 분리해서 관리하고 싶다면 BGM 그룹을 연결")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    [Header("Fades (unscaled time)")]
    [SerializeField] private float fadeInSeconds = 0.6f;
    [SerializeField] private float fadeOutSeconds = 0.6f;

    [Header("GameOver Hook")]
    [Tooltip("플레이어의 PlayerLives. 비워두면 자동 검색")]
    [SerializeField] private PlayerLives playerLives;

    private AudioSource _src;
    private Coroutine _fadeCo;
    private float _targetVolume;

    private void Reset()
    {
        // 에디터에서 붙일 때 기본 세팅
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = true;
        _src.spatialBlend = 0f;  // BGM은 2D 권장
    }

    private void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = loop;
        _src.spatialBlend = 0f; // 전역으로 들리게
        if (outputMixerGroup) _src.outputAudioMixerGroup = outputMixerGroup;

        if (!playerLives) playerLives = FindFirstObjectByType<PlayerLives>();

        // 시작 시 자동 플레이 + 페이드인
        if (bgmClip)
        {
            _src.clip = bgmClip;
            _src.volume = 0f;
            _src.Play();
            _targetVolume = Mathf.Clamp01(bgmVolume);
            StartFadeIn();
        }
        else
        {
            Debug.LogWarning("[BgmPlayer] bgmClip이 비어 있습니다.", this);
        }
    }

    private void OnEnable()
    {
        if (!playerLives) playerLives = FindFirstObjectByType<PlayerLives>();
        if (playerLives) playerLives.onGameOver.AddListener(OnGameOver);
    }

    private void OnDisable()
    {
        if (playerLives) playerLives.onGameOver.RemoveListener(OnGameOver);
    }

    private void OnDestroy()
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
    }

    private void OnGameOver()
    {
        // 게임오버 시 페이드아웃 후 정지
        StartFadeOut(stopAfterFade: true);
    }

    // ── Fade helpers (unscaled time: Time.timeScale==0에서도 정상) ──
    private void StartFadeIn()
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeVolumeRoutine(0f, _targetVolume, fadeInSeconds));
    }

    private void StartFadeOut(bool stopAfterFade)
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeVolumeRoutine(_src.volume, 0f, fadeOutSeconds, stopAfterFade));
    }

    private IEnumerator FadeVolumeRoutine(float from, float to, float seconds, bool stopAfter = false)
    {
        seconds = Mathf.Max(0.0001f, seconds);
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            _src.volume = Mathf.Lerp(from, to, k);
            yield return null;
        }
        _src.volume = to;
        if (stopAfter) _src.Stop();
        _fadeCo = null;
    }

    // (선택) 외부에서 재시작하고 싶을 때 호출 가능
    public void RestartBgm()
    {
        if (!bgmClip) return;
        _src.Stop();
        _src.clip = bgmClip;
        _src.volume = 0f;
        _src.Play();
        _targetVolume = Mathf.Clamp01(bgmVolume);
        StartFadeIn();
    }
}
