using System.Text;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(400)]
public class Panel_Audio : SingletonDontDestroy<Panel_Audio>
{
    [Header("Volume Slider")]
    [SerializeField] Slider m_masterVolumeSlider;
    [SerializeField] Slider m_BGMVolumeSilider;
    [SerializeField] Slider m_SFXVolumeSilider;
    
    [Header("Volume Scale Text")]
    [SerializeField] Text m_masterVolumeText;
    [SerializeField] Text m_BGMVolumeText;
    [SerializeField] Text m_SFXVolumeText;

    [Header("SFX Check Button")]
    [SerializeField] Button m_SFXCheckBtn;

    StringBuilder m_sb;

    protected override void Awake()
    {
        base.Awake();

        m_sb = new StringBuilder(6);

        m_masterVolumeSlider = transform.GetChild((int)AudioUIType.MasterVolume).GetComponentInChildren<Slider>();
        m_masterVolumeText = transform.GetChild((int)AudioUIType.MasterVolume).GetComponentsInChildren<Text>()[1];

        m_BGMVolumeSilider = transform.GetChild((int)AudioUIType.BGMVolume).GetComponentInChildren<Slider>();
        m_BGMVolumeText = transform.GetChild((int)AudioUIType.BGMVolume).GetComponentsInChildren<Text>()[1];

        m_SFXVolumeSilider = transform.GetChild((int)AudioUIType.SFXVolume).GetComponentInChildren<Slider>();
        m_SFXVolumeText = transform.GetChild((int)AudioUIType.SFXVolume).GetComponentsInChildren<Text>()[1];

        m_SFXCheckBtn = transform.GetChild((int)AudioUIType.SFXCheck).GetComponentInChildren<Button>();

        m_masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        m_BGMVolumeSilider.onValueChanged.AddListener(OnBGMVolumeChanged);
        m_SFXVolumeSilider.onValueChanged.AddListener(OnSFXVolumeChanged);
        m_SFXCheckBtn.onClick.AddListener(OnSFXCheck);
    }

    protected override void Start()
    {
        base.Start();
        SettingService.Instance.RegisterAudio(ApplySettings);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    #region [Private Func]
    void OnApplicationQuit()
    {
        SettingService.Instance.OnAudioSettingsLoaded -= ApplySettings;
    }

    void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
        m_masterVolumeText.text = CalculatePercent(value);
    }

    void OnBGMVolumeChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
        m_BGMVolumeText.text = CalculatePercent(value);
    }

    void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
        m_SFXVolumeText.text = CalculatePercent(value);
    }

    void OnSFXCheck()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
    }

    string CalculatePercent(float value)
    {
        var percent = Mathf.RoundToInt(value * 100f);
        m_sb.Clear();
        m_sb.Append(percent.ToString());
        return m_sb.ToString();
    }

    #endregion

    #region [Public Func]
    public AudioSettingData GetAudioSetting()
    {
        var data = new AudioSettingData();
        data.m_masterVolumeSliderValue = m_masterVolumeSlider.value;
        data.m_BGMVolumeSliderValue = m_BGMVolumeSilider.value;
        data.m_SFXVolumeSliderValue = m_SFXVolumeSilider.value;
        return data;
    }

    public void ApplySettings(AudioSettingData data)
    {
        m_masterVolumeSlider.value = data.m_masterVolumeSliderValue;
        m_BGMVolumeSilider.value = data.m_BGMVolumeSliderValue;
        m_SFXVolumeSilider.value = data.m_SFXVolumeSliderValue;
    }
    #endregion
    enum AudioUIType
    {
        None = -1,
        MasterVolume,
        BGMVolume,
        SFXVolume,
        SFXCheck,
        Max
    }
}
