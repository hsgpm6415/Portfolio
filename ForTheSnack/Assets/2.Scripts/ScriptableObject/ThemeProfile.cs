using UnityEngine;

[CreateAssetMenu(menuName = "Audio/ScriptableObject")]
public class ThemeProfile : ScriptableObject
{
    public ThemeType m_theme;
    public AudioClip m_backgroundMusic;
    public float m_musicFade;
}
public enum ThemeType
{
    None = -1,
    House,
    Trip,
    Forest,
    Desert,
    Ice,
    Industry,
    Max
}