using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroPlayer : MonoBehaviour
{
    [SerializeField]
    ThemeProfile m_profile;

    void Start()
    {
        SoundManager.Instance.PlayBGM(m_profile);
    }
}
