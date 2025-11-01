using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonDontDestroy<SoundManager>
{
    const string MASTER_PARAM = "MasterVolume";
    const string BGM_PARAM = "BGMVolume";
    const string SFX_PARAM = "SFXVolume";

    [Header("Audio Mixer")]
    [SerializeField]
    AudioMixer m_mixer;

    [Header("Background Sound")]
    [SerializeField]
    AudioSource m_backgroundAudioSource;
    ObjectPool<AudioSource> m_audioSourcePool;

    protected override void Awake()
    {
        base.Awake();
        m_mixer = Resources.Load<AudioMixer>("Sound/MasterMixer");

        m_backgroundAudioSource = GetComponent<AudioSource>();
        m_backgroundAudioSource.loop = true;
        m_backgroundAudioSource.playOnAwake = false;

        m_audioSourcePool = new ObjectPool<AudioSource>(10, () =>
        {
            var audioSourceObj = new GameObject("SFXAudioSource");
            audioSourceObj.transform.parent = transform.GetChild(0);
            
            var audioSource = audioSourceObj.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = m_mixer.FindMatchingGroups("SFX")[0];
            audioSource.volume = 0.7f;
            return audioSource;
        });
    }

    protected override void Start()
    {
        ThemeManager.Instance.OnThemeChanged += PlayBGM;
    }

    private void OnDisable()
    {
        ThemeManager.Instance.OnThemeChanged -= PlayBGM;
    }

    public void SetMasterVolume(float volume)
    {
        SetVolumeParameter(MASTER_PARAM, volume);
    }

    public void SetBGMVolume(float volume)
    {
        SetVolumeParameter(BGM_PARAM, volume);
    }
    public void SetSFXVolume(float volume)
    {
        SetVolumeParameter(SFX_PARAM, volume);
    }

    public void GetVolumeParameter(string param, out float volume)
    {
        m_mixer.GetFloat(param, out volume);
    }

    void SetVolumeParameter(string param, float volume)
    {
        // 볼륨 범위 클램프
        volume = Mathf.Clamp01(volume);
        // 0.0001~1 사이 값 → -80dB ~ 0dB
        float dB = (volume <= 0f)
        ? -80f
            : Mathf.Log10(volume) * 20f;
        m_mixer.SetFloat(param, dB);
    }

    public void PlaySFX(string name)
    {
        var audioSource = m_audioSourcePool.Get();
        audioSource.clip = LoadClip(name);
        audioSource.loop = false;
        audioSource.Play();
        StartCoroutine(Coroutine_ReturnToPool(audioSource));
    }

    public void PlayBGM(ThemeProfile theme)
    {
        var next = theme.m_backgroundMusic;
        var fadeTime = theme.m_musicFade;

        StartCoroutine(Coroutine_FadeSwap(m_backgroundAudioSource, next, fadeTime));
    }


    public AudioClip LoadClip(string name)
    {
        var audioClip = Resources.Load<AudioClip>("Sound/" + name);
        if (audioClip == null) Debug.Log("audioClip is NULL");
        return audioClip;
    }

    IEnumerator Coroutine_ReturnToPool(AudioSource audioSoruce)
    {
        yield return null;
        yield return new WaitWhile(() => audioSoruce.isPlaying);
        m_audioSourcePool.Set(audioSoruce);
    }

    IEnumerator Coroutine_FadeOut(AudioSource src, float fadeOutTime)
    {
        float start = 1f;
        float t = 0f;
        
        while(t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            var k = (t / fadeOutTime);
            k = k * k * (3 - 2 * k);
            src.volume = Mathf.Lerp(start, 0f, k);
            yield return null;
        }

        src.Stop();
        src.volume = start;
    }

    IEnumerator Coroutine_FadeIn(AudioSource src, float fadeInTime)
    {
        float target = 1f;
        src.volume = 0f;
        src.Play();

        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            var k = (t / fadeInTime);
            k = k * k * (3-2*k);
            src.volume = Mathf.Lerp(0f, target, k);
            yield return null;
        }
    }

    IEnumerator Coroutine_FadeSwap(AudioSource src, AudioClip next, float fadeTime)
    {
        yield return Coroutine_FadeOut(src, fadeTime);

        m_backgroundAudioSource.clip = next;
        m_backgroundAudioSource.loop = true;

        yield return Coroutine_FadeIn(src, fadeTime);
    }
}
