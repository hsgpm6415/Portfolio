using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using UnityEngine.Rendering;


public class Panel_Graphics : SingletonDontDestroy<Panel_Graphics>
{
    [Header("Resolution Settings")]
    [SerializeField] Dropdown m_resolutionDropdown;
    [SerializeField] Dropdown m_screenModeDropDown;

    [Header("Quality Settings")]
    [SerializeField] Dropdown m_frameRateDropdown;
    [SerializeField] Toggle m_vsyncToggle;

    readonly Vector2Int[] m_resolutions = new[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(2560, 1080),
        new Vector2Int(3440, 1440),
        new Vector2Int(3840, 2160),
    };

    static readonly Dictionary<Vector2Int, int[]> m_commonRefreshRates = new Dictionary<Vector2Int, int[]>
    {
        { new Vector2Int(1280,720), new[]{ 60 } },
        { new Vector2Int(1366,768), new[]{ 60 } },
        { new Vector2Int(1600,900), new[]{ 60, 75 } },
        { new Vector2Int(1920,1080),new[]{ 60,75,120,144,240 } },
        { new Vector2Int(2560,1440),new[]{ 60,100,120,144,165,240 } },
        { new Vector2Int(2560,1080),new[]{ 60,100,120,144 } },
        { new Vector2Int(3440,1440),new[]{ 60,100,120,144 } },
        { new Vector2Int(3840,2160),new[]{ 30,60,120 } },
    };

    readonly public FullScreenMode[] m_fullScreenModes = {  FullScreenMode.ExclusiveFullScreen,
                                                            FullScreenMode.FullScreenWindow,
                                                            FullScreenMode.Windowed };

    readonly public List<int> m_frameRates = new List<int> { 60, 120, 144, 0 };


    protected override void Awake()
    {
        base.Awake();

        m_resolutionDropdown = transform.GetChild((int)GraphicsUIType.Resolution).GetChild(0).GetComponent<Dropdown>();
        m_screenModeDropDown = transform.GetChild((int)GraphicsUIType.ScreenMode).GetChild(0).GetComponent<Dropdown>();
        m_frameRateDropdown = transform.GetChild((int)GraphicsUIType.FrameRate).GetChild(0).GetComponent<Dropdown>();
        m_vsyncToggle = transform.GetChild((int)GraphicsUIType.VSync).GetChild(0).GetComponent<Toggle>();

        m_vsyncToggle.isOn = false;
        OnVSyncToggle(false);
        UpdateFrameRateInteractable(false);

        PopulateResolutions();
        PopulateScreenMode();
        PopulateFrameRate();

        OnFrameRateChanged(2);
        OnScreenModeChanged(0);


        m_resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        m_screenModeDropDown.onValueChanged.AddListener(OnScreenModeChanged);
        m_frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        m_vsyncToggle.onValueChanged.AddListener(OnVSyncToggle);
        
    }

    protected override void Start()
    {
        base.Start();

    }
    protected override void OnEnable()
    {
        base.OnEnable();

        SettingService.Instance.OnGraphicSettingsLoaded += ApplySettings;
    }

    #region [Private Func]


    void OnApplicationQuit()
    {
        SettingService.Instance.OnGraphicSettingsLoaded -= ApplySettings;
    }

    public int PopulateResolutions()
    {
        var filtered = GetFilteredResolutionOptions();
        int currentIndex = 0;
        List<string> options = new List<string>();

        for (int i = 0; i < filtered.Count; i++)
        {
            Resolution r = filtered.ElementAt(i);
            options.Add($"{r.width} X {r.height} @ {r.refreshRateRatio}");

            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height &&
                r.refreshRateRatio.numerator == Screen.currentResolution.refreshRateRatio.numerator)
                currentIndex = i;

        }

