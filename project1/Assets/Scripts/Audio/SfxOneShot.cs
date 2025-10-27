// Assets/Scripts/Audio/SfxOneShot.cs
using UnityEngine;
using UnityEngine.Audio;

public static class SfxOneShot
{
    /// <summary>
    /// 임시 AudioSource를 만들어 1회 재생 후 자동 파괴.
    /// 파괴 직전 호출해도 끝까지 들립니다.
    /// </summary>
    public static void Play(AudioClip clip, Vector3 position,
        float volume = 1f, float pitch = 1f, float spatialBlend = 0f,
        float minDistance = 1f, float maxDistance = 50f,
        AudioMixerGroup outputGroup = null)
    {
        if (!clip) return;

        var go = new GameObject("[SFX] OneShot");
        go.transform.position = position;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch  = Mathf.Clamp(pitch, 0.5f, 2f);
        src.spatialBlend = Mathf.Clamp01(spatialBlend);
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.playOnAwake = false;
        src.loop = false;
        if (outputGroup) src.outputAudioMixerGroup = outputGroup;

        src.Play();
        Object.Destroy(go, clip.length / Mathf.Max(0.01f, src.pitch));
    }

    public static AudioClip Pick(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
