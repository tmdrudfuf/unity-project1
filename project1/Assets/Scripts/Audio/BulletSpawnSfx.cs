// Assets/Scripts/Audio/BulletSpawnSfx.cs
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class BulletSpawnSfx : MonoBehaviour
{
    [Header("Clips (one chosen at random)")]
    [SerializeField] private AudioClip[] fireClips;

    [Header("Volume / Pitch")]
    [Range(0f,1f)] [SerializeField] private float volumeMin = 0.9f;
    [Range(0f,1f)] [SerializeField] private float volumeMax = 1.0f;
    [Range(0.5f,2f)] [SerializeField] private float pitchMin  = 0.98f;
    [Range(0.5f,2f)] [SerializeField] private float pitchMax  = 1.02f;

    [Header("3D Settings")]
    [Range(0f,1f)] [SerializeField] private float spatialBlend = 0f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 40f;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    private void OnEnable()
    {
        var clip = SfxOneShot.Pick(fireClips);
        if (!clip) return;

        float v = Random.Range(volumeMin, volumeMax);
        float p = Random.Range(pitchMin,  pitchMax);
        SfxOneShot.Play(clip, transform.position, v, p, spatialBlend, minDistance, maxDistance, outputMixerGroup);
    }
}
