using UnityEngine;

public static class DefaultSetting
{
    public static GraphicSettingData Griphic => new GraphicSettingData()
    {
        m_resolutionIndex = Panel_Graphics.Instance.PopulateResolutions(),
        m_fullScreenModeIndex = 0,
        m_frameRateIndex = 0,
        m_vSync = false
    };

    public static AudioSettingData Audio => new AudioSettingData()
    {
        m_masterVolumeSliderValue = 0.8f,
        m_BGMVolumeSliderValue = 0.8f,
        m_SFXVolumeSliderValue = 0.8f
    };

}
