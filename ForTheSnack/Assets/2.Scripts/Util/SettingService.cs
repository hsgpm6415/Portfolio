using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class SettingService : SingletonDontDestroy<SettingService>
{
    [SerializeField] List<CanvasScaler> m_canvasScalers = new();
    [SerializeField] List<RectTransform> m_uiRoots = new();

    const string GRAPHIC_KEY = "GraphicSettings";
    const string AUDIO_KEY = "AudioSettings";

    public event Action<GraphicSettingData> OnGraphicSettingsLoaded = _ => { };
    public event Action<AudioSettingData> OnAudioSettingsLoaded = _ => { };

    public List<Action<AudioSettingData>> m_pending = new List<Action<AudioSettingData>>();


    protected override void Awake()
    {
        //PlayerPrefs.DeleteAll();
        base.Awake();

        RebindCanvas();

        foreach (var item in m_pending)
        {
            var data = LoadAudioSettings();
            item(data);
        }
        m_pending.ForEach(h => OnAudioSettingsLoaded += h);
        m_pending.Clear();
    }

    protected override void OnEnable() => base.OnEnable();

    protected override void Start()
    {
        base.Start();
        LoadSetting();
    }

    public void InitSetting()
    {
        StartCoroutine(Coroutine_AfterLoadScene());
    }

    public void ApplyResolution(Resolution resolution)
    {
        StartCoroutine(Coroutine_Resolution(resolution));
    }

    public void ApplyFullScreen(FullScreenMode mode)
    {
        Screen.fullScreenMode = mode;
    }

    public void ApplyFrameRate(int selected)
    {
        Application.targetFrameRate = selected == 0 ? -1 : selected;
    }

    public void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
        if (on) Application.targetFrameRate = -1;
    }

    public void SaveSetting()
    {
        var graphicSettingData = Panel_Graphics.Instance.GetGraphicSetting();
        var audioSettingData = Panel_Audio.Instance.GetAudioSetting();

        string jsonGraphicSetting = JsonUtility.ToJson(graphicSettingData);
        string jsonAudioSetting = JsonUtility.ToJson(audioSettingData);

        
        PlayerPrefs.SetString(GRAPHIC_KEY, jsonGraphicSetting);
        PlayerPrefs.SetString(AUDIO_KEY, jsonAudioSetting);
        PlayerPrefs.Save();
    }

    
    public void LoadSetting()
    {
        var graphicSettingData = LoadGraphicSettings();
        var audioSettingData = LoadAudioSettings();
        OnGraphicSettingsLoaded?.Invoke(graphicSettingData);
        OnAudioSettingsLoaded?.Invoke(audioSettingData);
    }

    public void RegisterAudio(Action<AudioSettingData> handler)
    {
        if(Instance != this)
        {
            m_pending.Add(handler);
        }
        else
        {
            var audioSettingData = LoadAudioSettings();
            handler(audioSettingData);
            OnAudioSettingsLoaded += handler;
        }
    }


    public GraphicSettingData LoadGraphicSettings()
    {
        GraphicSettingData data;
        if(PlayerPrefs.HasKey(GRAPHIC_KEY))
        {
            string json = PlayerPrefs.GetString(GRAPHIC_KEY);
            data = JsonUtility.FromJson<GraphicSettingData>(json);
        }
        else
        {
            data = DefaultSetting.Griphic;
        }
        return data;
    }

    public AudioSettingData LoadAudioSettings()
    {
        AudioSettingData data;

        if (PlayerPrefs.HasKey(AUDIO_KEY))
        {
            string json = PlayerPrefs.GetString(AUDIO_KEY);
            data = JsonUtility.FromJson<AudioSettingData>(json);
        }
        else
        {
            data = DefaultSetting.Audio;
        }
        return data;
    }

    public IEnumerator Coroutine_Resolution(Resolution resolution)
    {
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
        yield return new WaitForEndOfFrame();
        RebuildAllLayouts();
    }

    IEnumerator Coroutine_AfterLoadScene()
    {
        yield return null;

        RebindCanvas();
        LoadSetting();
        RebuildAllLayouts();
    }

    void RebindCanvas()
    {
        m_canvasScalers.Clear();
        m_uiRoots.Clear();

        var scalers = FindObjectsOfType<CanvasScaler>(true);

        foreach (CanvasScaler sc in scalers)
        {
            m_canvasScalers.Add(sc);
            m_uiRoots.Add(sc.GetComponent<RectTransform>());
        }
    }

    void RebuildAllLayouts()
    {
        Canvas.ForceUpdateCanvases();

        foreach(RectTransform rt in m_uiRoots)
        {
            if(rt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }
    }
}