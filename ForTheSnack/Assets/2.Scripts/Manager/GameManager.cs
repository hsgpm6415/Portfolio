using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonDontDestroy<GameManager>
{
    [SerializeField] bool m_isPause;
    #region
    public Transform m_testSpawnPosition;
    #endregion

    [SerializeField] Camera m_camera;
    [SerializeField] CatController m_cat;

    SceneType m_currentSceneType;

    public event Action<bool> OnPauseChanged = _ => { };
    public bool IsPause { get { return m_isPause; } }
    public Camera MainCamera { get { return m_camera; } }
    public CatController Cat { get { return m_cat; } }
    protected override void Awake()
    {
        base.Awake();
        m_isPause = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_cat.transform.position = m_testSpawnPosition.position;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(m_isPause) MenuManager.Instance.OnCancel();
            else Toogle();
        }
    }


    public void Toogle()
    {
        Set(!m_isPause);
    }

    void Set(bool isPause)
    {
        if (m_isPause == isPause) return;
        m_isPause = isPause;

        if(m_currentSceneType == SceneType.Main)
        {
            Time.timeScale = isPause ? 0.0f : 1.0f;
            Cursor.visible = isPause;
            Cursor.lockState = isPause ? CursorLockMode.None : CursorLockMode.Locked;
        }
        OnPauseChanged?.Invoke(isPause);
    }

    public void Init(SceneType sceneType)
    {
        m_currentSceneType = sceneType;

        switch (sceneType)
        {
            case SceneType.Start:
                {
                    m_camera = Camera.main;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                break;
            case SceneType.Main:
                {
                    m_cat = GameObject.FindWithTag("Cat").GetComponent<CatController>();
                    m_camera = Camera.main;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
#if UNITY_EDITOR
                    m_testSpawnPosition = GameObject.FindWithTag("TestSpawnPosition").gameObject.transform;
#endif
                }
                break;
            default:
                break;
        }

        
    }

}
