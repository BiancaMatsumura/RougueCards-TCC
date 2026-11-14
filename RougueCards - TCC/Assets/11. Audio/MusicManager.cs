using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Músicas")]
    [SerializeField] private AudioClip temaPrincipal;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("Config")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource _source;
    private Coroutine _fadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = volume;
        _source.outputAudioMixerGroup = musicGroup;

        if (temaPrincipal != null) Play(temaPrincipal, instant: true);
    }

    public void Play(AudioClip clip, bool instant = false)
    {
        if (clip == null || (_source.clip == clip && _source.isPlaying)) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);

        if (instant)
        {
            _source.clip = clip;
            _source.volume = volume;
            _source.Play();
        }
        else
        {
            _fadeRoutine = StartCoroutine(FadeTo(clip));
        }
    }

    public void PlayTema() => Play(temaPrincipal);

    IEnumerator FadeTo(AudioClip next)
    {
        float start = _source.volume;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            _source.volume = Mathf.Lerp(start, 0f, t / fadeDuration);
            yield return null;
        }

        _source.clip = next;
        _source.Play();

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            _source.volume = Mathf.Lerp(0f, volume, t / fadeDuration);
            yield return null;
        }

        _source.volume = volume;
    }
}