using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : SingletonDontDestroy<MenuManager>
{
    [SerializeField]
    TextMeshProUGUI m_timer;


    Dictionary<BtnType, GameObject> m_uiDic;
    Dictionary<PanelType, GameObject> m_panelDic;

    bool m_running;
    double m_elapsed;
    public double Elapsed => m_elapsed;
    protected override void Awake()
    {
        base.Awake();
        
        m_uiDic = new Dictionary<BtnType, GameObject>();
        m_panelDic = new Dictionary<PanelType, GameObject>();

        m_panelDic[PanelType.Audio]     =    GameObject.FindGameObjectWithTag("Panel_Audio");
        m_panelDic[PanelType.Graphics]  =    GameObject.FindGameObjectWithTag("Panel_Graphics");
        m_panelDic[PanelType.Controls]  =    GameObject.FindGameObjectWithTag("Panel_Controls");
        m_panelDic[PanelType.Menu]      =    GameObject.FindGameObjectWithTag("Panel_Menu");

        m_uiDic[BtnType.Audio]          =    GameObject.FindGameObjectWithTag("Btn_Audio");
        m_uiDic[BtnType.Graphics]       =    GameObject.FindGameObjectWithTag("Btn_Graphics");
        m_uiDic[BtnType.Controls]       =    GameObject.FindGameObjectWithTag("Btn_Controls");
        m_uiDic[BtnType.Save]           =    GameObject.FindGameObjectWithTag("Btn_Save");
        m_uiDic[BtnType.Cancel]         =    GameObject.FindGameObjectWithTag("Btn_Cancel");
        m_uiDic[BtnType.SaveAndExit]    =    GameObject.FindGameObjectWithTag("Btn_SaveAndExit");

        m_timer                         = GameObject.FindGameObjectWithTag("Text_Timer").GetComponent<TextMeshProUGUI>();
        m_running                       = false;

        m_uiDic[BtnType.Audio].GetComponent<Button>().onClick.AddListener(OnAudio);
        m_uiDic[BtnType.Graphics].GetComponent<Button>().onClick.AddListener(OnGraphic);
        m_uiDic[BtnType.Controls].GetComponent<Button>().onClick.AddListener(OnControls);
        m_uiDic[BtnType.Save].GetComponent<Button>().onClick.AddListener(OnSave);
        m_uiDic[BtnType.Cancel].GetComponent<Button>().onClick.AddListener(OnCancel);
        m_uiDic[BtnType.SaveAndExit].GetComponent<Button>().onClick.AddListener(OnSaveAndExit);

        
    }

    protected override void Start()
    {
        base.Start();
        InitMenu(SceneType.Start);
        m_panelDic[PanelType.Controls].SetActive(false);
        m_panelDic[PanelType.Graphics].SetActive(false);
        GameManager.Instance.Toogle();
    }

    void Update()
    {
        if(m_running)
        {
            m_elapsed += Time.unscaledDeltaTime;
            
            int sec = (int)Math.Floor(m_elapsed);
            int h = sec / 3600;
            int m = (sec % 3600) / 60;
            int s = sec % 60;

            m_timer.SetText("{0:00}:{1:00}:{2:00}", h, m, s);
        }
    }



    public void InitMenu(SceneType sceneType)
    {
        if (sceneType == SceneType.Start)
        {
            m_uiDic[BtnType.Continue]   = GameObject.FindGameObjectWithTag("Btn_Continue");
            m_uiDic[BtnType.NewGame]    = GameObject.FindGameObjectWithTag("Btn_NewGame");
            m_uiDic[BtnType.Exit]       = GameObject.FindGameObjectWithTag("Btn_Exit");
            m_uiDic[BtnType.Option]     = GameObject.FindGameObjectWithTag("Btn_Option");
            m_running                   = false;
            
            m_uiDic[BtnType.NewGame].GetComponent<Button>().onClick.AddListener(OnNewGame);
            m_uiDic[BtnType.Exit].GetComponent<Button>().onClick.AddListener(OnExit);
            m_uiDic[BtnType.Option].GetComponent<Button>().onClick.AddListener(OnOption);

            m_uiDic[BtnType.SaveAndExit].SetActive(false);

            if(!GameProgressService.Instance.HasSave)
            {
                m_uiDic[BtnType.Continue].gameObject.SetActive(false);
            }
            else if(GameProgressService.Instance.HasSave)
            {
                m_uiDic[BtnType.Continue].GetComponent<Button>().onClick.AddListener(OnContinue);
            }
        }
        else if (sceneType == SceneType.Main)
        {
            m_uiDic[BtnType.SaveAndExit].SetActive(true);
            m_running = true;
            m_elapsed = 0f;
        }
    }

    public void OnContinue()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        GameProgressService.Instance.LoadProgress();
    }
    public void OnNewGame()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        GameProgressService.Instance.NewGame();
    }
    public void OnExit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnOption()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        GameManager.Instance.Toogle();
    }
    public void OnAudio()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        if (m_panelDic[PanelType.Graphics].activeSelf) m_panelDic[PanelType.Graphics].SetActive(false);
        if (m_panelDic[PanelType.Controls].activeSelf) m_panelDic[PanelType.Controls ].SetActive(false);
        m_panelDic[PanelType.Audio].SetActive(true);
    }
    public void OnGraphic()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        if (m_panelDic[PanelType.Audio].activeSelf) m_panelDic[PanelType.Audio].SetActive(false);
        if (m_panelDic[PanelType.Controls].activeSelf) m_panelDic[PanelType.Controls].SetActive(false);
        m_panelDic[PanelType.Graphics].SetActive(true);
    }
    public void OnControls()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        if (m_panelDic[PanelType.Graphics].activeSelf) m_panelDic[PanelType.Graphics].SetActive(false);
        if (m_panelDic[PanelType.Audio].activeSelf) m_panelDic[PanelType.Audio].SetActive(false);
        m_panelDic[PanelType.Controls].SetActive(true);
    }
    public void OnSave()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        SettingService.Instance.SaveSetting();
        GameManager.Instance.Toogle();
    }

    public void OnCancel()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        SettingService.Instance.LoadSetting();
        GameManager.Instance.Toogle();
    }

    public void OnSaveAndExit()
    {
        SoundManager.Instance.PlaySFX("ButtonSFX");
        SettingService.Instance.SaveSetting();
        GameManager.Instance.Toogle();
        GameProgressService.Instance.SaveAndExitToTitle();
    }

    public void SetElapsed(double elapsed)
    {
        m_elapsed = elapsed;
    }
}

public enum BtnType
{
    None = -1,
    Continue,
    NewGame,
    Exit,
    Option,
    Audio,
    Graphics,
    Controls,
    Save,
    Cancel,
    SaveAndExit,
    Max
}

public enum PanelType
{
    None = -1,
    Audio,
    Graphics,
    Controls,
    Menu,
    Max
}