        m_resolutionDropdown.ClearOptions();
        m_resolutionDropdown.AddOptions(options);
        m_resolutionDropdown.value = currentIndex;
        m_resolutionDropdown.RefreshShownValue();
        return currentIndex;
    }

    void PopulateScreenMode()
    {
        var options = new List<string>();
        int currentIndex = 0;

        options.Add("Full Screen");
        options.Add("Borderless");
        options.Add("Windowed");

        m_screenModeDropDown.ClearOptions();
        m_screenModeDropDown.AddOptions(options);
        m_screenModeDropDown.value = currentIndex;
        m_screenModeDropDown.RefreshShownValue();

    }

    void PopulateFrameRate()
    {
        var options = new List<string>();

        int currentIndex = 0;
        for (int i = 0; i < m_frameRates.Count; i++)
        {
            var rate = m_frameRates[i];
            options.Add(rate == 0 ? "Uncapped" : $"{rate} FPS");

            if (rate == Application.targetFrameRate)
                currentIndex = i;
        }



        m_frameRateDropdown.ClearOptions();
        m_frameRateDropdown.AddOptions(options);
        m_frameRateDropdown.value = currentIndex;
        m_frameRateDropdown.RefreshShownValue();

    }

    /// <summary>
    /// VSync가 켜져 있으면 FrameRate 드롭다운을 비활성화한다.
    /// </summary>
    void UpdateFrameRateInteractable(bool vsyncOn)
    {
        m_frameRateDropdown.interactable = !vsyncOn;
    }

    List<Resolution> GetFilteredResolutionOptions()
    {
        Resolution[] supported = Screen.resolutions;
        List<Resolution> filtered = new List<Resolution>();

        foreach (Vector2Int com in m_resolutions)
        {
            if (!m_commonRefreshRates.TryGetValue(com, out int[] hzList))
                continue;

            foreach (int hz in hzList)
            {
                Resolution match = supported.FirstOrDefault(r =>
                    r.width == com.x &&
                    r.height == com.y &&
                    r.refreshRateRatio.numerator == hz);

                if (match.width != 0)
                {
                    filtered.Add(match);
                }
            }
        }

        return filtered;
    }


    #region [UI_Event]
    void OnResolutionChanged(int index)
    {
        List<Resolution> options = GetFilteredResolutionOptions();
        Resolution resolution = options.ElementAt(index);
        SettingService.Instance.ApplyResolution(resolution);
    }
    void OnScreenModeChanged(int index)
    {
        var screenMode = m_fullScreenModes[index];
        SettingService.Instance.ApplyFullScreen(screenMode);
    }
    void OnFrameRateChanged(int index)
    {
        int selected = m_frameRates[index];
        SettingService.Instance.ApplyFrameRate(selected);
    }
    void OnVSyncToggle(bool isVSyncOn)
    {
        UpdateFrameRateInteractable(isVSyncOn);
        SettingService.Instance.ApplyVSync(isVSyncOn);
    }
    #endregion


    #endregion


    #region [public Func]

    public GraphicSettingData GetGraphicSetting()
    {
        var data = new GraphicSettingData();

        data.m_resolutionIndex = m_resolutionDropdown.value;
        data.m_fullScreenModeIndex = m_screenModeDropDown.value;
        data.m_frameRateIndex = m_frameRateDropdown.value;
        data.m_vSync = m_vsyncToggle.enabled;

        return data;
    }

    public void ApplySettings(GraphicSettingData data)
    {
        var curValue = m_resolutionDropdown.value;

        m_resolutionDropdown.value = data.m_resolutionIndex;
        m_screenModeDropDown.value = data.m_fullScreenModeIndex;
        m_frameRateDropdown.value = data.m_frameRateIndex;
        m_vsyncToggle.enabled = data.m_vSync;

        //기존 해상도와 같아도 실행
        if(curValue == data.m_resolutionIndex)
        {
            OnResolutionChanged(m_resolutionDropdown.value);
            OnScreenModeChanged(m_screenModeDropDown.value);
            OnFrameRateChanged(m_frameRateDropdown.value);
            OnVSyncToggle(m_vsyncToggle.enabled);
        }
    }
    #endregion


    enum GraphicsUIType
    {
        None = -1,
        Resolution,
        ScreenMode,
        FrameRate,
        VSync,
        Max
    }
}

